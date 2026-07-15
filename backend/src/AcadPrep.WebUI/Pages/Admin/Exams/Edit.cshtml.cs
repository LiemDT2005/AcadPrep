using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Features.Admin.Exams.Commands.DeleteExamContent;
using AcadPrep.Application.Features.Admin.Exams.Commands.UpdateExam;
using AcadPrep.Application.Features.Admin.Exams.Queries.Common.DTOs;
using AcadPrep.Application.Features.Admin.Exams.Queries.GetExamDetail;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.WebUI.Pages.Admin.Exams;

public class EditModel(ISender mediator, IAppDbContext context, IFileStorageService fileStorage) : PageModel
{
    public ExamDetailDto Exam { get; set; } = null!;
    public List<ExamSeriesOption> ExamSeries { get; set; } = new();
    public List<string> UpdateErrors { get; set; } = new();

    [BindProperty]
    public UpdateExamFormModel UpdateForm { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!await LoadExamAsync(id))
        {
            return RedirectToPage("./Index");
        }

        if (Exam.IsDeleted)
        {
            return RedirectToPage("./Detail", new { id });
        }

        BindUpdateForm();
        await LoadExamSeriesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id)
    {
        // Form posts can drop query-string id; fall back to the bound UpdateForm.Id.
        if (id <= 0)
        {
            id = UpdateForm.Id;
        }

        if (id <= 0 || !await LoadExamAsync(id))
        {
            ErrorMessage = "Exam not found or could not be updated.";
            return RedirectToPage("./Index");
        }

        await LoadExamSeriesAsync();
        UpdateForm.Id = id;

        if (UpdateForm.ExamSeriesId <= 0)
        {
            UpdateErrors.Add("Please select an exam series.");
            return Page();
        }

        if (string.IsNullOrWhiteSpace(UpdateForm.Title))
        {
            UpdateErrors.Add("Exam title is required.");
            return Page();
        }

        if (UpdateForm.Duration <= 0 || UpdateForm.Duration > 120)
        {
            UpdateErrors.Add("Exam duration must be between 1 and 120 minutes.");
            return Page();
        }

        const long maxAudioBytes = 100L * 1024 * 1024;
        if (UpdateForm.AudioFile is { Length: > maxAudioBytes })
        {
            UpdateErrors.Add("Audio file is too large. Maximum size is 100 MB.");
            return Page();
        }

        string? audioUrl = null;
        if (UpdateForm.AudioFile is { Length: > 0 })
        {
            try
            {
                using var stream = UpdateForm.AudioFile.OpenReadStream();
                var uploadResult = await fileStorage.UploadAudioAsync(stream, UpdateForm.AudioFile.FileName);
                audioUrl = uploadResult.Url;
            }
            catch (Exception ex)
            {
                UpdateErrors.Add($"Audio upload error: {ex.Message}");
                return Page();
            }
        }

        try
        {
            var result = await mediator.Send(new UpdateExamCommand
            {
                UpdateExamDto = new UpdateExamDto
                {
                    Id = id,
                    Title = UpdateForm.Title,
                    Description = UpdateForm.Description,
                    Duration = UpdateForm.Duration,
                    ExamSeriesId = UpdateForm.ExamSeriesId,
                    AudioUrl = audioUrl
                }
            });

            if (result.IsSuccess)
            {
                SuccessMessage = "Exam information updated successfully.";
                return RedirectToPage("./Edit", new { id });
            }

            UpdateErrors.Add(result.Error ?? "Failed to update exam.");
        }
        catch (ValidationException ex)
        {
            UpdateErrors.AddRange(ex.Errors.Select(e => e.ErrorMessage));
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, string contentType, int targetId)
    {
        var result = await mediator.Send(new DeleteExamContentCommand
        {
            ExamId = id,
            ContentType = contentType,
            TargetId = targetId
        });

        if (result.IsSuccess)
        {
            SuccessMessage = "Content deleted successfully.";
        }
        else
        {
            ErrorMessage = result.Error ?? "Failed to delete content.";
        }

        return RedirectToPage("./Edit", new { id });
    }

    public async Task<IActionResult> OnPostRemoveAudioAsync(int id)
    {
        if (!await LoadExamAsync(id))
        {
            return RedirectToPage("./Index");
        }

        if (Exam.IsDeleted)
        {
            return RedirectToPage("./Detail", new { id });
        }

        if (string.IsNullOrEmpty(Exam.AudioUrl))
        {
            SuccessMessage = "This exam has no audio to remove.";
            return RedirectToPage("./Edit", new { id });
        }

        var result = await mediator.Send(new UpdateExamCommand
        {
            UpdateExamDto = new UpdateExamDto
            {
                Id = id,
                Title = Exam.Title,
                Description = Exam.Description,
                Duration = Exam.Duration,
                ExamSeriesId = Exam.ExamSeriesId,
                ClearAudio = true
            }
        });

        if (result.IsSuccess)
        {
            SuccessMessage = "Exam audio removed successfully.";
        }
        else
        {
            ErrorMessage = result.Error ?? "Failed to remove exam audio.";
        }

        return RedirectToPage("./Edit", new { id });
    }

    private async Task<bool> LoadExamAsync(int id)
    {
        var result = await mediator.Send(new GetExamDetailQuery { Id = id });
        if (!result.IsSuccess || result.Data == null)
        {
            return false;
        }

        Exam = result.Data;
        return true;
    }

    private void BindUpdateForm()
    {
        UpdateForm = new UpdateExamFormModel
        {
            Id = Exam.Id,
            Title = Exam.Title,
            Description = Exam.Description,
            Duration = Exam.Duration,
            ExamSeriesId = Exam.ExamSeriesId
        };
    }

    private async Task LoadExamSeriesAsync()
    {
        ExamSeries = await context.ExamSeries
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Name)
            .ThenByDescending(s => s.Year)
            .Select(s => new ExamSeriesOption(s.Id, s.Name, s.Year))
            .ToListAsync();
    }
}

public class UpdateExamFormModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Duration { get; set; } = 120;
    public int ExamSeriesId { get; set; }
    public IFormFile? AudioFile { get; set; }
}
