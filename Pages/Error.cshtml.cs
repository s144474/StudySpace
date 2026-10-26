using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudySpace.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? Code { get; set; }

    public string RequestId { get; private set; } = string.Empty;
    public string Heading { get; private set; } = "Something went wrong";
    public string Message { get; private set; } = "An unexpected error occurred on our side. Please try again in a moment.";

    public void OnGet() => Build();
    public void OnPost() => Build();

    private void Build()
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        switch (Code)
        {
            case 404:
                Heading = "Page not found";
                Message = "We could not find what you were looking for. The spot or page may have been removed.";
                break;
            case 403:
                Heading = "Access denied";
                Message = "You do not have permission to view this page.";
                break;
            case 400:
                Heading = "That request was not valid";
                Message = "Some of the information sent was missing or incorrect. Go back and check your input.";
                break;
        }
    }
}
