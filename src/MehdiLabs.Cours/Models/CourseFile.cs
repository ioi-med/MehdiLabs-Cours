using System.IO;

namespace MehdiLabs.Cours.Models;

/// <summary>
/// Représente un fichier ou dossier dans l'arborescence de cours.
/// </summary>
public class CourseFile
{
    public string Name { get; set; } = "";
    public string FullPath { get; set; } = "";
    public bool IsDirectory { get; set; }
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public List<CourseFile> Children { get; set; } = new();

    /// <summary>
    /// Icône emoji basée sur le type.
    /// </summary>
    public string Icon => IsDirectory ? "📁" : GetFileIcon();

    /// <summary>
    /// Nom affiché avec icône.
    /// </summary>
    public string DisplayName => $"{Icon} {Name}";

    private string GetFileIcon()
    {
        var ext = Path.GetExtension(Name).ToLowerInvariant();
        if (ext == ".md") return "📄";
        if (ext == ".txt") return "📝";
        if (ext == ".json") return "⚙️";
        if (ext == ".py") return "🐍";
        if (ext == ".cs") return "💎";
        if (ext == ".html" || ext == ".css" || ext == ".js") return "🌐";
        if (ext == ".pdf") return "📕";
        if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".svg") return "🖼️";
        return "📄";
    }
}
