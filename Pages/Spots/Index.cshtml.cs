using Microsoft.AspNetCore.Mvc;
using StudySpace.Infrastructure;
using StudySpace.Models;
using StudySpace.Services;

namespace StudySpace.Pages.Spots;

public class IndexModel : AppPageModel
{
    private readonly SpotService _spots;
    public IndexModel(SpotService spots) => _spots = spots;

    [BindProperty(SupportsGet = true)]
    public SpotFilter Filter { get; set; } = new();

    public SpotResults Results { get; private set; } = new();

    public void OnGet() => Load();

    public IActionResult OnGetCards()
    {
        Load();
        return Partial("_SpotCards", Results);
    }

    private void Load()
    {
        // ignore invalid seat counts
        if (Filter.MinSeats is < 1 or > 500) Filter.MinSeats = null;
        Results = _spots.Search(Filter);
    }
}
