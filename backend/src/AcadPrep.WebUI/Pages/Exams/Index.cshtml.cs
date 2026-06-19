using AcadPrep.Application.Common.Models;
using Application.Features.Exam.Queries.Common.DTOs;
using Application.Features.Exam.Queries.GetExamList;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcadPrep.WebUI.Pages.Exams
{
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;

        public IndexModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ===== Bind Properties (Query String Parameters) =====
        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedSeries { get; set; } = "All";

        [BindProperty(SupportsGet = true)]
        public string SelectedYear { get; set; } = "All";

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 6;

        // ===== Filter Options (Dynamic from DB) =====
        public List<string> SeriesFilters { get; set; } = new();
        public List<string> YearFilters { get; set; } = new();

        // ===== Data Properties =====
        public PaginatedList<GetExamDto>? ExamsPaginated { get; set; }
        public IReadOnlyCollection<GetExamDto> Exams => ExamsPaginated?.Items ?? Array.Empty<GetExamDto>();
        public int TotalPages => ExamsPaginated?.TotalPage ?? 1;
        public bool HasPreviousPage => ExamsPaginated?.HasPreviousPage ?? false;
        public bool HasNextPage => ExamsPaginated?.HasNextPage ?? false;
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            // Build query from filter parameters
            var query = new GetExamListQuery
            {
                Search = string.IsNullOrWhiteSpace(Search) ? null : Search,
                SeriesName = SelectedSeries == "All" ? null : SelectedSeries,
                Year = SelectedYear == "All" ? null : int.TryParse(SelectedYear, out var year) ? year : null,
                PageIndex = CurrentPage,
                PageSize = PageSize
            };
            
            var result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data is not null)
            {
                ExamsPaginated = result.Data.Exams;
                SeriesFilters = result.Data.SeriesFilters;
                YearFilters = result.Data.YearFilters;
            }
            else
            {
                ErrorMessage = result.Error ?? "An error occurred while loading the exam list.";
                ExamsPaginated = new PaginatedList<GetExamDto>(
                    Array.Empty<GetExamDto>(), 0, 1, PageSize, true);
                
                SeriesFilters = new List<string> { "All" };
                YearFilters = new List<string> { "All" };
            }
        }
    }
}
