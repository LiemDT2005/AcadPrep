using System.Collections.Generic;
using System.Linq;
using Domain.Entities;

namespace Application.Common.Validation;

public static class Part7ReadingSetValidator
{
    public const int MinPassages = 1;
    public const int MaxPassages = 3;
    public const int MinQuestions = 2;
    public const int MaxQuestions = 5;

    public static IReadOnlyList<string> Validate(
        QuestionGroup group,
        IReadOnlyList<Passage> passages,
        IReadOnlyList<Question> questions)
    {
        var errors = new List<string>();

        if (passages.Count is < MinPassages or > MaxPassages)
        {
            errors.Add($"Part 7 reading set must have {MinPassages}-{MaxPassages} passages (found {passages.Count}).");
        }

        if (questions.Count is < MinQuestions or > MaxQuestions)
        {
            errors.Add($"Part 7 reading set must have {MinQuestions}-{MaxQuestions} questions (found {questions.Count}).");
        }

        foreach (var passage in passages)
        {
            if (string.IsNullOrWhiteSpace(passage.Content) && string.IsNullOrWhiteSpace(passage.ImageUrl))
            {
                errors.Add($"Passage #{passage.Id} must have text content or an image.");
            }

            if (passage.QuestionGroupId != group.Id)
            {
                errors.Add($"Passage #{passage.Id} must belong to question group #{group.Id}.");
            }

            if (passage.ExamId != group.ExamId)
            {
                errors.Add($"Passage #{passage.Id} must belong to the same exam as the reading set.");
            }
        }

        foreach (var question in questions)
        {
            if (question.Part != 7)
            {
                errors.Add($"Question #{question.QuestionNumber} must be Part 7.");
            }

            if (question.QuestionGroupId != group.Id)
            {
                errors.Add($"Question #{question.QuestionNumber} must belong to question group #{group.Id}.");
            }

            if (question.ExamId != group.ExamId)
            {
                errors.Add($"Question #{question.QuestionNumber} must belong to the same exam as the reading set.");
            }
        }

        var duplicateOrders = passages
            .GroupBy(p => p.DisplayOrder)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateOrders.Count > 0)
        {
            errors.Add($"Passage display orders must be unique within a reading set (duplicates: {string.Join(", ", duplicateOrders)}).");
        }

        return errors;
    }
}
