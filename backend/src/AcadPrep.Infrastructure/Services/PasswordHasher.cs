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
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Hash cũ trong DB không phải BCrypt → coi như sai mật khẩu, tránh 500.
            return false;
        }
    }
}
