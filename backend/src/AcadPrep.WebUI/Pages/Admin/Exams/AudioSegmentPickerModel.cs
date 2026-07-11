namespace AcadPrep.WebUI.Pages.Admin.Exams;

public class AudioSegmentPickerModel
{
    public required string PickerId { get; init; }
    public required string AudioUrl { get; init; }
    public required string StartInputName { get; init; }
    public required string EndInputName { get; init; }
    public int? InitialStart { get; init; }
    public int? InitialEnd { get; init; }
}
