using AcadPrep.Application.Common.Models;
using Application.Features.Exam.Queries.Common.DTOs;

namespace Application.Features.Exam.Queries.GetExamList;

public class GetExamListResponse
{
    public PaginatedList<GetExamDto> Exams { get; set; } = null!;
    public List<string> SeriesFilters { get; set; } = new();
    public List<string> YearFilters { get; set; } = new();
}
