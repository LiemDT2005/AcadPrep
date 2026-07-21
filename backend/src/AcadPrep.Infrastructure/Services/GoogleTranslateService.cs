using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Application.Common.Interfaces;

namespace Infrastructure.Services;

public class GoogleTranslateService : ITranslationService
{
    private readonly HttpClient _httpClient;

    public GoogleTranslateService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> TranslateToVietnameseAsync(string word, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(word))
            return null;

        try
        {
            var encoded = HttpUtility.UrlEncode(word.Trim());
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=vi&dt=t&q={encoded}";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);

            var root = doc.RootElement;
            if (root.GetArrayLength() > 0)
            {
                var sentences = root[0];
                if (sentences.ValueKind == JsonValueKind.Array && sentences.GetArrayLength() > 0)
                {
                    var translation = sentences[0][0].GetString();
                    return translation;
                }
            }
        }
        catch (Exception)
        {
            // Silently fail — word will be saved without translation
        }

        return null;
    }
}
