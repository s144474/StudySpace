using Microsoft.AspNetCore.Identity;
using StudySpace.Exceptions;
using StudySpace.Models;

namespace StudySpace.Services;

public class UserService
{
    private readonly DataStore _store;
    private readonly IPasswordHasher<User> _hasher;

    public UserService(DataStore store, IPasswordHasher<User> hasher)
    {
        _store = store;
        _hasher = hasher;
    }

    public User Register(string fullName, string email, string password)
    {
        lock (_store.SyncRoot)
        {
            if (_store.Users.Any(u => u.MatchesEmail(email)))
                throw new BusinessRuleException("An account with this email address already exists.");

            var user = new User(_store.NextUserId(), fullName.Trim(), email.Trim().ToLowerInvariant(), string.Empty, UserRole.Student);
            user.PasswordHash = _hasher.HashPassword(user, password);
            _store.Users.Add(user);
            return user;
        }
    }

    public User? Authenticate(string email, string password)
    {
        User? user;
        lock (_store.SyncRoot) user = _store.Users.FirstOrDefault(u => u.MatchesEmail(email));
        if (user is null) return null;
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Failed ? null : user;
    }
}
