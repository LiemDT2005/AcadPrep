using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces;

public interface ITranslationService
{
    Task<string?> TranslateToVietnameseAsync(string word, CancellationToken cancellationToken = default);
}
