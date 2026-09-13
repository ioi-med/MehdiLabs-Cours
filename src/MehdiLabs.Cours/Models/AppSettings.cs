namespace MehdiLabs.Cours.Models;

/// <summary>
/// Configuration persistante de l'application (sérialisable en JSON).
/// </summary>
public class AppSettings
{
    public Dictionary<string, string> ApiKeys { get; set; } = new()
    {
        ["Mistral"] = "",
        ["Gemini"] = "",
        ["DeepSeek"] = "",
        ["GPT"] = "",
        ["Claude"] = "",
        ["Groq"] = "",
    };

    public string DefaultProvider { get; set; } = "";
    public string Theme { get; set; } = "dark";
    public string Language { get; set; } = "fr";
    public int AutoSaveInterval { get; set; } = 30;
    public int EditorFontSize { get; set; } = 14;

    public Dictionary<string, string> Models { get; set; } = new()
    {
        ["Mistral"] = "mistral-small-latest",
        ["Gemini"] = "gemini-3.6-flash",
        ["DeepSeek"] = "deepseek-chat",
        ["GPT"] = "gpt-4o",
        ["Claude"] = "claude-3-5-sonnet-latest",
        ["Groq"] = "openai/gpt-oss-120b",
    };

    public List<string> RecentFiles { get; set; } = new();
    public string LastOpenedFile { get; set; } = "";
}
