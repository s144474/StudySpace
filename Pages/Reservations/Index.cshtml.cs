using Microsoft.AspNetCore.Mvc;
using StudySpace.Exceptions;
using StudySpace.Infrastructure;
using StudySpace.Models;
using StudySpace.Services;

namespace StudySpace.Pages.Reservations;

public class IndexModel : AppPageModel
{
    private readonly ReservationService _reservations;
    public IndexModel(ReservationService reservations) => _reservations = reservations;

    [BindProperty(SupportsGet = true)]
    public ReservationStatus? Status { get; set; }

    public List<ReservationRow> Rows { get; private set; } = new();

    public void OnGet() => Rows = _reservations.GetRows(CurrentUserId, Status);

    public IActionResult OnPostCancel(int id)
    {
        try
        {
            _reservations.Cancel(id, CurrentUserId, IsAdmin);
            FlashSuccess($"Reservation #{id} was cancelled.");
        }
        catch (AppException ex)
        {
            FlashError(ex.Message);
        }
        return RedirectToPage();
    }
}
