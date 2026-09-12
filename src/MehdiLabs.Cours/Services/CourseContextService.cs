using System.IO;

namespace MehdiLabs.Cours.Services;

/// <summary>
/// Service de scan des cours pour injecter du contexte dans les conversations IA.
/// Équivalent de _scan_cours() dans chatbot.py.
/// </summary>
public class CourseContextService
{
    private readonly string _coursDir;
    private const int MaxContextChars = 6000;

    public CourseContextService(string coursDir)
    {
        _coursDir = coursDir;
    }

    /// <summary>
    /// Scanne les fichiers .md du dossier cours et retourne un résumé structuré.
    /// </summary>
    public string ScanCourses()
    {
        var courseTexts = new List<string>();
        int totalChars = 0;

        try
        {
            foreach (var file in Directory.EnumerateFiles(_coursDir, "*.md", SearchOption.AllDirectories))
            {
                // Ignorer les dossiers cachés
                if (file.Contains(Path.DirectorySeparatorChar + "."))
                    continue;

                try
                {
                    var content = File.ReadAllText(file, System.Text.Encoding.UTF8).Trim();
                    if (string.IsNullOrEmpty(content))
                        continue;

                    var relPath = Path.GetRelativePath(_coursDir, file);

                    // Limiter chaque fichier
                    int maxPerFile = MaxContextChars / Math.Max(1, courseTexts.Count + 1);
                    if (content.Length > maxPerFile)
                        content = content[..maxPerFile] + "\n[... tronqué ...]";

                    var entry = $"--- Fichier : {relPath} ---\n{content}\n";
                    totalChars += entry.Length;

                    if (totalChars > MaxContextChars)
                        break;

                    courseTexts.Add(entry);
                }
                catch { }
            }
        }
        catch { }

        if (courseTexts.Count == 0)
            return "";

        return "\n\n=== COURS DE L'ÉTUDIANT (pour contexte) ===\n"
             + string.Join("\n", courseTexts)
             + "\n=== FIN DES COURS ===\n";
    }

    /// <summary>
    /// Liste les fichiers .md disponibles pour l'attachment.
    /// </summary>
    public List<(string RelativePath, string FullPath)> ListAvailableCourses()
    {
        var files = new List<(string, string)>();
        try
        {
            foreach (var file in Directory.EnumerateFiles(_coursDir, "*.md", SearchOption.AllDirectories))
            {
                if (file.Contains(Path.DirectorySeparatorChar + "."))
                    continue;

                var relPath = Path.GetRelativePath(_coursDir, file);
                files.Add((relPath, file));
            }
        }
        catch { }
        return files.OrderBy(f => f.Item1).ToList();
    }

    /// <summary>
    /// Lit le contenu d'un fichier cours pour l'attacher à une conversation.
    /// </summary>
    public string? ReadCourseContent(string filepath)
    {
        try
        {
            return File.ReadAllText(filepath, System.Text.Encoding.UTF8);
        }
        catch { return null; }
    }
}
