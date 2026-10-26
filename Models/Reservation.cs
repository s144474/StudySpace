using StudySpace.Exceptions;

namespace StudySpace.Models;

public class Reservation
{
    public int ReservationID { get; set; }
    public int SpotID { get; set; }
    public int UserID { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Reservation() { }

    public Reservation(int id, int spotId, int userId, DateTime start, DateTime end, string? notes = null)
    {
        if (end <= start) throw new BusinessRuleException("End time must be after the start time.");
        ReservationID = id; SpotID = spotId; UserID = userId;
        StartTime = start; EndTime = end; Notes = notes;
        Status = ReservationStatus.Confirmed;
    }

    public TimeSpan Duration => EndTime - StartTime;

    public bool OverlapsWith(DateTime start, DateTime end) =>
        Status == ReservationStatus.Confirmed && start < EndTime && end > StartTime;

    public bool IsUpcoming(DateTime now) => Status == ReservationStatus.Confirmed && StartTime > now;

    public void Cancel()
    {
        if (Status != ReservationStatus.Confirmed)
            throw new BusinessRuleException($"A {Status.ToString().ToLower()} reservation cannot be cancelled.");
        if (EndTime <= DateTime.Now)
            throw new BusinessRuleException("This reservation has already ended.");
        Status = ReservationStatus.Cancelled;
    }

    public void MarkCompleted()
    {
        if (Status == ReservationStatus.Confirmed && EndTime <= DateTime.Now)
            Status = ReservationStatus.Completed;
    }
}
