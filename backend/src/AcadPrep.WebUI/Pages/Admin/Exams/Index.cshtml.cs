using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AcadPrep.Application.Features.Admin.Exams.Commands.CreateExam;
using AcadPrep.Application.Features.Admin.Exams.Queries.Common.DTOs;
using AcadPrep.Application.Features.Admin.Exams.Queries.GetAdminExamList;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.WebUI.Pages.Admin.Exams;

public class IndexModel(ISender mediator, IAppDbContext context, IFileStorageService fileStorage) : PageModel
{
    public List<AdminExamDto> Exams { get; set; } = new();
    public List<ExamSeriesOption> ExamSeries { get; set; } = new();
    public bool ShowCreateModal { get; set; }
    public List<string> CreateExamErrors { get; set; } = new();

    [BindProperty]
    public CreateExamFormModel CreateExamForm { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadPageDataAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        await LoadPageDataAsync();
        ShowCreateModal = true;

        if (CreateExamForm.ExamSeriesId <= 0)
        {
            CreateExamErrors.Add("Please select an exam series.");
            return Page();
        }

        if (string.IsNullOrWhiteSpace(CreateExamForm.Title))
        {
            CreateExamErrors.Add("Exam title is required.");
            return Page();
        }

        if (CreateExamForm.Duration <= 0)
        {
            CreateExamErrors.Add("Exam duration must be greater than 0 minutes.");
            return Page();
        }

        const long maxAudioBytes = 100L * 1024 * 1024;
        if (CreateExamForm.AudioFile is { Length: > maxAudioBytes })
        {
            CreateExamErrors.Add("Audio file is too large. Maximum size is 100 MB.");
            return Page();
        }

        string? audioUrl = null;
        if (CreateExamForm.AudioFile is { Length: > 0 })
        {
            try
            {
                using var stream = CreateExamForm.AudioFile.OpenReadStream();
                var uploadResult = await fileStorage.UploadAudioAsync(stream, CreateExamForm.AudioFile.FileName);
                audioUrl = uploadResult.Url;
            }
            catch (Exception ex)
            {
                CreateExamErrors.Add($"Audio upload error: {ex.Message}");
                return Page();
            }
        }

        var dto = new CreateExamDto
        {
            Title = CreateExamForm.Title,
            Description = CreateExamForm.Description,
            Duration = CreateExamForm.Duration,
            ExamSeriesId = CreateExamForm.ExamSeriesId,
            AudioUrl = audioUrl
        };

        try
        {
            var result = await mediator.Send(new CreateExamCommand { CreateExamDto = dto });

            if (result.IsSuccess)
            {
                return RedirectToPage("./Edit", new { id = result.Data });
            }

            CreateExamErrors.Add(result.Error ?? "Failed to create exam.");
        }
        catch (ValidationException ex)
        {
            CreateExamErrors.AddRange(
                ex.Errors.Select(e => string.IsNullOrEmpty(e.PropertyName)
                    ? e.ErrorMessage
                    : $"{e.ErrorMessage}"));
        }

        return Page();
    }

    private async Task LoadPageDataAsync()
    {
        var result = await mediator.Send(new GetAdminExamListQuery());
        if (result.IsSuccess && result.Data != null)
        {
            Exams = result.Data;
        }

        ExamSeries = await context.ExamSeries
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Name)
            .ThenByDescending(s => s.Year)
            .Select(s => new ExamSeriesOption(s.Id, s.Name, s.Year))
            .ToListAsync();
    }
}

public class CreateExamFormModel
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Duration { get; set; } = 120;
    public int ExamSeriesId { get; set; }
    public IFormFile? AudioFile { get; set; }
}

public record ExamSeriesOption(int Id, string Name, int Year);
