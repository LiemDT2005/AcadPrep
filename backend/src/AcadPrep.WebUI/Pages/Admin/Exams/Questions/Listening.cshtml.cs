using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Utils;
using AcadPrep.Application.Features.Admin.Exams.Commands.CreateListeningGroup;
using AcadPrep.Application.Features.Admin.Exams.Commands.UpdateListeningGroup;
using Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using Domain.Enums;

namespace AcadPrep.WebUI.Pages.Admin.Exams.Questions;

[Authorize(Roles = nameof(UserRole.Admin) + "," + nameof(UserRole.Moderator))]
public class ListeningModel(ISender mediator, IAppDbContext context, IFileStorageService fileStorage) : PageModel
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public bool HasExamFullAudio { get; set; }
    public string? ExamAudioUrl { get; set; }
    public bool IsEditMode { get; set; }

    [BindProperty]
    public ListeningFormModel Form { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int examId, int part, int? groupId = null)
    {
        if (part is 1 or 2)
        {
            return RedirectToPage("/Admin/Exams/Questions/ListeningQuestion", new { examId, part });
        }

        ExamId = examId;
        Form.Part = part is < 3 or > 4 ? 3 : part;

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

        if (groupId.HasValue)
        {
            IsEditMode = true;
            Form.GroupId = groupId.Value;

            var group = await context.QuestionGroups
                .Include(g => g.Questions)
                    .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefaultAsync(g => g.Id == groupId.Value && g.ExamId == examId);

            if (group == null)
            {
                TempData["ErrorMessage"] = "Listening group not found.";
                return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
            }

            var questions = group.Questions.Where(q => q.Part == Form.Part).OrderBy(q => q.QuestionNumber).ToList();
            if (questions.Count != 3)
            {
                TempData["ErrorMessage"] = "Listening group must have exactly 3 questions to edit.";
                return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
            }

            Form.Name = group.Name;
            Form.ImageUrl = group.ImageUrl;
            Form.AudioUrl = group.AudioUrl;
            Form.AudioStartSecond = group.AudioStartSecond;
            Form.AudioEndSecond = group.AudioEndSecond;
            Form.UseExamFullAudio = HasExamFullAudio || group.AudioStartSecond.HasValue;

            foreach (var q in questions)
            {
                var item = new ListeningQuestionFormItem
                {
                    Id = q.Id,
                    QuestionNumber = q.QuestionNumber,
                    QuestionText = q.QuestionText,
                    ImageUrl = q.ImageUrl,
                    CorrectOption = q.CorrectOption.ToString()
                };

                foreach (var opt in q.QuestionOptions.OrderBy(o => o.OptionLetter))
                {
                    switch (opt.OptionLetter.ToString())
                    {
                        case "A": item.OptionA = opt.OptionText; break;
                        case "B": item.OptionB = opt.OptionText; break;
                        case "C": item.OptionC = opt.OptionText; break;
                        case "D": item.OptionD = opt.OptionText; break;
                    }
                }

                Form.Questions.Add(item);
            }

            return Page();
        }

        var partCount = await context.Questions.CountAsync(q => q.ExamId == examId && q.Part == Form.Part);
        if (!ToeicPartLimits.CanAddQuestionCount(Form.Part, partCount, ToeicPartLimits.ListeningGroupQuestionCount))
        {
            TempData["ErrorMessage"] =
                $"Part {Form.Part} already has the maximum of {ToeicPartLimits.GetLimit(Form.Part)} questions.";
            return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
        }

        Form.Name = Form.Part switch
        {
            3 => "Part 3 - Conversations Set",
            4 => "Part 4 - Talks Set",
            _ => "New listening group"
        };

        var maxQNum = await context.Questions
            .Where(q => q.ExamId == examId)
            .Select(q => (int?)q.QuestionNumber)
            .MaxAsync();

        var startNum = (maxQNum ?? 0) + 1;

        for (int i = 0; i < 3; i++)
        {
            Form.Questions.Add(new ListeningQuestionFormItem
            {
                QuestionNumber = startNum + i,
                QuestionText = "Select the best answer based on the listening content."
            });
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int examId)
    {
        if (Form.Part is 1 or 2)
        {
            return RedirectToPage("/Admin/Exams/Questions/ListeningQuestion", new { examId, part = Form.Part });
        }

        ExamId = examId;
        IsEditMode = Form.GroupId.HasValue && Form.GroupId.Value > 0;

        var exam = await context.Exams.FirstOrDefaultAsync(e => e.Id == examId && !e.IsDeleted);
        if (exam == null)
        {
            return RedirectToPage("/Admin/Exams/Index");
        }
        ExamTitle = exam.Title;
        HasExamFullAudio = !string.IsNullOrWhiteSpace(exam.AudioUrl);
        ExamAudioUrl = exam.AudioUrl;

        if (!ModelState.IsValid)
        {
            ValidationErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrEmpty(m))
                .ToList();
            return Page();
        }

        if (Form.Questions.Count != 3)
        {
            ValidationErrors.Add("Part 3 and Part 4 require exactly 3 questions.");
            return Page();
        }

        if (HasExamFullAudio)
        {
            Form.UseExamFullAudio = true;
        }
        else if (Form.UseExamFullAudio)
        {
            ErrorMessage = "This exam does not have a full audio file. Upload a separate audio file for this group.";
            return Page();
        }

        for (int i = 0; i < Form.Questions.Count; i++)
        {
            var imageFileKey = $"Form.Questions[{i}].ImageFile";
            var audioFileKey = $"Form.Questions[{i}].AudioFile";
            var imageFile = Request.Form.Files[imageFileKey];
            var audioFile = Request.Form.Files[audioFileKey];
            if (imageFile != null) Form.Questions[i].ImageFile = imageFile;
            if (audioFile != null) Form.Questions[i].AudioFile = audioFile;
        }

        string? groupImageUrl = Form.ImageUrl;
        if (Form.ImageFile != null && Form.ImageFile.Length > 0)
        {
            try
            {
                using var stream = Form.ImageFile.OpenReadStream();
                var uploadResult = await fileStorage.UploadImageAsync(stream, Form.ImageFile.FileName);
                groupImageUrl = uploadResult.Url;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Image upload error: {ex.Message}";
                return Page();
            }
        }

        string? groupAudioUrl = Form.AudioUrl;
        if (!Form.UseExamFullAudio && Form.Part >= 3 && Form.AudioFile != null && Form.AudioFile.Length > 0)
        {
            try
            {
                using var stream = Form.AudioFile.OpenReadStream();
                var uploadResult = await fileStorage.UploadAudioAsync(stream, Form.AudioFile.FileName);
                groupAudioUrl = uploadResult.Url;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Audio upload error: {ex.Message}";
                return Page();
            }
        }

        if (!Form.UseExamFullAudio && string.IsNullOrWhiteSpace(groupAudioUrl))
        {
            ValidationErrors.Add("Group audio file is required.");
            return Page();
        }

        for (int i = 0; i < Form.Questions.Count; i++)
        {
            var q = Form.Questions[i];
            if (q.ImageFile != null && q.ImageFile.Length > 0)
            {
                try
                {
                    using var stream = q.ImageFile.OpenReadStream();
                    var uploadResult = await fileStorage.UploadImageAsync(stream, q.ImageFile.FileName);
                    q.ImageUrl = uploadResult.Url;
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Question {q.QuestionNumber} image upload error: {ex.Message}";
                    return Page();
                }
            }
        }

        try
        {
            if (IsEditMode)
            {
                var updateResult = await mediator.Send(new UpdateListeningGroupCommand
                {
                    ExamId = examId,
                    QuestionGroupId = Form.GroupId!.Value,
                    Group = new UpdateListeningGroupDto
                    {
                        Part = Form.Part,
                        Name = Form.Name,
                        Media = new ListeningGroupMediaDto
                        {
                            AudioUrl = groupAudioUrl,
                            AudioStartSecond = Form.AudioStartSecond,
                            AudioEndSecond = Form.AudioEndSecond,
                            ImageUrl = groupImageUrl,
                            UseExamFullAudio = Form.UseExamFullAudio
                        },
                        Questions = Form.Questions.Select(q => new UpdateListeningGroupQuestionDto
                        {
                            Id = q.Id,
                            QuestionNumber = q.QuestionNumber,
                            QuestionText = q.QuestionText,
                            ImageUrl = q.ImageUrl,
                            CorrectOption = q.CorrectOption,
                            Options = new List<ListeningOptionDto>
                            {
                                new() { Letter = "A", Text = q.OptionA },
                                new() { Letter = "B", Text = q.OptionB },
                                new() { Letter = "C", Text = q.OptionC },
                                new() { Letter = "D", Text = q.OptionD }
                            }
                        }).ToList()
                    }
                });

                if (updateResult.IsSuccess)
                {
                    TempData["SuccessMessage"] = $"Successfully updated listening group '{Form.Name}'.";
                    return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
                }

                ErrorMessage = updateResult.Error ?? "Failed to update listening group.";
                return Page();
            }

            var result = await mediator.Send(new CreateListeningGroupCommand
            {
                ExamId = examId,
                Group = new CreateListeningGroupDto
                {
                    Part = Form.Part,
                    Name = Form.Name,
                    Media = new ListeningGroupMediaDto
                    {
                        AudioUrl = groupAudioUrl,
                        AudioStartSecond = Form.AudioStartSecond,
                        AudioEndSecond = Form.AudioEndSecond,
                        ImageUrl = groupImageUrl,
                        UseExamFullAudio = Form.UseExamFullAudio
                    },
                    Questions = Form.Questions.Select(q => new ListeningQuestionDto
                    {
                        QuestionNumber = q.QuestionNumber,
                        QuestionText = q.QuestionText,
                        ImageUrl = q.ImageUrl,
                        CorrectOption = q.CorrectOption,
                        AudioUrl = q.AudioUrl,
                        UseExamFullAudio = q.UseExamFullAudio,
                        AudioStartSecond = q.AudioStartSecond,
                        AudioEndSecond = q.AudioEndSecond,
                        Options = new List<ListeningOptionDto>
                        {
                            new() { Letter = "A", Text = q.OptionA },
                            new() { Letter = "B", Text = q.OptionB },
                            new() { Letter = "C", Text = q.OptionC },
                            new() { Letter = "D", Text = q.OptionD }
                        }
                    }).ToList()
                }
            });

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = $"Successfully created listening group '{Form.Name}'.";
                return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
            }

            ErrorMessage = result.Error ?? "Failed to create listening group.";
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

public class ListeningFormModel
{
    public int? GroupId { get; set; }
    public int Part { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool UseExamFullAudio { get; set; }
    public string? AudioUrl { get; set; }
    public int? AudioStartSecond { get; set; }
    public int? AudioEndSecond { get; set; }
    public string? ImageUrl { get; set; }

    public IFormFile? AudioFile { get; set; }
    public IFormFile? ImageFile { get; set; }

    public List<ListeningQuestionFormItem> Questions { get; set; } = new();
}

public class ListeningQuestionFormItem
{
    public int Id { get; set; }
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
