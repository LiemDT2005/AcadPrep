using Domain.Constants;

namespace Application.Common.Options;

/// <summary>Hạn mức Free — bind từ section "Freemium".</summary>
public sealed class FreemiumSettings
{
    public const string SectionName = "Freemium";

    public int FullTestsPerMonth { get; set; } = FreemiumLimits.DefaultFullTestsPerMonth;
    public int PracticeSessionsPerDay { get; set; } = FreemiumLimits.DefaultPracticeSessionsPerDay;
    public int SavedVocabularyMax { get; set; } = FreemiumLimits.DefaultSavedVocabularyMax;
}
