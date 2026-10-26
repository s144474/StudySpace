using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using StudySpace.Exceptions;
using StudySpace.Infrastructure;
using StudySpace.Models;
using StudySpace.Services;

namespace StudySpace.Pages.Spots;

public class DetailsModel : AppPageModel
{
    private readonly SpotService _spots;
    public DetailsModel(SpotService spots) => _spots = spots;

    public StudySpot Spot { get; private set; } = null!;
    public Building Building { get; private set; } = null!;
    public List<StatusReport> Reports { get; private set; } = new();

    [BindProperty]
    public ReportInput Input { get; set; } = new();

    public class ReportInput
    {
        [Required(ErrorMessage = "Please choose the current noise level.")]
        [Display(Name = "Noise level")]
        public NoiseCategory? Noise { get; set; }

        [Required(ErrorMessage = "Please choose how crowded the spot is.")]
        [Display(Name = "Occupancy")]
        public OccupancyLevel? Occupancy { get; set; }

        [StringLength(140, ErrorMessage = "The comment cannot be longer than 140 characters.")]
        public string? Comment { get; set; }
    }

    public IActionResult OnGet(int id) => Load(id) ? Page() : NotFound();

    public IActionResult OnGetStatus(int id) => Load(id) ? Partial("_StatusPanel", this) : NotFound();

    public IActionResult OnPostReport(int id)
    {
        if (User.Identity?.IsAuthenticated != true)
            return Fail(401, "Please log in with your university account to submit a report.");

        if (!ModelState.IsValid)
        {
            var message = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return Fail(400, message);
        }

        try
        {
            _spots.AddReport(id, CurrentUserId, CurrentUserName, Input.Noise!.Value, Input.Occupancy!.Value, Input.Comment);
            return new JsonResult(new { ok = true, message = "Thanks! Your update is now visible to other students." });
        }
        catch (NotFoundException ex) { return Fail(404, ex.Message); }
        catch (AppException ex) { return Fail(409, ex.Message); }
    }

    private bool Load(int id)
    {
        var spot = _spots.Find(id);
        if (spot is null) return false;
        Spot = spot;
        Building = _spots.GetBuilding(spot.BuildingID);
        Reports = _spots.GetReports(id);
        return true;
    }

    private static JsonResult Fail(int status, string message) =>
        new(new { ok = false, message }) { StatusCode = status };
}
