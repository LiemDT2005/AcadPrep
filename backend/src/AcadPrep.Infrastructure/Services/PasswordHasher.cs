using Application.Common.Interfaces;

namespace Infrastructure.Services;

/// <summary>
/// Implement IPasswordHasher dùng BCrypt.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool Verify(string hash, string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
