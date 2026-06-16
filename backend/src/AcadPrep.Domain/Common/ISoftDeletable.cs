namespace Domain.Common;

/// <summary>
/// Interface cho entity hỗ trợ xóa mềm (soft delete).
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; }
    void SoftDelete();
}
