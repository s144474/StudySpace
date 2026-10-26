using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudySpace.Exceptions;
using StudySpace.Infrastructure;
using StudySpace.Models;
using StudySpace.Services;

namespace StudySpace.Pages.Admin;

[Authorize(Roles = "Admin")]
public class EditSpotModel : AppPageModel
{
    private readonly SpotService _spots;
    public EditSpotModel(SpotService spots) => _spots = spots;

    [BindProperty] public SpotInput Input { get; set; } = new();
    public SelectList BuildingOptions { get; private set; } = null!;
    public bool IsNew => Input.SpotID == 0;

    public class SpotInput : IValidatableObject
    {
        public int SpotID { get; set; }

        [Required(ErrorMessage = "Please select a building.")]
        [Display(Name = "Building")]
        public int? BuildingID { get; set; }

        [Required(ErrorMessage = "Spot name is required.")]
        [StringLength(60, MinimumLength = 3, ErrorMessage = "Spot name must be between 3 and 60 characters.")]
        [Display(Name = "Spot name")]
        public string SpotName { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Room number cannot be longer than 20 characters.")]
        [RegularExpression(@"^[A-Za-z0-9\-\s]*$", ErrorMessage = "Room number may contain only letters, digits, spaces and hyphens.")]
        [Display(Name = "Room / zone number")]
        public string? RoomNumber { get; set; }

        [Required(ErrorMessage = "Seat capacity is required.")]
        [Range(1, 500, ErrorMessage = "Capacity must be between 1 and 500 seats.")]
        [Display(Name = "Maximum capacity")]
        public int? MaxCapacity { get; set; }

        [Display(Name = "Noise policy")]
        public NoiseCategory NoiseCategory { get; set; } = NoiseCategory.Quiet;

        [Display(Name = "Has power outlets")]
        public bool HasPowerOutlets { get; set; } = true;

        [Display(Name = "Requires reservation")]
        public bool IsBookable { get; set; }

        [Display(Name = "Enabled")]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Opening time is required.")]
        [Display(Name = "Opens at")]
        public TimeOnly? OpeningTime { get; set; } = new(8, 0);

        [Required(ErrorMessage = "Closing time is required.")]
        [Display(Name = "Closes at")]
        public TimeOnly? ClosingTime { get; set; } = new(22, 0);

        [StringLength(250, ErrorMessage = "The notice cannot be longer than 250 characters.")]
        [Display(Name = "Maintenance notice")]
        public string? MaintenanceNotice { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (OpeningTime.HasValue && ClosingTime.HasValue && ClosingTime <= OpeningTime)
                yield return new ValidationResult("Closing time must be after opening time.", new[] { nameof(ClosingTime) });
        }
    }

    public IActionResult OnGet(int? id)
    {
        LoadOptions();
        if (id is null) return Page();

        var spot = _spots.Find(id.Value);
        if (spot is null) return NotFound();

        Input = new SpotInput
        {
            SpotID = spot.SpotID, BuildingID = spot.BuildingID, SpotName = spot.SpotName, RoomNumber = spot.RoomNumber,
            MaxCapacity = spot.MaxCapacity, NoiseCategory = spot.NoiseCategory, HasPowerOutlets = spot.HasPowerOutlets,
            IsBookable = spot.IsBookable, IsActive = spot.IsActive, OpeningTime = spot.OpeningTime,
            ClosingTime = spot.ClosingTime, MaintenanceNotice = spot.MaintenanceNotice
        };
        return Page();
    }

    public IActionResult OnPost(int? id)
    {
        LoadOptions();
        if (!ModelState.IsValid) return Page();

        try
        {
            var spot = new StudySpot(Input.SpotID, Input.BuildingID!.Value, Input.SpotName.Trim(),
                string.IsNullOrWhiteSpace(Input.RoomNumber) ? null : Input.RoomNumber.Trim(),
                Input.MaxCapacity!.Value, Input.NoiseCategory, Input.HasPowerOutlets, Input.IsBookable,
                Input.OpeningTime, Input.ClosingTime)
            {
                IsActive = Input.IsActive,
                MaintenanceNotice = Input.MaintenanceNotice
            };
            var saved = _spots.Save(spot);
            FlashSuccess($"{saved.SpotName} was {(Input.SpotID == 0 ? "created" : "updated")}.");
            return RedirectToPage("Dashboard", null, null, "spots");
        }
        catch (AppException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }

    private void LoadOptions() =>
        BuildingOptions = new SelectList(_spots.GetBuildings(), nameof(Building.BuildingID), nameof(Building.BuildingName));
}
