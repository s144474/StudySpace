using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudySpace.Exceptions;
using StudySpace.Infrastructure;
using StudySpace.Models;
using StudySpace.Services;

namespace StudySpace.Pages.Admin;

[Authorize(Roles = "Admin")]
public class DashboardModel : AppPageModel
{
    private readonly SpotService _spots;
    private readonly ReservationService _reservations;

    public DashboardModel(SpotService spots, ReservationService reservations)
    {
        _spots = spots;
        _reservations = reservations;
    }

    public int TotalSpots { get; private set; }
    public int ActiveSpots { get; private set; }
    public int ReservationsToday { get; private set; }
    public int UpcomingReservations { get; private set; }
    public List<SpotCardVm> Spots { get; private set; } = new();
    public List<StatusReport> Reports { get; private set; } = new();
    public List<ReservationRow> Rows { get; private set; } = new();
    public List<(string Building, int Reservations, int Spots)> Usage { get; private set; } = new();

    public string SpotNameOf(int spotId) => _spots.SpotName(spotId);

    public void OnGet() => Load();

    public IActionResult OnPostToggle(int id)
    {
        try
        {
            var spot = _spots.Get(id);
            _spots.SetActive(id, !spot.IsActive);
            FlashSuccess($"{spot.SpotName} is now {(spot.IsActive ? "enabled" : "disabled")}.");
        }
        catch (AppException ex) { FlashError(ex.Message); }
        return RedirectToPage(null, null, null, "spots");
    }

    public IActionResult OnPostHideReport(int id)
    {
        try
        {
            _spots.HideReport(id);
            FlashSuccess($"Report #{id} was hidden and the live status was recalculated.");
        }
        catch (AppException ex) { FlashError(ex.Message); }
        return RedirectToPage(null, null, null, "reports");
    }

    public IActionResult OnPostCancel(int id)
    {
        try
        {
            _reservations.Cancel(id, CurrentUserId, isAdmin: true);
            FlashSuccess($"Reservation #{id} was cancelled.");
        }
        catch (AppException ex) { FlashError(ex.Message); }
        return RedirectToPage(null, null, null, "reservations");
    }

    private void Load()
    {
        Spots = _spots.Search(new SpotFilter(), includeInactive: true).Cards;
        TotalSpots = Spots.Count;
        ActiveSpots = Spots.Count(c => c.Spot.IsActive);
        Reports = _spots.RecentReports(15);
        Rows = _reservations.GetRows();
        ReservationsToday = Rows.Count(r => r.Reservation.StartTime.Date == DateTime.Today && r.Reservation.Status != ReservationStatus.Cancelled);
        UpcomingReservations = Rows.Count(r => r.Reservation.IsUpcoming(DateTime.Now));
        Usage = _reservations.UsageByBuilding();
    }
}
