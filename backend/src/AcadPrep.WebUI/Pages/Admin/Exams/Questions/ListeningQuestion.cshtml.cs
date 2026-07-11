using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AcadPrep.Application.Features.Admin.Exams.Commands.CreateListeningQuestion;
using AcadPrep.Application.Common.Utils;
using Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.WebUI.Pages.Admin.Exams.Questions;

public class ListeningQuestionModel(ISender mediator, IAppDbContext context, IFileStorageService fileStorage) : PageModel
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public bool HasExamFullAudio { get; set; }
    public string? ExamAudioUrl { get; set; }
    public bool IsPart1 => Form.Part == 1;
    public bool IsPart2 => Form.Part == 2;
    public int PartQuestionCount { get; set; }
    public int PartQuestionLimit { get; set; }
    public bool CanAddAnother => PartQuestionCount < PartQuestionLimit;
    public bool IsPartFull => PartQuestionCount >= PartQuestionLimit;

    [BindProperty]
    public ListeningQuestionFormModel Form { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int examId, int part)
    {
        if (part is not (1 or 2))
        {
            return RedirectToPage("/Admin/Exams/Questions/Listening", new { examId, part = part is < 3 ? 3 : part });
        }

        ExamId = examId;
        Form.Part = part;

        var exam = await context.Exams.FirstOrDefaultAsync(e => e.Id == examId && !e.IsDeleted);
        if (exam == null)
        {
            return RedirectToPage("/Admin/Exams/Index");
        }

        ExamTitle = exam.Title;
        HasExamFullAudio = !string.IsNullOrWhiteSpace(exam.AudioUrl);
        ExamAudioUrl = exam.AudioUrl;
        if (HasExamFullAudio)
        {
            Form.UseExamFullAudio = true;
        }

        Form.QuestionText = part == 1
            ? "Which description best matches the photograph?"
            : "Select the best response for the question.";

        var maxQNum = await context.Questions
            .Where(q => q.ExamId == examId)
            .Select(q => (int?)q.QuestionNumber)
            .MaxAsync();

        Form.QuestionNumber = (maxQNum ?? 0) + 1;

        await LoadPartStatsAsync(examId, part);
        if (IsPartFull)
        {
            TempData["ErrorMessage"] =
                $"Part {part} already has the maximum of {PartQuestionLimit} questions.";
            return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
        }

        return Page();
    }

    private async Task LoadPartStatsAsync(int examId, int part)
    {
        PartQuestionCount = await context.Questions.CountAsync(q => q.ExamId == examId && q.Part == part);
        PartQuestionLimit = ToeicPartLimits.GetLimit(part);
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

        if (Form.Part is not (1 or 2))
        {
            return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
        }

        var exam = await context.Exams.FirstOrDefaultAsync(e => e.Id == examId && !e.IsDeleted);
        if (exam == null)
        {
            return RedirectToPage("/Admin/Exams/Index");
        }

        ExamTitle = exam.Title;
        HasExamFullAudio = !string.IsNullOrWhiteSpace(exam.AudioUrl);
        ExamAudioUrl = exam.AudioUrl;

        await LoadPartStatsAsync(examId, Form.Part);
        if (IsPartFull)
        {
            ErrorMessage = $"Part {Form.Part} already has the maximum of {PartQuestionLimit} questions.";
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

        if (HasExamFullAudio)
        {
            Form.UseExamFullAudio = true;
        }
        else if (Form.UseExamFullAudio)
        {
            ErrorMessage = "This exam does not have a full audio file. Upload a separate audio file for this question.";
            return Page();
        }

        if (Form.Part == 1 && (Form.ImageFile == null || Form.ImageFile.Length == 0) && string.IsNullOrWhiteSpace(Form.ImageUrl))
        {
            ValidationErrors.Add("Photograph image is required.");
            return Page();
        }

        try
        {
            if (Form.ImageFile != null && Form.ImageFile.Length > 0)
            {
                using var stream = Form.ImageFile.OpenReadStream();
                var uploadResult = await fileStorage.UploadImageAsync(stream, Form.ImageFile.FileName);
                Form.ImageUrl = uploadResult.Url;
            }

            if (!Form.UseExamFullAudio && Form.AudioFile != null && Form.AudioFile.Length > 0)
            {
                using var stream = Form.AudioFile.OpenReadStream();
                var uploadResult = await fileStorage.UploadAudioAsync(stream, Form.AudioFile.FileName);
                Form.AudioUrl = uploadResult.Url;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"File upload error: {ex.Message}";
            return Page();
        }

        var cmd = new CreateListeningQuestionCommand
        {
            ExamId = examId,
            Part = Form.Part,
            Question = new ListeningQuestionInputDto
            {
                QuestionNumber = Form.QuestionNumber,
                QuestionText = Form.QuestionText,
                ImageUrl = Form.ImageUrl,
                CorrectOption = Form.CorrectOption,
                AudioUrl = Form.AudioUrl,
                UseExamFullAudio = Form.UseExamFullAudio,
                AudioStartSecond = Form.AudioStartSecond,
                AudioEndSecond = Form.AudioEndSecond,
                Options = Form.Part == 2
                    ? new List<ListeningQuestionOptionDto>
                    {
                        new() { Letter = "A", Text = Form.OptionA },
                        new() { Letter = "B", Text = Form.OptionB },
                        new() { Letter = "C", Text = Form.OptionC }
                    }
                    : new List<ListeningQuestionOptionDto>
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
                TempData["SuccessMessage"] = $"Successfully created Part {Form.Part} question number {Form.QuestionNumber}.";
                if (addAnother)
                {
                    if (!CanAddAnother)
                    {
                        return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
                    }

                    return RedirectToPage(new { examId, part = Form.Part });
                }

                return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
            }

            ErrorMessage = result.Error ?? "Failed to create listening question.";
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

public class ListeningQuestionFormModel
{
    public int Part { get; set; }
    public int QuestionNumber { get; set; }
    public string? QuestionText { get; set; }
    public string CorrectOption { get; set; } = "A";
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }

    public string? AudioUrl { get; set; }
    public IFormFile? AudioFile { get; set; }
    public bool UseExamFullAudio { get; set; }
    public int? AudioStartSecond { get; set; }
    public int? AudioEndSecond { get; set; }
}
