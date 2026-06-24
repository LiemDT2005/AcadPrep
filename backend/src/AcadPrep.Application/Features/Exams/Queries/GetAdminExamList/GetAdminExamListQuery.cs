using System.Collections.Generic;
using Application.Common.Models;
using Application.Features.Exams.Queries.Common.DTOs;
using MediatR;

namespace Application.Features.Exams.Queries.GetAdminExamList;

public record GetAdminExamListQuery(int PageNumber = 1, int PageSize = 100) : IRequest<Result<List<AdminExamDto>>>;
