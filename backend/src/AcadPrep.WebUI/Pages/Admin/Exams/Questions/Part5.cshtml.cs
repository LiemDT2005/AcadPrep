using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Admin.Exams.Commands.CreatePart5Question;
using AcadPrep.Application.Common.Utils;
using AcadPrep.Application.Features.Admin.Exams.Queries.GetExamDetail;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces;

namespace AcadPrep.WebUI.Pages.Admin.Exams.Questions;

public class Part5Model(ISender mediator, IAppDbContext context) : PageModel
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;

    [BindProperty]
    public Part5FormModel Form { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public int PartQuestionCount { get; set; }
    public int PartQuestionLimit { get; set; } = ToeicPartLimits.GetLimit(5);
    public bool CanAddAnother => PartQuestionCount < PartQuestionLimit;
    public bool IsPartFull => PartQuestionCount >= PartQuestionLimit;

    public async Task<IActionResult> OnGetAsync(int examId)
    {
        ExamId = examId;
        var exam = await context.Exams.FirstOrDefaultAsync(e => e.Id == examId && !e.IsDeleted);
        if (exam == null)
        {
            return RedirectToPage("/Admin/Exams/Index");
        }

        ExamTitle = exam.Title;

        // Suggest the next question number
        var maxQNum = await context.Questions
            .Where(q => q.ExamId == examId)
            .Select(q => (int?)q.QuestionNumber)
            .MaxAsync();
        
        Form.QuestionNumber = (maxQNum ?? 100) + 1;

        PartQuestionCount = await context.Questions.CountAsync(q => q.ExamId == examId && q.Part == 5);
        if (IsPartFull)
        {
            TempData["ErrorMessage"] =
                $"Part 5 already has the maximum of {PartQuestionLimit} questions.";
            return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int examId)
    {
        return await SaveQuestionAsync(examId, addAnother: false);
    }

    public async Task<IActionResult> OnPostAddAnotherAsync(int examId)
    {
        return await SaveQuestionAsync(examId, addAnother: true);
    }

    private async Task<IActionResult> SaveQuestionAsync(int examId, bool addAnother)
    {
        ExamId = examId;
        var exam = await context.Exams.FirstOrDefaultAsync(e => e.Id == examId && !e.IsDeleted);
        if (exam == null)
        {
            return RedirectToPage("/Admin/Exams/Index");
        }
        ExamTitle = exam.Title;

        PartQuestionCount = await context.Questions.CountAsync(q => q.ExamId == examId && q.Part == 5);
        if (IsPartFull)
        {
            ErrorMessage = $"Part 5 already has the maximum of {PartQuestionLimit} questions.";
            return Page();
        }

        if (!ModelState.IsValid)
        {
            ValidationErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrEmpty(m))
                .ToList();
            return Page();
        }

        var cmd = new CreatePart5QuestionCommand
        {
            ExamId = examId,
            Question = new Part5QuestionDto
            {
                QuestionNumber = Form.QuestionNumber,
                QuestionText = Form.QuestionText,
                CorrectOption = Form.CorrectOption,
                QuestionType = Form.QuestionType,
                TopicTag = Form.TopicTag,
                Options = new List<Part5OptionDto>
                {
                    new() { Letter = "A", Text = Form.OptionA },
                    new() { Letter = "B", Text = Form.OptionB },
                    new() { Letter = "C", Text = Form.OptionC },
                    new() { Letter = "D", Text = Form.OptionD }
                }
            }
        };

        try
        {
            var result = await mediator.Send(cmd);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = $"Successfully created question number {Form.QuestionNumber}.";
                if (addAnother)
                {
                    if (!CanAddAnother)
                    {
                        return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
                    }

                    return RedirectToPage(new { examId });
                }

                return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
            }

            ErrorMessage = result.Error ?? "Failed to create question.";
        }
        catch (ValidationException ex)
        {
            ValidationErrors = ex.Errors.Select(e => e.ErrorMessage).ToList();
            if (ValidationErrors.Count == 0)
            {
                ErrorMessage = "Invalid input data.";
            }
        }

        return Page();
    }
}

public class Part5FormModel
{
    public int QuestionNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string CorrectOption { get; set; } = "A";
    public string? QuestionType { get; set; }
    public string? TopicTag { get; set; }
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
}
