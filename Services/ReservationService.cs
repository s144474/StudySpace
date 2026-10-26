using StudySpace.Exceptions;
using StudySpace.Models;

namespace StudySpace.Services;

public class ReservationRow
{
    public Reservation Reservation { get; set; } = null!;
    public string SpotName { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}

public class ReservationService
{
    public const int MaxHours = 3;
    public const int MaxDaysAhead = 14;

    private readonly DataStore _store;
    public ReservationService(DataStore store) => _store = store;

    public Reservation Create(int spotId, int userId, DateTime start, DateTime end, string? notes)
    {
        lock (_store.SyncRoot)
        {
            var spot = _store.Spots.FirstOrDefault(s => s.SpotID == spotId)
                       ?? throw new NotFoundException($"Study spot #{spotId} was not found.");

            if (!spot.IsActive) throw new BusinessRuleException("This study spot is currently disabled and cannot be booked.");
            if (!spot.IsBookable) throw new BusinessRuleException("This spot is walk-in only and cannot be reserved.");
            if (end <= start) throw new BusinessRuleException("The end time must be after the start time.");
            if (start < DateTime.Now) throw new BusinessRuleException("Reservations must start in the future.");
            if (start > DateTime.Now.AddDays(MaxDaysAhead))
                throw new BusinessRuleException($"You can only book up to {MaxDaysAhead} days in advance.");
            if (start.Date != end.Date) throw new BusinessRuleException("A reservation must start and end on the same day.");
            if (end - start > TimeSpan.FromHours(MaxHours))
                throw new BusinessRuleException($"A single reservation cannot be longer than {MaxHours} hours.");
            if (!spot.IsOpenAt(TimeOnly.FromDateTime(start)) || TimeOnly.FromDateTime(end) > spot.ClosingTime)
                throw new BusinessRuleException($"{spot.SpotName} is open {spot.OpeningTime:HH\\:mm} - {spot.ClosingTime:HH\\:mm}. Choose a time inside these hours.");

            var spotClash = _store.Reservations.FirstOrDefault(r => r.SpotID == spotId && r.OverlapsWith(start, end));
            if (spotClash is not null)
                throw new BookingConflictException($"{spot.SpotName} is already booked from {spotClash.StartTime:HH:mm} to {spotClash.EndTime:HH:mm}. Please pick another time.");

            if (_store.Reservations.Any(r => r.UserID == userId && r.OverlapsWith(start, end)))
                throw new BookingConflictException("You already have another reservation during this time.");

            var reservation = new Reservation(_store.NextReservationId(), spotId, userId, start, end,
                string.IsNullOrWhiteSpace(notes) ? null : notes.Trim());
            _store.Reservations.Add(reservation);
            return reservation;
        }
    }

    public void Cancel(int reservationId, int userId, bool isAdmin)
    {
        lock (_store.SyncRoot)
        {
            var reservation = _store.Reservations.FirstOrDefault(r => r.ReservationID == reservationId)
                              ?? throw new NotFoundException($"Reservation #{reservationId} was not found.");
            if (!isAdmin && reservation.UserID != userId)
                throw new BusinessRuleException("You can only cancel your own reservations.");
            reservation.Cancel();
        }
    }

    public List<ReservationRow> GetRows(int? userId = null, ReservationStatus? status = null)
    {
        lock (_store.SyncRoot)
        {
            foreach (var r in _store.Reservations) r.MarkCompleted();

            return _store.Reservations
                .Where(r => (!userId.HasValue || r.UserID == userId) && (!status.HasValue || r.Status == status))
                .OrderByDescending(r => r.StartTime)
                .Select(r =>
                {
                    var spot = _store.Spots.FirstOrDefault(s => s.SpotID == r.SpotID);
                    var building = _store.Buildings.FirstOrDefault(b => b.BuildingID == spot?.BuildingID);
                    return new ReservationRow
                    {
                        Reservation = r,
                        SpotName = spot?.SpotName ?? "(deleted spot)",
                        BuildingName = building?.BuildingName ?? "-",
                        UserName = _store.Users.FirstOrDefault(u => u.UserID == r.UserID)?.FullName ?? "(unknown)"
                    };
                }).ToList();
        }
    }

    public List<Reservation> GetBookedSlots(int spotId, DateTime date)
    {
        lock (_store.SyncRoot)
            return _store.Reservations
                .Where(r => r.SpotID == spotId && r.Status == ReservationStatus.Confirmed && r.StartTime.Date == date.Date)
                .OrderBy(r => r.StartTime).ToList();
    }

    public List<(string Building, int Reservations, int Spots)> UsageByBuilding()
    {
        lock (_store.SyncRoot)
            return _store.Buildings.Select(b =>
            {
                var spotIds = _store.Spots.Where(s => s.BuildingID == b.BuildingID).Select(s => s.SpotID).ToList();
                var count = _store.Reservations.Count(r => spotIds.Contains(r.SpotID) && r.Status != ReservationStatus.Cancelled);
                return (b.BuildingName, count, spotIds.Count);
            }).OrderByDescending(x => x.Item2).ToList();
    }
}
