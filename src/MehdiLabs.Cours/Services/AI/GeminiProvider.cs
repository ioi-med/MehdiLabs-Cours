using System.Net.Http;
using System.Text;
using System.Text.Json;
using MehdiLabs.Cours.Models;

namespace MehdiLabs.Cours.Services.AI;

/// <summary>
/// Provider pour l'API Google Gemini.
/// </summary>
public class GeminiProvider : IAIProvider
{
    private readonly string _apiKey;
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(60) };
    private const string ApiBase = "https://generativelanguage.googleapis.com/v1beta/models";

    public string Name => "Gemini";
    public string DefaultModel => "gemini-2.5-flash";
    public IReadOnlyList<string> AvailableModels => new[]
    {
        "gemini-2.5-flash",
        "gemini-3.5-flash",
        "gemini-3.8-flash",
        "gemini-3.1-pro-preview"
    };

    public GeminiProvider(string apiKey) => _apiKey = apiKey;

    public async Task<string> SendMessageAsync(List<ChatMessage> messages, string? model = null,
                                                CancellationToken ct = default)
    {
        model ??= DefaultModel;
        var url = $"{ApiBase}/{model}:generateContent?key={_apiKey}";

        string? systemInstruction = null;
        var geminiContents = new List<object>();

        foreach (var msg in messages)
        {
            if (msg.Role == "system")
            {
                systemInstruction = msg.Content;
                continue;
            }
            var role = msg.Role == "user" ? "user" : "model";
            geminiContents.Add(new
            {
                role,
                parts = new[] { new { text = msg.Content } }
            });
        }

        var payload = new Dictionary<string, object> { ["contents"] = geminiContents };
        if (systemInstruction != null)
        {
            payload["systemInstruction"] = new { parts = new[] { new { text = systemInstruction } } };
        }

        var json = JsonSerializer.Serialize(payload);
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        try
        {
            var response = await Http.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw (int)response.StatusCode switch
                {
                    400 => new AIProviderException("Gemini", "Clé API invalide ou requête malformée.", 400),
                    429 => new AIProviderException("Gemini", "Quota dépassé. Réessaie plus tard.", 429),
                    _ => new AIProviderException("Gemini", $"Erreur serveur (HTTP {(int)response.StatusCode}).", (int)response.StatusCode)
                };
            }

            using var doc = JsonDocument.Parse(body);
            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";
        }
        catch (AIProviderException) { throw; }
        catch (TaskCanceledException) { throw new AIProviderException("Gemini", "Timeout — le serveur met trop de temps à répondre."); }
        catch (HttpRequestException) { throw new AIProviderException("Gemini", "Impossible de se connecter au serveur."); }
        catch (Exception ex) { throw new AIProviderException("Gemini", $"Erreur inattendue : {ex.Message}"); }
    }
}
