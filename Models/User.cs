namespace StudySpace.Models;

public class User
{
    public int UserID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Student;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User() { }

    public User(int userId, string fullName, string email, string passwordHash, UserRole role = UserRole.Student)
    {
        UserID = userId;
        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    public bool IsAdmin() => Role == UserRole.Admin;

    public string FirstName() => FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? FullName;

    public bool MatchesEmail(string email) =>
        string.Equals(Email, email?.Trim(), StringComparison.OrdinalIgnoreCase);
}
