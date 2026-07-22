namespace Domain.Constants;

/// <summary>
/// Mã lỗi/quota dùng trong Result.Error để UI nhận diện paywall.
/// </summary>
public static class BillingCodes
{
    public const string ProRequiredPrefix = "PRO_REQUIRED:";

    public const string FullTestQuota = "FULL_TEST_QUOTA";
    public const string PracticeQuota = "PRACTICE_QUOTA";
    public const string VocabQuota = "VOCAB_QUOTA";
    public const string LoginRequired = "LOGIN_REQUIRED";

    public static string ProRequired(string reason) => $"{ProRequiredPrefix}{reason}";

    public static bool IsProRequired(string? error) =>
        !string.IsNullOrEmpty(error) && error.StartsWith(ProRequiredPrefix, StringComparison.Ordinal);
}
