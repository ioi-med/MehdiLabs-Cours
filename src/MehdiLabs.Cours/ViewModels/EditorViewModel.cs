using System.IO;
using System.Text;
using System.Windows.Threading;
using MehdiLabs.Cours.Services;
using MehdiLabs.Cours.ViewModels.Base;

namespace MehdiLabs.Cours.ViewModels;

public class EditorViewModel : ViewModelBase
{
    private string _currentFilePath = "";
    private string _markdownContent = "";
    private string _htmlContent = "";
    private bool _isModified;
    private int _wordCount;
    private readonly DispatcherTimer _autoSaveTimer;

    public string CurrentFilePath
    {
        get => _currentFilePath;
        private set
        {
            if (SetProperty(ref _currentFilePath, value))
                OnPropertyChanged(nameof(FileName));
        }
    }

    public string FileName => string.IsNullOrEmpty(CurrentFilePath) ? "Aucun fichier" : Path.GetFileName(CurrentFilePath);

    public string MarkdownContent
    {
        get => _markdownContent;
        set
        {
            if (SetProperty(ref _markdownContent, value))
            {
                IsModified = true;
                UpdatePreviewAndStats();
            }
        }
    }

    public string HtmlContent
    {
        get => _htmlContent;
        private set => SetProperty(ref _htmlContent, value);
    }

    public bool IsModified
    {
        get => _isModified;
        set => SetProperty(ref _isModified, value);
    }

    public int WordCount
    {
        get => _wordCount;
        set => SetProperty(ref _wordCount, value);
    }

    public RelayCommand SaveCommand { get; }
    public RelayCommand CloseCommand { get; }

    public EditorViewModel()
    {
        SaveCommand = new RelayCommand(Save, () => IsModified && !string.IsNullOrEmpty(CurrentFilePath));
        CloseCommand = new RelayCommand(Close);

        _autoSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(App.Settings.GetAutoSaveInterval()) };
        _autoSaveTimer.Tick += (s, e) => { if (IsModified) Save(); };
        _autoSaveTimer.Start();
    }

    public void LoadFile(string path)
    {
        if (IsModified) Save();

        try
        {
            CurrentFilePath = path;
            var text = File.ReadAllText(path, Encoding.UTF8);
            
            // Bypass setter to avoid triggering IsModified = true
            _markdownContent = text;
            OnPropertyChanged(nameof(MarkdownContent));
            
            IsModified = false;
            UpdatePreviewAndStats();
        }
        catch { }
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(CurrentFilePath)) return;

        try
        {
            File.WriteAllText(CurrentFilePath, MarkdownContent, Encoding.UTF8);
            IsModified = false;
        }
        catch { }
    }

    private void Close()
    {
        if (IsModified) Save();
        CurrentFilePath = "";
        _markdownContent = "";
        _htmlContent = "";
        OnPropertyChanged(nameof(MarkdownContent));
        OnPropertyChanged(nameof(HtmlContent));
    }

    private void UpdatePreviewAndStats()
    {
        // HTML Preview
        HtmlContent = MarkdownService.ToHtml(MarkdownContent, App.Settings.GetTheme());

        // Stats
        WordCount = string.IsNullOrWhiteSpace(MarkdownContent)
            ? 0
            : MarkdownContent.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            
        RelayCommand.RaiseCanExecuteChanged();
    }
}
