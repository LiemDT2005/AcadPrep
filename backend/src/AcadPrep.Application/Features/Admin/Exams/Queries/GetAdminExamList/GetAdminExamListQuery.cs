using System.Collections.Generic;
using AcadPrep.Application.Common.Models;
using AcadPrep.Application.Features.Admin.Exams.Queries.Common.DTOs;
using MediatR;

namespace AcadPrep.Application.Features.Admin.Exams.Queries.GetAdminExamList;

public record GetAdminExamListQuery : IRequest<Result<List<AdminExamDto>>>;
