using System.IO;
using System.Text.Json;
using MehdiLabs.Cours.Models;

namespace MehdiLabs.Cours.Services;

/// <summary>
/// Service de gestion des conversations IA (persistence JSON).
/// Équivalent de la gestion d'historique dans chatbot.py.
/// </summary>
public class ConversationService
{
    private readonly string _historyDir;

    public ConversationService(string coursDir)
    {
        _historyDir = Path.Combine(coursDir, ".chat_history");
        Directory.CreateDirectory(_historyDir);
    }

    /// <summary>
    /// Charge toutes les conversations triées par date décroissante.
    /// </summary>
    public List<Conversation> LoadAll()
    {
        var conversations = new List<Conversation>();

        try
        {
            foreach (var file in Directory.GetFiles(_historyDir, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file, System.Text.Encoding.UTF8);
                    var conv = JsonSerializer.Deserialize<Conversation>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (conv != null)
                        conversations.Add(conv);
                }
                catch { }
            }
        }
        catch { }

        return conversations.OrderByDescending(c => c.Created).ToList();
    }

    /// <summary>
    /// Charge une conversation spécifique.
    /// </summary>
    public Conversation? Load(string convId)
    {
        var path = Path.Combine(_historyDir, $"{convId}.json");
        if (!File.Exists(path)) return null;

        try
        {
            var json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            return JsonSerializer.Deserialize<Conversation>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch { return null; }
    }

    /// <summary>
    /// Sauvegarde une conversation.
    /// </summary>
    public void Save(Conversation conversation)
    {
        try
        {
            var path = Path.Combine(_historyDir, $"{conversation.Id}.json");
            var json = JsonSerializer.Serialize(conversation, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(path, json, System.Text.Encoding.UTF8);
        }
        catch { }
    }

    /// <summary>
    /// Supprime une conversation.
    /// </summary>
    public bool Delete(string convId)
    {
        try
        {
            var path = Path.Combine(_historyDir, $"{convId}.json");
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
        }
        catch { return false; }
    }

    /// <summary>
    /// Crée une nouvelle conversation vide.
    /// </summary>
    public Conversation Create(string? title = null)
    {
        var conv = new Conversation
        {
            Title = title ?? $"Conversation {DateTime.Now:yyyy-MM-dd HH:mm}"
        };
        Save(conv);
        return conv;
    }
}
