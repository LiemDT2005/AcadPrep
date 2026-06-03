using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class User : BaseEntity<int>, IAuditable
{
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public int RoleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }

    // Navigation properties
    public virtual Role Role { get; set; } = null!;
    public virtual StudyStreak? StudyStreak { get; set; }
    public virtual ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();
    public virtual ICollection<SavedVocabulary> SavedVocabularies { get; set; } = new List<SavedVocabulary>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
