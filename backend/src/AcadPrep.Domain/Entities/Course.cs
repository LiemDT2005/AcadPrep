using System;
using Domain.Common;

namespace Domain.Entities;

public class Course : BaseEntity
{
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Level { get; private set; } = null!; // E.g., IELTS, TOEIC, B1, B2, General
    public decimal Price { get; private set; }

    // Parameterless constructor for EF Core
    private Course() { }

    private Course(string title, string description, string level, decimal price, string createdBy, DateTimeOffset utcNow)
    {
        Id = Guid.NewGuid().ToString();
        Title = title;
        Description = description;
        Level = level;
        Price = price;
        CreatedBy = createdBy;
        CreatedDate = utcNow.UtcDateTime;
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
        LastModifiedDate = utcNow.UtcDateTime;
    }
}
