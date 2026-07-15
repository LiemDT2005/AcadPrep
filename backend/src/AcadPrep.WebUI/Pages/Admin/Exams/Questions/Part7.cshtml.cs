using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Utils;
using AcadPrep.Application.Features.Admin.Exams.Commands.CreateReadingSet;
using AcadPrep.Application.Features.Admin.Exams.Commands.UpdateReadingSet;
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
    public bool IsEditMode { get; set; }

    [BindProperty]
    public Part7FormModel Form { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int examId, int? groupId = null)
    {
        ExamId = examId;
        var exam = await context.Exams.FirstOrDefaultAsync(e => e.Id == examId && !e.IsDeleted);
        if (exam == null)
        {
            return RedirectToPage("/Admin/Exams/Index");
        }

        ExamTitle = exam.Title;

        if (groupId.HasValue)
        {
            IsEditMode = true;
            Form.GroupId = groupId.Value;

            var group = await context.QuestionGroups
                .Include(g => g.Passages)
                .Include(g => g.Questions)
                    .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefaultAsync(g => g.Id == groupId.Value && g.ExamId == examId);

            if (group == null)
            {
                TempData["ErrorMessage"] = "Reading set not found.";
                return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
            }

            Form.Name = group.Name;

            foreach (var p in group.Passages.OrderBy(x => x.DisplayOrder))
            {
                Form.Passages.Add(new Part7PassageFormItem
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    DisplayOrder = p.DisplayOrder
                });
            }

            foreach (var q in group.Questions.Where(x => x.Part == 7).OrderBy(x => x.QuestionNumber))
            {
                var item = new Part7QuestionFormItem
                {
                    Id = q.Id,
                    QuestionNumber = q.QuestionNumber,
                    QuestionText = q.QuestionText ?? string.Empty,
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

            if (Form.Passages.Count < 1 || Form.Questions.Count < 2)
            {
                TempData["ErrorMessage"] = "Reading set is incomplete and cannot be edited.";
                return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
            }

            return Page();
        }

        var part7Count = await context.Questions.CountAsync(q => q.ExamId == examId && q.Part == 7);
        if (!ToeicPartLimits.CanAddQuestionCount(7, part7Count, ToeicPartLimits.ReadingSetMinQuestionCount))
        {
            TempData["ErrorMessage"] =
                $"Part 7 already has the maximum of {ToeicPartLimits.GetLimit(7)} questions.";
            return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
        }

        var count = await context.QuestionGroups
            .CountAsync(g => g.ExamId == examId && g.Questions.Any(q => q.Part == 7));

        Form.Name = $"Set {count + 1:D2} - Reading Comprehension";

        var maxQNum = await context.Questions
            .Where(q => q.ExamId == examId)
            .Select(q => (int?)q.QuestionNumber)
            .MaxAsync();

        var startNum = maxQNum != null ? maxQNum.Value + 1 : 147;

        Form.Passages.Add(new Part7PassageFormItem { Content = "Dear Mr. Lee, We are writing to confirm...", DisplayOrder = 1 });
        Form.Questions.Add(new Part7QuestionFormItem { QuestionNumber = startNum, QuestionText = "What is the main purpose of the email?", CorrectOption = "A" });
        Form.Questions.Add(new Part7QuestionFormItem { QuestionNumber = startNum + 1, QuestionText = "What is indicated about Mr. Lee?", CorrectOption = "B" });

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int examId)
    {
        ExamId = examId;
        IsEditMode = Form.GroupId.HasValue && Form.GroupId.Value > 0;

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

        var commandPassages = new List<(int Id, string? Content, string? ImageUrl, int DisplayOrder)>();
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

            commandPassages.Add((pItem.Id, pItem.Content?.Trim(), imageUrl, pItem.DisplayOrder));
        }

        try
        {
            if (IsEditMode)
            {
                var updateResult = await mediator.Send(new UpdateReadingSetCommand
                {
                    ExamId = examId,
                    QuestionGroupId = Form.GroupId!.Value,
                    Set = new UpdateReadingSetDto
                    {
                        Name = Form.Name,
                        Passages = commandPassages.Select(p => new UpdateReadingPassageDto
                        {
                            Id = p.Id,
                            Content = p.Content,
                            ImageUrl = p.ImageUrl,
                            DisplayOrder = p.DisplayOrder
                        }).ToList(),
                        Questions = Form.Questions.Select(q => new UpdateReadingQuestionDto
                        {
                            Id = q.Id,
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
                });

                if (updateResult.IsSuccess)
                {
                    TempData["SuccessMessage"] = $"Successfully updated Reading Set '{Form.Name}'.";
                    return RedirectToPage("/Admin/Exams/Edit", new { id = examId });
                }

                ErrorMessage = updateResult.Error ?? "Failed to update Reading Set.";
                return Page();
            }

            var result = await mediator.Send(new CreateReadingSetCommand
            {
                ExamId = examId,
                Set = new CreateReadingSetDto
                {
                    Name = Form.Name,
                    Passages = commandPassages.Select(p => new ReadingPassageDto
                    {
                        Content = p.Content,
                        ImageUrl = p.ImageUrl,
                        DisplayOrder = p.DisplayOrder
                    }).ToList(),
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
            });

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
    public int? GroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Part7PassageFormItem> Passages { get; set; } = new();
    public List<Part7QuestionFormItem> Questions { get; set; } = new();
}

public class Part7PassageFormItem
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
    public int DisplayOrder { get; set; } = 1;
}

public class Part7QuestionFormItem
{
    public int Id { get; set; }
    public int QuestionNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string CorrectOption { get; set; } = "A";
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
}
