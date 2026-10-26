using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using StudySpace.Infrastructure;

namespace StudySpace.Pages.Account;

public class LogoutModel : AppPageModel
{
    public IActionResult OnGet() => RedirectToPage("/Spots/Index");

    public async Task<IActionResult> OnPostAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        FlashSuccess("You have been logged out.");
        return RedirectToPage("/Spots/Index");
    }
}
