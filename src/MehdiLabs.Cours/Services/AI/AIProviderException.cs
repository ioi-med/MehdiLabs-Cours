namespace MehdiLabs.Cours.Services.AI;

/// <summary>
/// Exception typée pour les erreurs de providers IA.
/// </summary>
public class AIProviderException : Exception
{
    public string Provider { get; }
    public int? StatusCode { get; }

    public AIProviderException(string provider, string message, int? statusCode = null)
        : base($"[{provider}] {message}")
    {
        Provider = provider;
        StatusCode = statusCode;
    }
}
