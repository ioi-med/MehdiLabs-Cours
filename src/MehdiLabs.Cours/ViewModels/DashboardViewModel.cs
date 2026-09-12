using System.Collections.ObjectModel;
using System.IO;
using MehdiLabs.Cours.Services;
using MehdiLabs.Cours.ViewModels.Base;

namespace MehdiLabs.Cours.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private int _totalCourses;
    private int _totalSubjects;
    private int _totalWords;
    private string _sessionDuration = "0h 0m";
    private readonly FileManagerService _fileService;
    private readonly DateTime _startTime;

    public int TotalCourses
    {
        get => _totalCourses;
        set => SetProperty(ref _totalCourses, value);
    }

    public int TotalSubjects
    {
        get => _totalSubjects;
        set => SetProperty(ref _totalSubjects, value);
    }

    public int TotalWords
    {
        get => _totalWords;
        set => SetProperty(ref _totalWords, value);
    }

    public string SessionDuration
    {
        get => _sessionDuration;
        set => SetProperty(ref _sessionDuration, value);
    }

    public ObservableCollection<string> RecentFiles { get; } = new();
    public ObservableCollection<TodoItem> Todos { get; } = new();

    public DashboardViewModel()
    {
        _fileService = new FileManagerService(App.CoursDir);
        _startTime = DateTime.Now;

        // Mise à jour de la durée de session (timer asynchrone basique)
        _ = UpdateSessionDurationAsync();

        Refresh();
    }

    public void Refresh()
    {
        // Stats
        var (dirs, files) = _fileService.GetFileCount();
        TotalCourses = files;
        TotalSubjects = dirs;

        // Extraction des mots et TODOs (basique)
        int words = 0;
        Todos.Clear();

        try
        {
            foreach (var file in Directory.EnumerateFiles(App.CoursDir, "*.md", SearchOption.AllDirectories))
            {
                if (file.Contains(".trash") || file.Contains(".chat_history")) continue;

                var text = File.ReadAllText(file);
                words += text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;

                // Extraire TODOs
                foreach (var line in text.Split('\n'))
                {
                    var t = line.Trim();
                    if (t.StartsWith("- [ ]") || t.StartsWith("* [ ]"))
                    {
                        Todos.Add(new TodoItem
                        {
                            Text = t.Substring(5).Trim(),
                            FilePath = file,
                            FileName = Path.GetFileName(file)
                        });
                    }
                }
            }
        }
        catch { }

        TotalWords = words;

        // Récents
        RecentFiles.Clear();
        foreach (var rf in App.Settings.GetRecentFiles())
        {
            if (File.Exists(rf))
                RecentFiles.Add(Path.GetFileName(rf));
        }
    }

    private async Task UpdateSessionDurationAsync()
    {
        while (true)
        {
            var span = DateTime.Now - _startTime;
            SessionDuration = $"{(int)span.TotalHours}h {span.Minutes}m";
            await Task.Delay(60000); // 1 minute
        }
    }
}

public class TodoItem
{
    public string Text { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string FileName { get; set; } = "";
}
