namespace Domain.Common;

/// <summary>
/// Interface cho entity có audit timestamp (thời gian tạo/sửa).
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; }
    DateTime? LastModifiedAt { get; }
}
