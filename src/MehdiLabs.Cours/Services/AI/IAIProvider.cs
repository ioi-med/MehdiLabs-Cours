using MehdiLabs.Cours.Models;

namespace MehdiLabs.Cours.Services.AI;

/// <summary>
/// Interface abstraite pour les providers IA.
/// Inspiré de l'architecture PKHex (ISaveHandler).
/// </summary>
public interface IAIProvider
{
    /// <summary>Nom du provider (ex: "Mistral", "GPT").</summary>
    string Name { get; }

    /// <summary>Modèle par défaut du provider.</summary>
    string DefaultModel { get; }

    /// <summary>Liste des modèles disponibles.</summary>
    IReadOnlyList<string> AvailableModels { get; }

    /// <summary>
    /// Envoie un message au provider et retourne la réponse.
    /// </summary>
    Task<string> SendMessageAsync(List<ChatMessage> messages, string? model = null,
                                   CancellationToken cancellationToken = default);
}
