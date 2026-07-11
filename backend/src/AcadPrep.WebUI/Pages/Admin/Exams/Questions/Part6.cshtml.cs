using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Admin.Exams.Commands.CreateTextCompletionSet;
using AcadPrep.Application.Common.Utils;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.WebUI.Pages.Admin.Exams.Questions;

public class Part6Model(ISender mediator, IAppDbContext context, IFileStorageService fileStorage) : PageModel
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;

    [BindProperty]
    public Part6FormModel Form { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int examId)
    {
        ExamId = examId;
        var exam = await context.Exams.FirstOrDefaultAsync(e => e.Id == examId && !e.IsDeleted);
        if (exam == null)
        {
            return RedirectToPage("/Admin/Exams/Index");
        }

        ExamTitle = exam.Title;

        var part6Count = await context.Questions.CountAsync(q => q.ExamId == examId && q.Part == 6);
        if (!ToeicPartLimits.CanAddQuestionCount(6, part6Count, ToeicPartLimits.TextCompletionQuestionCount))
        {
            TempData["ErrorMessage"] =
                $"Part 6 already has the maximum of {ToeicPartLimits.GetLimit(6)} questions.";
            return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
        }

        // Suggest starting question number (Part 6 typically starts at 131)
        var maxQNum = await context.Questions
            .Where(q => q.ExamId == examId)
            .Select(q => (int?)q.QuestionNumber)
            .MaxAsync();
        
        var startNum = maxQNum != null ? maxQNum.Value + 1 : 131;

        for (int i = 0; i < 4; i++)
        {
            Form.Questions.Add(new Part6QuestionFormItem
            {
                QuestionNumber = startNum + i
            });
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int examId)
    {
        ExamId = examId;
        var exam = await context.Exams.FirstOrDefaultAsync(e => e.Id == examId && !e.IsDeleted);
        if (exam == null)
        {
            return RedirectToPage("/Admin/Exams/Index");
        }
        ExamTitle = exam.Title;

        if (Form.PassageInputMode == "image")
        {
            ModelState.Remove($"{nameof(Form)}.{nameof(Form.PassageContent)}");
        }
        else
        {
            ModelState.Remove($"{nameof(Form)}.{nameof(Form.ImageFile)}");
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

        if (Form.Questions.Count != 4)
        {
            ValidationErrors.Add("Part 6 requires exactly 4 questions.");
            return Page();
        }

        var hasText = !string.IsNullOrWhiteSpace(Form.PassageContent);
        var hasImageUpload = Form.ImageFile != null && Form.ImageFile.Length > 0;
        var hasImageUrl = !string.IsNullOrWhiteSpace(Form.ImageUrl);

        if (Form.PassageInputMode == "text")
        {
            if (!hasText)
            {
                ValidationErrors.Add("Passage content is required.");
                return Page();
            }

            if (hasImageUpload)
            {
                ValidationErrors.Add("Cannot upload an image when using text passage mode.");
                return Page();
            }
        }
        else
        {
            if (!hasImageUpload && !hasImageUrl)
            {
                ValidationErrors.Add("Passage image is required.");
                return Page();
            }

            if (hasText)
            {
                ValidationErrors.Add("Cannot enter passage text when using image mode.");
                return Page();
            }

            Form.PassageContent = string.Empty;
        }

        // Process Passage Image Upload (image mode only)
        string? imageUrl = null;
        if (Form.PassageInputMode == "image")
        {
            imageUrl = Form.ImageUrl;
            if (Form.ImageFile != null && Form.ImageFile.Length > 0)
            {
                try
                {
                    using var stream = Form.ImageFile.OpenReadStream();
                    var uploadResult = await fileStorage.UploadImageAsync(stream, Form.ImageFile.FileName);
                    imageUrl = uploadResult.Url;
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Passage image upload error: {ex.Message}";
                    return Page();
                }
            }
        }

        var cmd = new CreateTextCompletionSetCommand
        {
            ExamId = examId,
            Set = new CreateTextCompletionSetDto
            {
                Passage = new TextCompletionPassageDto
                {
                    Content = Form.PassageInputMode == "text" ? Form.PassageContent : null,
                    ImageUrl = imageUrl
                },
                Questions = Form.Questions.Select(q => new TextCompletionQuestionDto
                {
                    QuestionNumber = q.QuestionNumber,
                    CorrectOption = q.CorrectOption,
                    Options = new List<TextCompletionOptionDto>
                    {
                        new() { Letter = "A", Text = q.OptionA },
                        new() { Letter = "B", Text = q.OptionB },
                        new() { Letter = "C", Text = q.OptionC },
                        new() { Letter = "D", Text = q.OptionD }
                    }
                }).ToList()
            }
        };

        var result = await mediator.Send(cmd);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = $"Successfully created Part 6 passage (Q{Form.Questions.First().QuestionNumber} - Q{Form.Questions.Last().QuestionNumber}).";
            return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
        }

        ErrorMessage = result.Error ?? "Failed to create Part 6 passage.";
        return Page();
    }
}

public class Part6FormModel
{
    public string PassageInputMode { get; set; } = "text";
    public string? PassageContent { get; set; }
    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
    public List<Part6QuestionFormItem> Questions { get; set; } = new();
}

public class Part6QuestionFormItem
{
    public int QuestionNumber { get; set; }
    public string CorrectOption { get; set; } = "A";
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
}
