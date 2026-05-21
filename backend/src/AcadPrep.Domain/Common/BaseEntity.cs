using System;

namespace Domain.Common;

public abstract class BaseEntity
{
    public string Id { get; protected set; } = Guid.NewGuid().ToString();
    public string CreatedBy { get; protected set; } = "System";
    public DateTime CreatedDate { get; protected set; } = DateTime.UtcNow;
    public string? LastModifiedBy { get; protected set; }
    public DateTime? LastModifiedDate { get; protected set; }
    public bool IsDeleted { get; protected set; }
    public bool IsActive { get; protected set; } = true;

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void SoftDelete(string deletedBy)
    {
        IsDeleted = true;
        LastModifiedBy = deletedBy;
        LastModifiedDate = DateTime.UtcNow;
    }
}
