using Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;

/// <summary>
/// Implement IPasswordHasher dùng Microsoft.AspNetCore.Identity.PasswordHasher&lt;T&gt; làm nền.
/// Dùng object như type parameter vì entity User không kế thừa IdentityUser.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _inner = new();

    public string Hash(string password)
    {
        return _inner.HashPassword(null!, password);
    }

    public bool Verify(string hash, string password)
    {
        var result = _inner.VerifyHashedPassword(null!, hash, password);
        // PasswordVerificationResult.Success hoặc SuccessRehashNeeded đều coi là đúng
        return result != PasswordVerificationResult.Failed;
    }
}
