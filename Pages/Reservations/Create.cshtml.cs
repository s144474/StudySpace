using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudySpace.Exceptions;
using StudySpace.Infrastructure;
using StudySpace.Services;

namespace StudySpace.Pages.Reservations;

public class CreateModel : AppPageModel
{
    private readonly SpotService _spots;
    private readonly ReservationService _reservations;

    public CreateModel(SpotService spots, ReservationService reservations)
    {
        _spots = spots;
        _reservations = reservations;
    }

    [BindProperty(SupportsGet = true)]
    public BookingInput Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int? SpotId { get; set; }

    public List<SelectListItem> SpotOptions { get; private set; } = new();

    public class BookingInput : IValidatableObject
    {
        [Required(ErrorMessage = "Please select a study room.")]
        [Display(Name = "Study room")]
        public int? SpotId { get; set; }

        [Required(ErrorMessage = "Please pick a date.")]
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        [Display(Name = "Start time")]
        public TimeOnly? StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        [Display(Name = "End time")]
        public TimeOnly? EndTime { get; set; }

        [StringLength(200, ErrorMessage = "Notes cannot be longer than 200 characters.")]
        public string? Notes { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the booking policy to continue.")]
        [Display(Name = "Booking policy")]
        public bool AcceptPolicy { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (StartTime.HasValue && EndTime.HasValue && EndTime <= StartTime)
                yield return new ValidationResult("The end time must be after the start time.", new[] { nameof(EndTime) });

            if (Date.HasValue && Date.Value.Date < DateTime.Today)
                yield return new ValidationResult("The date cannot be in the past.", new[] { nameof(Date) });
        }
    }

    public void OnGet()
    {
        ModelState.Clear();
        if (SpotId.HasValue && !Input.SpotId.HasValue) Input.SpotId = SpotId;
        LoadOptions();
    }

    public IActionResult OnPost()
    {
        LoadOptions();
        if (!ModelState.IsValid) return Page();

        try
        {
            var start = Input.Date!.Value.Date + Input.StartTime!.Value.ToTimeSpan();
            var end = Input.Date!.Value.Date + Input.EndTime!.Value.ToTimeSpan();
            var reservation = _reservations.Create(Input.SpotId!.Value, CurrentUserId, start, end, Input.Notes);

            FlashSuccess($"Reservation #{reservation.ReservationID} confirmed for {start:ddd d MMM}, {start:HH:mm} to {end:HH:mm}.");
            return RedirectToPage("Index");
        }
        catch (AppException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }

    public IActionResult OnGetAvailability(int spotId, DateTime date)
    {
        var spot = _spots.Find(spotId);
        if (spot is null) return new JsonResult(new { ok = false, message = "Room not found." }) { StatusCode = 404 };

        var booked = _reservations.GetBookedSlots(spotId, date)
            .Select(r => new { start = r.StartTime.ToString("HH:mm"), end = r.EndTime.ToString("HH:mm") });
        return new JsonResult(new
        {
            ok = true,
            hours = $"{spot.OpeningTime:HH\\:mm} to {spot.ClosingTime:HH\\:mm}",
            booked
        });
    }

    private void LoadOptions()
    {
        var groups = new Dictionary<string, SelectListGroup>();
        SpotOptions = _spots.Search(new Models.SpotFilter { BookableOnly = true }).Cards
            .Select(c =>
            {
                if (!groups.TryGetValue(c.BuildingName, out var g))
                    groups[c.BuildingName] = g = new SelectListGroup { Name = c.BuildingName };
                return new SelectListItem($"{c.Spot.SpotName} ({c.Spot.MaxCapacity} seats)", c.Spot.SpotID.ToString()) { Group = g };
            }).ToList();
    }
}
