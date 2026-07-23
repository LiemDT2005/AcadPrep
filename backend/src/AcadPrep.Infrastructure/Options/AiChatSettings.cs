using System.Collections.Generic;

namespace Infrastructure.Options;

public sealed class AiChatSettings
{
    public const string SectionName = "AiChat";
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public IReadOnlyList<AiModelOption> Models { get; set; } = new List<AiModelOption>();
}

public sealed class AiModelOption
{
    public string Id { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 12;
}
