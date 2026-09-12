using System.Net.Http;
using System.Text;
using System.Text.Json;
using MehdiLabs.Cours.Models;

namespace MehdiLabs.Cours.Services.AI;

/// <summary>
/// Provider pour l'API Anthropic Claude.
/// </summary>
public class ClaudeProvider : IAIProvider
{
    private readonly string _apiKey;
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(60) };
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";

    public string Name => "Claude";
    public string DefaultModel => "claude-sonnet-4-20250514";
    public IReadOnlyList<string> AvailableModels => new[]
    {
        "claude-sonnet-4-20250514",
        "claude-3-5-sonnet-20241022",
        "claude-3-5-haiku-20241022",
        "claude-3-opus-20240229"
    };

    public ClaudeProvider(string apiKey) => _apiKey = apiKey;

    public async Task<string> SendMessageAsync(List<ChatMessage> messages, string? model = null,
                                                CancellationToken ct = default)
    {
        model ??= DefaultModel;

        string? systemText = null;
        var apiMessages = new List<object>();
        foreach (var msg in messages)
        {
            if (msg.Role == "system")
            {
                systemText = msg.Content;
            }
            else
            {
                apiMessages.Add(new { role = msg.Role, content = msg.Content });
            }
        }

        var payloadDict = new Dictionary<string, object>
        {
            ["model"] = model,
            ["max_tokens"] = 4096,
            ["messages"] = apiMessages
        };
        if (systemText != null)
            payloadDict["system"] = systemText;

        var json = JsonSerializer.Serialize(payloadDict);
        var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("x-api-key", _apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        try
        {
            var response = await Http.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw (int)response.StatusCode switch
                {
                    401 => new AIProviderException("Claude", "Clé API invalide.", 401),
                    429 => new AIProviderException("Claude", "Quota dépassé. Réessaie plus tard.", 429),
                    _ => new AIProviderException("Claude", $"Erreur serveur (HTTP {(int)response.StatusCode}).", (int)response.StatusCode)
                };
            }

            using var doc = JsonDocument.Parse(body);
            var contentBlocks = doc.RootElement.GetProperty("content");
            var texts = new List<string>();
            foreach (var block in contentBlocks.EnumerateArray())
            {
                if (block.GetProperty("type").GetString() == "text")
                    texts.Add(block.GetProperty("text").GetString() ?? "");
            }
            return string.Join("\n", texts);
        }
        catch (AIProviderException) { throw; }
        catch (TaskCanceledException) { throw new AIProviderException("Claude", "Timeout — le serveur met trop de temps à répondre."); }
        catch (HttpRequestException) { throw new AIProviderException("Claude", "Impossible de se connecter au serveur."); }
        catch (Exception ex) { throw new AIProviderException("Claude", $"Erreur inattendue : {ex.Message}"); }
    }
}
