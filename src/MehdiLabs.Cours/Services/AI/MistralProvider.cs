using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MehdiLabs.Cours.Models;

namespace MehdiLabs.Cours.Services.AI;

/// <summary>
/// Provider pour l'API Mistral AI.
/// </summary>
public class MistralProvider : IAIProvider
{
    private readonly string _apiKey;
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(60) };
    private const string ApiUrl = "https://api.mistral.ai/v1/chat/completions";

    public string Name => "Mistral";
    public string DefaultModel => "mistral-small-latest";
    public IReadOnlyList<string> AvailableModels => new[]
    {
        "mistral-small-latest",
        "mistral-large-latest",
        "mistral-medium-latest",
        "open-mistral-nemo"
    };

    public MistralProvider(string apiKey) => _apiKey = apiKey;

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

        try
        {
            var response = await Http.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw (int)response.StatusCode switch
                {
                    401 => new AIProviderException("Mistral", "Clé API invalide.", 401),
                    429 => new AIProviderException("Mistral", "Quota dépassé. Réessaie plus tard.", 429),
                    _ => new AIProviderException("Mistral", $"Erreur serveur (HTTP {(int)response.StatusCode}).", (int)response.StatusCode)
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
        catch (TaskCanceledException) { throw new AIProviderException("Mistral", "Timeout — le serveur met trop de temps à répondre."); }
        catch (HttpRequestException) { throw new AIProviderException("Mistral", "Impossible de se connecter au serveur."); }
        catch (Exception ex) { throw new AIProviderException("Mistral", $"Erreur inattendue : {ex.Message}"); }
    }
}
