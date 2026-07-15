using System.Collections.Generic;
using System.Linq;
using AcadPrep.Application.Features.Admin.Exams.Queries.Common.DTOs;

namespace AcadPrep.WebUI.Pages.Admin.Exams;

public class ExamStructureViewModel
{
    public required ExamDetailDto Exam { get; init; }
    public bool IsReadOnly { get; init; }

    public string? SuccessMessage { get; init; }
    public string? ErrorMessage { get; init; }

    public const int TargetP1 = 6;
    public const int TargetP2 = 25;
    public const int TargetP3 = 39;
    public const int TargetP4 = 30;
    public const int TargetP5 = 30;
    public const int TargetP6 = 16;
    public const int TargetP7 = 54;

    public List<QuestionDetailDto> Part1Questions { get; private set; } = new();
    public List<QuestionDetailDto> Part2Questions { get; private set; } = new();
    public List<IGrouping<int?, QuestionDetailDto>> Part3Groups { get; private set; } = new();
    public List<IGrouping<int?, QuestionDetailDto>> Part4Groups { get; private set; } = new();
    public List<QuestionDetailDto> Part5Questions { get; private set; } = new();
    public List<IGrouping<int?, QuestionDetailDto>> Part6Groups { get; private set; } = new();
    public List<ReadingSetDto> Part7Sets { get; private set; } = new();
    public List<QuestionDetailDto> Part7Orphans { get; private set; } = new();

    public int Part1Count { get; private set; }
    public int Part2Count { get; private set; }
    public int Part3Count { get; private set; }
    public int Part4Count { get; private set; }
    public int Part5Count { get; private set; }
    public int Part6Count { get; private set; }
    public int Part7Count { get; private set; }
    public int TotalQuestions { get; private set; }

    public bool CanAddP1 => Part1Count < TargetP1;
    public bool CanAddP2 => Part2Count < TargetP2;
    public bool CanAddP3 => Part3Count + 3 <= TargetP3;
    public bool CanAddP4 => Part4Count + 3 <= TargetP4;
    public bool CanAddP5 => Part5Count < TargetP5;
    public bool CanAddP6 => Part6Count + 4 <= TargetP6;
    public bool CanAddP7 => Part7Count + 2 <= TargetP7;

    public static ExamStructureViewModel From(
        ExamDetailDto exam,
        bool isReadOnly,
        string? successMessage = null,
        string? errorMessage = null)
    {
        var vm = new ExamStructureViewModel
        {
            Exam = exam,
            IsReadOnly = isReadOnly,
            SuccessMessage = successMessage,
            ErrorMessage = errorMessage
        };
        vm.Build();
        return vm;
    }

    private void Build()
    {
        Part1Questions = Exam.Questions.Where(q => q.Part == 1).OrderBy(q => q.QuestionNumber).ToList();
        Part2Questions = Exam.Questions.Where(q => q.Part == 2).OrderBy(q => q.QuestionNumber).ToList();
        Part3Groups = Exam.Questions.Where(q => q.Part == 3).GroupBy(q => q.QuestionGroupId).ToList();
        Part4Groups = Exam.Questions.Where(q => q.Part == 4).GroupBy(q => q.QuestionGroupId).ToList();
        Part5Questions = Exam.Questions.Where(q => q.Part == 5).OrderBy(q => q.QuestionNumber).ToList();
        Part6Groups = Exam.Questions.Where(q => q.Part == 6).GroupBy(q => q.PassageId)
            .OrderBy(g => g.Min(q => q.PassageDisplayOrder ?? q.QuestionNumber))
            .ToList();
        Part7Sets = Exam.Part7ReadingSets;
        Part7Orphans = Exam.Questions.Where(q => q.Part == 7 && !q.QuestionGroupId.HasValue)
            .OrderBy(q => q.QuestionNumber)
            .ToList();

        Part1Count = Part1Questions.Count;
        Part2Count = Part2Questions.Count;
        Part3Count = Exam.Questions.Count(q => q.Part == 3);
        Part4Count = Exam.Questions.Count(q => q.Part == 4);
        Part5Count = Part5Questions.Count;
        Part6Count = Exam.Questions.Count(q => q.Part == 6);
        Part7Count = Part7Sets.Sum(s => s.Questions.Count) + Part7Orphans.Count;
        TotalQuestions = Part1Count + Part2Count + Part3Count + Part4Count + Part5Count + Part6Count + Part7Count;
    }
}

public class ExamPageHeroViewModel
{
    public required ExamDetailDto Exam { get; init; }
    public required int TotalQuestions { get; init; }
    public required bool IsReadOnly { get; init; }
    public string BreadcrumbCurrent { get; init; } = "Exam";
    public string HeroIcon { get; init; } = "quiz";
}
