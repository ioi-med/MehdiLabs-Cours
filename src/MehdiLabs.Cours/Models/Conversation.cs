namespace MehdiLabs.Cours.Models;

/// <summary>
/// Message individuel dans une conversation IA.
/// </summary>
public class ChatMessage
{
    public string Role { get; set; } = "user"; // "user", "assistant", "system"
    public string Content { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

/// <summary>
/// Conversation complète avec historique de messages.
/// </summary>
public class Conversation
{
    public string Id { get; set; } = Guid.NewGuid().ToString()[..8];
    public string Title { get; set; } = "";
    public string Created { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    public List<ChatMessage> Messages { get; set; } = new();
}
