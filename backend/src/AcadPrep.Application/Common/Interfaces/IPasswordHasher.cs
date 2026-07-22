namespace Application.Common.Interfaces;

/// <summary>
/// Abstraction cho password hashing — Application layer không phụ thuộc trực tiếp vào
/// bất kỳ thư viện hash cụ thể nào (dependency inversion).
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Hash plaintext password thành chuỗi lưu vào DB.</summary>
    string Hash(string password);

    /// <summary>
    /// Verify plaintext password với hash đã lưu.
    /// </summary>
    /// <returns>true nếu khớp, false nếu sai.</returns>
    bool Verify(string hash, string password);
}
