namespace Domain.Constants;

/// <summary>
/// Hạn mức Free mặc định (có thể override qua appsettings Freemium).
/// </summary>
public static class FreemiumLimits
{
    public const int DefaultFullTestsPerMonth = 1;
    public const int DefaultPracticeSessionsPerDay = 3;
    public const int DefaultSavedVocabularyMax = 50;
}
