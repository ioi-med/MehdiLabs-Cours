namespace MehdiLabs.Cours.Services.AI;

/// <summary>
/// Factory / Registry des providers IA (pattern Factory comme PKHex).
/// Permet d'instancier un provider par son nom.
/// </summary>
public static class AIProviderFactory
{
    /// <summary>
    /// Registre des providers disponibles.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, Func<string, IAIProvider>> Providers = new Dictionary<string, Func<string, IAIProvider>>
    {
        ["Mistral"] = key => new MistralProvider(key),
        ["Gemini"] = key => new GeminiProvider(key),
        ["DeepSeek"] = key => new DeepSeekProvider(key),
        ["GPT"] = key => new OpenAIProvider(key),
        ["Claude"] = key => new ClaudeProvider(key),
        ["Groq"] = key => new GroqProvider(key),
    };

    /// <summary>
    /// Crée une instance du provider spécifié.
    /// </summary>
    public static IAIProvider Create(string name, string apiKey)
    {
        if (!Providers.TryGetValue(name, out var factory))
            throw new ArgumentException($"Provider '{name}' inconnu. Disponibles: {string.Join(", ", Providers.Keys)}");
        return factory(apiKey);
    }

    /// <summary>
    /// Liste des noms de providers disponibles.
    /// </summary>
    public static IReadOnlyList<string> ProviderNames => Providers.Keys.ToList();

    /// <summary>
    /// Retourne les modèles disponibles pour un provider donné.
    /// </summary>
    public static IReadOnlyList<string> GetModels(string providerName)
    {
        if (!Providers.TryGetValue(providerName, out var factory))
            return Array.Empty<string>();
        // Créer une instance temporaire pour obtenir la liste des modèles
        var tempProvider = factory("");
        return tempProvider.AvailableModels;
    }
}
