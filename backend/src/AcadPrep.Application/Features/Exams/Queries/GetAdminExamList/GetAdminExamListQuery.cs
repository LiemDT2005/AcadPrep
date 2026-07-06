using System.Collections.Generic;
using AcadPrep.Application.Common.Models;
using Application.Features.Exams.Queries.Common.DTOs;
using MediatR;

namespace Application.Features.Exams.Queries.GetAdminExamList;

public record GetAdminExamListQuery : IRequest<Result<List<AdminExamDto>>>;
