using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Utils;
using AcadPrep.Application.Features.Admin.Exams.Commands.CreatePart5Question;
using AcadPrep.Application.Features.Admin.Exams.Commands.UpdatePart5Question;
using Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using Domain.Enums;

namespace AcadPrep.WebUI.Pages.Admin.Exams.Questions;

[Authorize(Roles = nameof(UserRole.Moderator))]
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
    public bool CanAddAnother => !IsEditMode && PartQuestionCount < PartQuestionLimit;
    public bool IsPartFull => PartQuestionCount >= PartQuestionLimit;
    public bool IsEditMode { get; set; }

    public async Task<IActionResult> OnGetAsync(int examId, int? editId = null)
    {
        ExamId = examId;
        var exam = await context.Exams.FirstOrDefaultAsync(e => e.Id == examId && !e.IsDeleted);
        if (exam == null)
        {
            return RedirectToPage("/Admin/Exams/Index");
        }

        ExamTitle = exam.Title;
        PartQuestionCount = await context.Questions.CountAsync(q => q.ExamId == examId && q.Part == 5);

        if (editId.HasValue)
        {
            IsEditMode = true;
            Form.EditId = editId.Value;

            var question = await context.Questions
                .Include(q => q.QuestionOptions)
                .FirstOrDefaultAsync(q => q.Id == editId.Value && q.ExamId == examId && q.Part == 5);

            if (question == null)
            {
                TempData["ErrorMessage"] = "Question not found.";
                return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
            }

            Form.QuestionNumber = question.QuestionNumber;
            Form.QuestionText = question.QuestionText ?? string.Empty;
            Form.CorrectOption = question.CorrectOption.ToString();
            Form.QuestionType = question.QuestionType;
            Form.TopicTag = question.TopicTag;
            Form.Explanation = question.Explanation ?? string.Empty;

            foreach (var opt in question.QuestionOptions.OrderBy(o => o.OptionLetter))
            {
                switch (opt.OptionLetter.ToString())
                {
                    case "A": Form.OptionA = opt.OptionText; break;
                    case "B": Form.OptionB = opt.OptionText; break;
                    case "C": Form.OptionC = opt.OptionText; break;
                    case "D": Form.OptionD = opt.OptionText; break;
                }
            }

            return Page();
        }

        var maxQNum = await context.Questions
            .Where(q => q.ExamId == examId)
            .Select(q => (int?)q.QuestionNumber)
            .MaxAsync();

        Form.QuestionNumber = (maxQNum ?? 100) + 1;

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
        IsEditMode = Form.EditId.HasValue && Form.EditId.Value > 0;

        var exam = await context.Exams.FirstOrDefaultAsync(e => e.Id == examId && !e.IsDeleted);
        if (exam == null)
        {
            return RedirectToPage("/Admin/Exams/Index");
        }
        ExamTitle = exam.Title;

        PartQuestionCount = await context.Questions.CountAsync(q => q.ExamId == examId && q.Part == 5);
        if (!IsEditMode && IsPartFull)
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

        var questionDto = new Part5QuestionDto
        {
            QuestionNumber = Form.QuestionNumber,
            QuestionText = Form.QuestionText,
            CorrectOption = Form.CorrectOption,
            QuestionType = Form.QuestionType,
            TopicTag = Form.TopicTag,
            Explanation = Form.Explanation,
            Options = new List<Part5OptionDto>
            {
                new() { Letter = "A", Text = Form.OptionA },
                new() { Letter = "B", Text = Form.OptionB },
                new() { Letter = "C", Text = Form.OptionC },
                new() { Letter = "D", Text = Form.OptionD }
            }
        };

        try
        {
            if (IsEditMode)
            {
                var updateResult = await mediator.Send(new UpdatePart5QuestionCommand
                {
                    ExamId = examId,
                    QuestionId = Form.EditId!.Value,
                    Question = questionDto
                });

                if (updateResult.IsSuccess)
                {
                    TempData["SuccessMessage"] = $"Successfully updated question number {Form.QuestionNumber}.";
                    return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
                }

                ErrorMessage = updateResult.Error ?? "Failed to update question.";
                return Page();
            }

            var result = await mediator.Send(new CreatePart5QuestionCommand
            {
                ExamId = examId,
                Question = questionDto
            });

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
    public int? EditId { get; set; }
    public int QuestionNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string CorrectOption { get; set; } = "A";
    public string? QuestionType { get; set; }
    public string? TopicTag { get; set; }
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Explanation is required.")]
    public string Explanation { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
}
