using Domain.Common;

namespace Domain.Entities;

public class ExamSeries : BaseEntity<int>, IAuditable, ISoftDeletable
{
    public string Name { get; set; } = null!;       // Ví dụ: "ETS", "New Economy", "Hacker TOEIC"
    public int Year { get; set; }                   // Ví dụ: 2023, 2024
    public string? Description { get; set; }        // Mô tả bộ đề (ví dụ: "Bộ đề thi thử TOEIC chuẩn cấu trúc...")
    public string? CoverImageUrl { get; set; }      // Đường dẫn ảnh bìa sách
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
    
    public void SoftDelete() => IsDeleted = true;
}
