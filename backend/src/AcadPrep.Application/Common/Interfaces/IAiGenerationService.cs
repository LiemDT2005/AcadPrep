using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces;

public interface IAiGenerationService
{
    /// <summary>
    /// Generates a contextual passage for a given vocabulary word.
    /// </summary>
    /// <param name="word">The vocabulary word to generate context for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A generated passage containing the vocabulary word.</returns>
    Task<string> GenerateVocabularyContextAsync(string word, CancellationToken cancellationToken = default);
}
