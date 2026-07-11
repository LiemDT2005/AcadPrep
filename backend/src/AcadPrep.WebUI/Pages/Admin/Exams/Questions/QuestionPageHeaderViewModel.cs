namespace AcadPrep.WebUI.Pages.Admin.Exams.Questions;

public class QuestionPageHeaderViewModel
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = "";
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string PartBadge { get; set; } = "";
    public string PartIcon { get; set; } = "quiz";
    public string PartBadgeVariant { get; set; } = "primary";
    public string BreadcrumbCurrent { get; set; } = "";
}
