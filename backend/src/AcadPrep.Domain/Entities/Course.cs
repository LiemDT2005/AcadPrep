using Domain.Common;

namespace Domain.Entities;

public class Course : BaseEntity<int>, IAuditable, ISoftDeletable
{
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Level { get; private set; } = null!; // E.g., IELTS, TOEIC, B1, B2, General
    public decimal Price { get; private set; }
    public string CreatedBy { get; private set; } = "System";
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Parameterless constructor for EF Core
    private Course() { }

    private Course(string title, string description, string level, decimal price, string createdBy, DateTimeOffset utcNow)
    {
        Title = title;
        Description = description;
        Level = level;
        Price = price;
        CreatedBy = createdBy;
        CreatedAt = utcNow.UtcDateTime;
        IsActive = true;
        IsDeleted = false;
    }

    // Rich domain factory method
    public static Course Create(string title, string description, string level, decimal price, string createdBy, DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Tiêu đề khóa học không được để trống", nameof(title));
        
        if (price < 0)
            throw new ArgumentException("Giá khóa học không được nhỏ hơn 0", nameof(price));

        return new Course(title, description, level, price, createdBy, utcNow);
    }

    // Encapsulated behavior for updating
    public void Update(string title, string description, string level, decimal price, string modifiedBy, DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Tiêu đề khóa học không được để trống", nameof(title));

        if (price < 0)
            throw new ArgumentException("Giá khóa học không được nhỏ hơn 0", nameof(price));

        Title = title;
        Description = description;
        Level = level;
        Price = price;
        LastModifiedBy = modifiedBy;
        LastModifiedAt = utcNow.UtcDateTime;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        IsActive = false;
    }
}
