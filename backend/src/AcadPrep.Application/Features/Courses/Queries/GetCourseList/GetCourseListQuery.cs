using System.Collections.Generic;
using Application.Features.Courses.Queries.Common.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Courses.Queries.GetCourseList;

public record GetCourseListQuery : IRequest<Result<List<GetCourseDto>>>;
