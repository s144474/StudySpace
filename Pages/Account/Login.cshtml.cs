using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using StudySpace.Infrastructure;
using StudySpace.Services;

namespace StudySpace.Pages.Account;

public class LoginModel : AppPageModel
{
    private readonly UserService _users;
    public LoginModel(UserService users) => _users = users;

    [BindProperty] public InputModel Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Enter your university email address.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Keep me signed in")]
        public bool RememberMe { get; set; }
    }

    public IActionResult OnGet() =>
        User.Identity?.IsAuthenticated == true ? RedirectToPage("/Spots/Index") : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = _users.Authenticate(Input.Email, Input.Password);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "The email or password is incorrect.");
            return Page();
        }

        await SignInUserAsync(user, Input.RememberMe);
        FlashSuccess($"Welcome back, {user.FirstName()}.");
        return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl! : "/Spots");
    }
}
