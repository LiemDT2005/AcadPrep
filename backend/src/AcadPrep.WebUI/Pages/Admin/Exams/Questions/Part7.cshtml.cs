using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Admin.Exams.Commands.CreateReadingSet;
using AcadPrep.Application.Common.Utils;
using Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.WebUI.Pages.Admin.Exams.Questions;

public class Part7Model(ISender mediator, IAppDbContext context, IFileStorageService fileStorage) : PageModel
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;

    [BindProperty]
    public Part7FormModel Form { get; set; } = new();

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

        var part7Count = await context.Questions.CountAsync(q => q.ExamId == examId && q.Part == 7);
        if (!ToeicPartLimits.CanAddQuestionCount(7, part7Count, ToeicPartLimits.ReadingSetMinQuestionCount))
        {
            TempData["ErrorMessage"] =
                $"Part 7 already has the maximum of {ToeicPartLimits.GetLimit(7)} questions.";
            return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
        }

        // Count existing Part 7 groups to suggest name
        var count = await context.QuestionGroups
            .CountAsync(g => g.ExamId == examId && g.Questions.Any(q => q.Part == 7));

        Form.Name = $"Set {count + 1:D2} - Reading Comprehension";

        // Suggest starting question number (Part 7 typically starts at 147)
        var maxQNum = await context.Questions
            .Where(q => q.ExamId == examId)
            .Select(q => (int?)q.QuestionNumber)
            .MaxAsync();
        
        var startNum = maxQNum != null ? maxQNum.Value + 1 : 147;

        // Prepopulate 1 passage
        Form.Passages.Add(new Part7PassageFormItem { Content = "Dear Mr. Lee, We are writing to confirm...", DisplayOrder = 1 });

        // Prepopulate 2 questions
        Form.Questions.Add(new Part7QuestionFormItem { QuestionNumber = startNum, QuestionText = "What is the main purpose of the email?", CorrectOption = "A" });
        Form.Questions.Add(new Part7QuestionFormItem { QuestionNumber = startNum + 1, QuestionText = "What is indicated about Mr. Lee?", CorrectOption = "B" });

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

        for (int i = 0; i < Form.Passages.Count; i++)
        {
            var passage = Form.Passages[i];
            var hasImage = (passage.ImageFile != null && passage.ImageFile.Length > 0)
                           || !string.IsNullOrWhiteSpace(passage.ImageUrl);
            if (hasImage)
            {
                ModelState.Remove($"{nameof(Form)}.{nameof(Form.Passages)}[{i}].{nameof(Part7PassageFormItem.Content)}");
            }
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

        if (Form.Passages.Count < 1 || Form.Passages.Count > 3)
        {
            ValidationErrors.Add("Part 7 requires 1 to 3 passages.");
            return Page();
        }

        if (Form.Questions.Count < 2 || Form.Questions.Count > 5)
        {
            ValidationErrors.Add("Part 7 requires 2 to 5 questions.");
            return Page();
        }

        for (int i = 0; i < Form.Passages.Count; i++)
        {
            var passage = Form.Passages[i];
            var hasText = !string.IsNullOrWhiteSpace(passage.Content);
            var hasImage = (passage.ImageFile != null && passage.ImageFile.Length > 0)
                           || !string.IsNullOrWhiteSpace(passage.ImageUrl);
            if (!hasText && !hasImage)
            {
                ValidationErrors.Add($"Passage {i + 1} must have either content text or an image.");
                return Page();
            }
        }

        // Process Passage Uploads
        var commandPassages = new List<ReadingPassageDto>();
        for (int i = 0; i < Form.Passages.Count; i++)
        {
            var pItem = Form.Passages[i];
            string? imageUrl = pItem.ImageUrl;
            
            if (pItem.ImageFile != null && pItem.ImageFile.Length > 0)
            {
                try
                {
                    using var stream = pItem.ImageFile.OpenReadStream();
                    var uploadResult = await fileStorage.UploadImageAsync(stream, pItem.ImageFile.FileName);
                    imageUrl = uploadResult.Url;
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Image upload error for passage {i + 1}: {ex.Message}";
                    return Page();
                }
            }

            commandPassages.Add(new ReadingPassageDto
            {
                Content = pItem.Content?.Trim(),
                ImageUrl = imageUrl,
                DisplayOrder = pItem.DisplayOrder
            });
        }

        var cmd = new CreateReadingSetCommand
        {
            ExamId = examId,
            Set = new CreateReadingSetDto
            {
                Name = Form.Name,
                Passages = commandPassages,
                Questions = Form.Questions.Select(q => new ReadingQuestionDto
                {
                    QuestionNumber = q.QuestionNumber,
                    QuestionText = q.QuestionText,
                    CorrectOption = q.CorrectOption,
                    Options = new List<ReadingOptionDto>
                    {
                        new() { Letter = "A", Text = q.OptionA },
                        new() { Letter = "B", Text = q.OptionB },
                        new() { Letter = "C", Text = q.OptionC },
                        new() { Letter = "D", Text = q.OptionD }
                    }
                }).ToList()
            }
        };

        try
        {
            var result = await mediator.Send(cmd);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = $"Successfully created Reading Set '{Form.Name}'.";
                return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
            }

            ErrorMessage = result.Error ?? "Failed to create Reading Set.";
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

public class Part7FormModel
{
    public string Name { get; set; } = string.Empty;
    public List<Part7PassageFormItem> Passages { get; set; } = new();
    public List<Part7QuestionFormItem> Questions { get; set; } = new();
}

public class Part7PassageFormItem
{
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
    public int DisplayOrder { get; set; } = 1;
}

public class Part7QuestionFormItem
{
    public int QuestionNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string CorrectOption { get; set; } = "A";
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
}
