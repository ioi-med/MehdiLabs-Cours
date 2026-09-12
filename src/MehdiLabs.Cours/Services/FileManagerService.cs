using System.IO;
using MehdiLabs.Cours.Models;

namespace MehdiLabs.Cours.Services;

/// <summary>
/// Service de gestion de fichiers — CRUD, corbeille, templates.
/// Équivalent de file_manager.py dans la V1.2.
/// </summary>
public class FileManagerService
{
    private readonly string _coursDir;
    private readonly string _trashDir;

    /// <summary>
    /// Templates de cours disponibles.
    /// </summary>
    public static readonly Dictionary<string, string> Templates = new()
    {
        ["Vide"] = "# {title}\n\n",
        ["Cours structuré"] = "# {title}\n\n## 1. Introduction\n\n## 2. Concepts clés\n- \n- \n\n## 3. Conclusion\n\n",
        ["Fiche de révision"] = "# Fiche : {title}\n\n### Définitions\n- \n\n### Formules / Points clés\n- \n\n### À retenir\n- \n",
        ["TD / Exercices"] = "# TD : {title}\n\n## Exercice 1\n**Énoncé :**\n\n**Brouillon :**\n\n**Correction :**\n",
    };

    public FileManagerService(string coursDir)
    {
        _coursDir = coursDir;
        _trashDir = Path.Combine(coursDir, ".trash");
        Directory.CreateDirectory(_coursDir);
        Directory.CreateDirectory(_trashDir);
    }

    /// <summary>
    /// Construit l'arborescence complète du dossier cours.
    /// </summary>
    public List<CourseFile> GetFileTree(string? sortBy = "Nom", string? searchFilter = null)
    {
        return GetDirectoryContents(_coursDir, sortBy, searchFilter);
    }

    private List<CourseFile> GetDirectoryContents(string path, string? sortBy, string? searchFilter)
    {
        var items = new List<CourseFile>();

        try
        {
            var entries = new List<FileSystemInfo>();
            entries.AddRange(new DirectoryInfo(path).GetDirectories());
            entries.AddRange(new DirectoryInfo(path).GetFiles());

            // Filtrer les fichiers cachés
            entries = entries.Where(e => !e.Name.StartsWith('.')).ToList();

            // Trier
            entries = sortBy switch
            {
                "Date" => entries.OrderBy(e => e is DirectoryInfo ? 0 : 1)
                                 .ThenByDescending(e => e.LastWriteTime).ToList(),
                "Taille" => entries.OrderBy(e => e is DirectoryInfo ? 0 : 1)
                                   .ThenByDescending(e => e is FileInfo fi ? fi.Length : 0).ToList(),
                _ => entries.OrderBy(e => e is DirectoryInfo ? 0 : 1)
                            .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase).ToList()
            };

            foreach (var entry in entries)
            {
                if (entry is DirectoryInfo dir)
                {
                    var children = GetDirectoryContents(dir.FullName, sortBy, searchFilter);
                    // Si recherche active et pas de résultats enfants et le nom ne match pas, skip
                    if (!string.IsNullOrEmpty(searchFilter)
                        && children.Count == 0
                        && !dir.Name.Contains(searchFilter, StringComparison.OrdinalIgnoreCase))
                        continue;

                    items.Add(new CourseFile
                    {
                        Name = dir.Name,
                        FullPath = dir.FullName,
                        IsDirectory = true,
                        LastModified = dir.LastWriteTime,
                        Children = children
                    });
                }
                else if (entry is FileInfo file)
                {
                    // Filtrer par recherche
                    if (!string.IsNullOrEmpty(searchFilter)
                        && !file.Name.Contains(searchFilter, StringComparison.OrdinalIgnoreCase))
                        continue;

                    items.Add(new CourseFile
                    {
                        Name = file.Name,
                        FullPath = file.FullName,
                        IsDirectory = false,
                        Size = file.Length,
                        LastModified = file.LastWriteTime
                    });
                }
            }
        }
        catch { }

        return items;
    }

    /// <summary>
    /// Crée un nouveau dossier.
    /// </summary>
    public bool CreateFolder(string parentDir, string name)
    {
        try
        {
            var path = Path.Combine(parentDir, name.Trim());
            Directory.CreateDirectory(path);
            return true;
        }
        catch { return false; }
    }

    /// <summary>
    /// Crée un nouveau fichier à partir d'un template.
    /// </summary>
    public string? CreateFile(string parentDir, string name, string templateName = "Vide")
    {
        try
        {
            if (!name.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                name += ".md";

            var path = Path.Combine(parentDir, name.Trim());
            var template = Templates.GetValueOrDefault(templateName, Templates["Vide"]);
            var title = Path.GetFileNameWithoutExtension(name).Replace("_", " ");
            var content = template.Replace("{title}", title);

            File.WriteAllText(path, content, System.Text.Encoding.UTF8);
            return path;
        }
        catch { return null; }
    }

    /// <summary>
    /// Renomme un fichier ou dossier.
    /// </summary>
    public bool Rename(string oldPath, string newName)
    {
        try
        {
            var dir = Path.GetDirectoryName(oldPath)!;
            var newPath = Path.Combine(dir, newName.Trim());
            if (Directory.Exists(oldPath))
                Directory.Move(oldPath, newPath);
            else
                File.Move(oldPath, newPath);
            return true;
        }
        catch { return false; }
    }

    /// <summary>
    /// Duplique un fichier.
    /// </summary>
    public string? Duplicate(string filePath)
    {
        try
        {
            if (Directory.Exists(filePath)) return null;

            var dir = Path.GetDirectoryName(filePath)!;
            var name = Path.GetFileNameWithoutExtension(filePath);
            var ext = Path.GetExtension(filePath);
            var newPath = Path.Combine(dir, $"{name} (copie){ext}");

            int counter = 1;
            while (File.Exists(newPath))
            {
                newPath = Path.Combine(dir, $"{name} (copie {counter}){ext}");
                counter++;
            }

            File.Copy(filePath, newPath);
            return newPath;
        }
        catch { return null; }
    }

    /// <summary>
    /// Déplace un fichier/dossier vers la corbeille (.trash).
    /// </summary>
    public bool MoveToTrash(string path)
    {
        try
        {
            var name = Path.GetFileName(path);
            var dest = Path.Combine(_trashDir, name);

            if (File.Exists(dest) || Directory.Exists(dest))
            {
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                dest = Path.Combine(_trashDir, $"{timestamp}_{name}");
            }

            if (Directory.Exists(path))
                Directory.Move(path, dest);
            else
                File.Move(path, dest);

            return true;
        }
        catch { return false; }
    }

    /// <summary>
    /// Compte les dossiers et fichiers dans le répertoire cours.
    /// </summary>
    public (int dirs, int files) GetFileCount()
    {
        int totalDirs = 0, totalFiles = 0;
        try
        {
            foreach (var dir in Directory.EnumerateDirectories(_coursDir, "*", SearchOption.AllDirectories))
            {
                if (dir.Contains(".chat_history") || dir.Contains(".trash") || dir.Contains(".backups"))
                    continue;
                totalDirs++;
            }
            foreach (var file in Directory.EnumerateFiles(_coursDir, "*", SearchOption.AllDirectories))
            {
                if (file.Contains(".chat_history") || file.Contains(".trash") || file.Contains(".backups"))
                    continue;
                totalFiles++;
            }
        }
        catch { }
        return (totalDirs, totalFiles);
    }

    /// <summary>
    /// Retourne le répertoire parent ou le dossier cours si rien n'est sélectionné.
    /// </summary>
    public string GetParentDir(string? selectedPath)
    {
        if (string.IsNullOrEmpty(selectedPath)) return _coursDir;
        if (Directory.Exists(selectedPath)) return selectedPath;
        return Path.GetDirectoryName(selectedPath) ?? _coursDir;
    }

    public string CoursDir => _coursDir;
}
