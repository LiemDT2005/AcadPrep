using AcadPrep.Application.Common.Models;
using Application.Common.Models;
using Application.Features.Courses.Queries.Common.DTOs;
using Application.Features.Exam.Queries.Common.DTOs;
using MediatR;

namespace Application.Features.Exam.Queries.GetExamList;

public record GetExamListQuery : IRequest<Result<GetExamListResponse>>
{
    public string? Search { get; init; }
    public string? SeriesName { get; init; }
    public int? Year { get; init; }
    public int PageIndex { get; init; } = 1;    
    public int PageSize { get; init; } = 10;
}