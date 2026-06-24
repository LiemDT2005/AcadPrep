using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;

namespace Infrastructure.Services;

public class MockAiGenerationService : IAiGenerationService
{
    public Task<string> GenerateVocabularyContextAsync(string word, CancellationToken cancellationToken = default)
    {
        // Simulate some processing delay
        Task.Delay(500, cancellationToken).Wait();
        
        string contextPassage = $"[Mock AI Generated Context]: The word '{word}' is very important in the English language. " +
                                $"For example, you might say: 'I learned the word {word} today, and I am excited to use it in a sentence.' " +
                                $"Understanding '{word}' will significantly improve your reading comprehension.";

        return Task.FromResult(contextPassage);
    }
}
