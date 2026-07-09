using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class User : BaseEntity<int>, IAuditable
{
    // Private parameterless constructor for EF Core
    private User() { }

    public string Email { get; private set; } = null!;
    public string? PasswordHash { get; private set; }
    public string FullName { get; private set; } = null!;
    public string? GoogleId { get; private set; }
    public UserStatus Status { get; private set; } = UserStatus.Active;
    public int RoleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }

    // Navigation properties
    public virtual Role Role { get; set; } = null!;
    public virtual StudyStreak? StudyStreak { get; set; }
    public virtual ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();
    public virtual ICollection<SavedVocabulary> SavedVocabularies { get; set; } = new List<SavedVocabulary>();
    public virtual ICollection<StudyStreak> StudyStreaks { get; set; } = new List<StudyStreak>();
    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    /// <summary>
    /// Factory: tạo User đăng nhập bằng email/password (PasswordHash được set sau khi hash).
    /// </summary>
    public static User Create(string email, string fullName, string passwordHash, int roleId)
    {
        return new User
        {
            Email = email,
            FullName = fullName,
            PasswordHash = passwordHash,
            Status = UserStatus.Inactive,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory: tạo User đăng nhập lần đầu bằng Google (không có PasswordHash).
    /// </summary>
    public static User CreateFromGoogle(string email, string fullName, string googleId, int defaultRoleId)
    {
        return new User
        {
            Email = email,
            FullName = fullName,
            GoogleId = googleId,
            Status = UserStatus.Active,
            RoleId = defaultRoleId,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Kích hoạt tài khoản (Inactive → Active). Idempotent nếu đã Active.
    /// </summary>
    public void Activate()
    {
        Status = UserStatus.Active;
    }

    /// <summary>
    /// Gán GoogleId cho tài khoản nếu chưa được liên kết. Idempotent nếu đã có.
    /// </summary>
    public void LinkGoogleIdentity(string googleId)
    {
        if (string.IsNullOrWhiteSpace(GoogleId))
        {
            GoogleId = googleId;
        }
    }
}
