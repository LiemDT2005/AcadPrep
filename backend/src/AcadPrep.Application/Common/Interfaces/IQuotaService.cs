using System.Threading;
using System.Threading.Tasks;
using AcadPrep.Application.Common.Models;

namespace Application.Common.Interfaces;

public interface IQuotaService
{
    Task<Result<int>> CheckAndConsumeAsync(int userId, bool isPro, int tokenAmount, CancellationToken cancellationToken = default);
}
