using System;
using System.Collections.Generic;

namespace Domain.Entities;

public class User
{
    public int UserId { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Status { get; set; } = "Active";
    public int RoleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Role Role { get; set; } = null!;
    public virtual StudyStreak? StudyStreak { get; set; }
    public virtual ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();
    public virtual ICollection<SavedVocabulary> SavedVocabularies { get; set; } = new List<SavedVocabulary>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
