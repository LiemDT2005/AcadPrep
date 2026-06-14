using Application.Features.Courses.Queries.GetCourseList;
using Application.Features.Courses.Queries.Common.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcadPrep.WebUI.Pages.Courses
{
    public class IndexModel : PageModel
    {
        private readonly ISender _mediator;

        public IndexModel(ISender mediator)
        {
            _mediator = mediator;
        }

        public List<GetCourseDto> Courses { get; set; } = new();

        public async Task OnGetAsync()
        {
            var response = await _mediator.Send(new GetCourseListQuery());
            
            if (response.IsSuccess && response.Data != null)
            {
                Courses = response.Data;
            }
        }
    }
}
