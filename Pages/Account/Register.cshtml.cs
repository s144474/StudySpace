using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using StudySpace.Exceptions;
using StudySpace.Infrastructure;
using StudySpace.Services;

namespace StudySpace.Pages.Account;

public class RegisterModel : AppPageModel
{
    private readonly UserService _users;
    public RegisterModel(UserService users) => _users = users;

    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 100 characters.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "University email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [RegularExpression(@"^[A-Za-z0-9._%+\-]+@(student\.)?squ\.edu\.om$",
            ErrorMessage = "Use your university email (ending in squ.edu.om).")]
        [Display(Name = "University email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(64, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 64 characters.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one letter and one number.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public IActionResult OnGet() =>
        User.Identity?.IsAuthenticated == true ? RedirectToPage("/Spots/Index") : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try
        {
            var user = _users.Register(Input.FullName, Input.Email, Input.Password);
            await SignInUserAsync(user, false);
            FlashSuccess($"Welcome to StudySpace, {user.FirstName()}!");
            return RedirectToPage("/Spots/Index");
        }
        catch (BusinessRuleException ex)
        {
            ModelState.AddModelError(nameof(Input) + "." + nameof(Input.Email), ex.Message);
            return Page();
        }
    }
}
