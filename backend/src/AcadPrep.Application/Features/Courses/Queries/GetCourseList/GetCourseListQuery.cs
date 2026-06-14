using System.Collections.Generic;
using Application.Common.Models;
using Application.Features.Courses.Queries.Common.DTOs;
using MediatR;

namespace Application.Features.Courses.Queries.GetCourseList;

public record GetCourseListQuery : IRequest<Result<List<GetCourseDto>>>;
