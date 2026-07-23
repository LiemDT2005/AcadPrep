namespace Domain.Constants;

/// <summary>
/// Bộ giá trị chuẩn hóa cho <c>Notification.Type</c> — dùng làm hợp đồng tích hợp
/// giữa các module. Mọi nơi tạo thông báo PHẢI tham chiếu các hằng số này thay vì
/// hardcode chuỗi rời rạc. Giá trị là ASCII snake_case dạng <c>domain.event</c>
/// để khớp ràng buộc cột (max 50 ký tự, non-unicode).
/// </summary>
public static class NotificationType
{
    // Account & Security (Thành viên 01)
    public const string AccountWelcome = "account.welcome";
    public const string AccountStatusChanged = "account.status_changed";
    public const string AccountRoleChanged = "account.role_changed";
    public const string SecurityPasswordChanged = "security.password_changed";
    public const string SecurityPasswordReset = "security.password_reset";

    // Exam / Testing (Thành viên 02)
    public const string ExamResultReady = "exam.result_ready";
    public const string ExamResumeReminder = "exam.resume_reminder";

    // Content (Thành viên 04)
    public const string ContentExamPublished = "content.exam_published";

    // Gamification & Vocabulary (Thành viên 03)
    public const string AchievementUnlocked = "gamification.achievement_unlocked";
    public const string StreakReminder = "gamification.streak_reminder";
    public const string StreakReset = "gamification.streak_reset";
    public const string VocabReviewDue = "vocab.review_due";

    // Billing
    public const string PaymentSucceeded = "billing.payment_succeeded";
    public const string SubscriptionExpiring = "billing.subscription_expiring";
    public const string SubscriptionGranted = "billing.subscription_granted";

    // Admin / System alerts — broadcast tới các tài khoản Admin
    public const string AdminNewUserRegistered = "admin.new_user_registered";
    public const string AdminExamCreated = "admin.exam_created";
    public const string AdminAccountRoleChanged = "admin.account_role_changed";
}
