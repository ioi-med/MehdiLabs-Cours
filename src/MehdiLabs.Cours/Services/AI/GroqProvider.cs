using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MehdiLabs.Cours.Models;

namespace MehdiLabs.Cours.Services.AI;

/// <summary>
/// Provider pour l'API Groq (gratuit, compatible OpenAI).
/// </summary>
public class GroqProvider : IAIProvider
{
    private readonly string _apiKey;
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(60) };
    private const string ApiUrl = "https://api.groq.com/openai/v1/chat/completions";

    public string Name => "Groq";
    public string DefaultModel => "openai/gpt-oss-120b";
    public IReadOnlyList<string> AvailableModels => new[]
    {
        "openai/gpt-oss-120b",
        "openai/gpt-oss-20b",
        "qwen/qwen3.8-27b",
        "groq/compound"
    };

    public GroqProvider(string apiKey) => _apiKey = apiKey;

    public async Task<string> SendMessageAsync(List<ChatMessage> messages, string? model = null,
                                                CancellationToken ct = default)
    {
        model ??= DefaultModel;
        var payload = new
        {
            model,
            messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToArray()
        };

        var json = JsonSerializer.Serialize(payload);
        var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        // User-Agent pour éviter le blocage Cloudflare
        request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

        try
        {
            var response = await Http.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                // Extraire le message d'erreur de l'API si possible
                var errorDetail = "";
                try
                {
                    using var errorDoc = JsonDocument.Parse(body);
                    if (errorDoc.RootElement.TryGetProperty("error", out var errorObj) &&
                        errorObj.TryGetProperty("message", out var msgProp))
                    {
                        errorDetail = msgProp.GetString() ?? "";
                    }
                }
                catch { }

                throw (int)response.StatusCode switch
                {
                    401 => new AIProviderException("Groq", "Clé API invalide.", 401),
                    429 => new AIProviderException("Groq", "Quota dépassé. Réessaie plus tard.", 429),
                    _ => new AIProviderException("Groq", string.IsNullOrEmpty(errorDetail) 
                        ? $"Erreur serveur (HTTP {(int)response.StatusCode})." 
                        : $"Erreur (HTTP {(int)response.StatusCode}): {errorDetail}", (int)response.StatusCode)
                };
            }

            using var doc = JsonDocument.Parse(body);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";
        }
        catch (AIProviderException) { throw; }
        catch (TaskCanceledException) { throw new AIProviderException("Groq", "Timeout — le serveur met trop de temps à répondre."); }
        catch (HttpRequestException) { throw new AIProviderException("Groq", "Impossible de se connecter au serveur."); }
        catch (Exception ex) { throw new AIProviderException("Groq", $"Erreur inattendue : {ex.Message}"); }
    }
}
