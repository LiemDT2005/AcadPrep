namespace AcadPrep.WebUI.Pages.Admin.Exams.Questions;

public class ValidationAlertsViewModel
{
    public string? ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
}
