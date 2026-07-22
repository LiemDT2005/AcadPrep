namespace AcadPrep.Application.Common.Models;

/// <summary>Shared reading stimulus shown with Part 6/7 question units.</summary>
public class SessionPassageDto
{
    public int DisplayOrder { get; set; }
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
}
