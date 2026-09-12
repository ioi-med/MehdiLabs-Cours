using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MehdiLabs.Cours.Models;
using MehdiLabs.Cours.Services;
using MehdiLabs.Cours.ViewModels;

namespace MehdiLabs.Cours.Views.Pages;

public partial class FilesPage : Page
{
    private readonly FileManagerViewModel _fmVM;
    private readonly EditorViewModel _editorVM;
    private readonly FileManagerService _fileService;

    private string _currentFilePath = "";
    private bool _isModified = false;
    private bool _suppressTextChanged = false;

    public FilesPage(FileManagerViewModel fmVM, EditorViewModel editorVM)
    {
        InitializeComponent();
        _fmVM = fmVM;
        _editorVM = editorVM;
        _fileService = new FileManagerService(App.CoursDir);
        DataContext = _fmVM;

        InitWebView();
    }

    private async void InitWebView()
    {
        try
        {
            await PreviewBrowser.EnsureCoreWebView2Async(null);
        }
        catch { }
    }

    // === CRUD Fichiers ===

    private void CreateFile_Click(object sender, RoutedEventArgs e)
    {
        string parent = _fmVM.SelectedFile?.IsDirectory == true
            ? _fmVM.SelectedFile.FullPath
            : App.CoursDir;

        // Demander le nom
        var name = PromptForName("Nouveau cours", "Entrez le nom du cours :");
        if (string.IsNullOrWhiteSpace(name)) return;

        var path = _fileService.CreateFile(parent, name);
        if (path != null)
        {
            _fmVM.LoadTree();
            LoadFileInEditor(path);
        }
    }

    private void CreateFolder_Click(object sender, RoutedEventArgs e)
    {
        string parent = _fmVM.SelectedFile?.IsDirectory == true
            ? _fmVM.SelectedFile.FullPath
            : App.CoursDir;

        var name = PromptForName("Nouveau dossier", "Entrez le nom du dossier :");
        if (string.IsNullOrWhiteSpace(name)) return;

        if (_fileService.CreateFolder(parent, name))
            _fmVM.LoadTree();
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (_fmVM.SelectedFile == null)
        {
            MessageBox.Show("Sélectionnez d'abord un élément à supprimer.", "Info", MessageBoxButton.OK);
            return;
        }

        var res = MessageBox.Show(
            $"Mettre '{_fmVM.SelectedFile.Name}' à la corbeille ?",
            "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (res == MessageBoxResult.Yes)
        {
            // Si on supprime le fichier en cours d'édition, fermer l'éditeur
            if (_fmVM.SelectedFile.FullPath == _currentFilePath)
            {
                _currentFilePath = "";
                _suppressTextChanged = true;
                EditorTextBox.Text = "";
                _suppressTextChanged = false;
                FileNameText.Text = "Aucun fichier";
                ModifiedIndicator.Visibility = Visibility.Collapsed;
                UpdatePreview("");
            }

            _fileService.MoveToTrash(_fmVM.SelectedFile.FullPath);
            _fmVM.SelectedFile = null;
            _fmVM.LoadTree();
        }
    }

    // === Éditeur ===

    private void FileTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is CourseFile file)
        {
            _fmVM.SelectedFile = file;

            // Si c'est un fichier .md, l'ouvrir dans l'éditeur
            if (!file.IsDirectory && file.FullPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                LoadFileInEditor(file.FullPath);
                App.Settings.AddRecentFile(file.FullPath);
            }
        }
    }

    private void LoadFileInEditor(string path)
    {
        // Sauvegarder le fichier actuel si modifié
        if (_isModified && !string.IsNullOrEmpty(_currentFilePath))
            SaveCurrentFile();

        try
        {
            _currentFilePath = path;
            var text = File.ReadAllText(path, Encoding.UTF8);

            _suppressTextChanged = true;
            EditorTextBox.Text = text;
            _suppressTextChanged = false;

            FileNameText.Text = Path.GetFileName(path);
            _isModified = false;
            ModifiedIndicator.Visibility = Visibility.Collapsed;

            UpdatePreview(text);
            UpdateWordCount(text);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'ouverture du fichier :\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        SaveCurrentFile();
    }

    private void SaveCurrentFile()
    {
        if (string.IsNullOrEmpty(_currentFilePath)) return;

        try
        {
            File.WriteAllText(_currentFilePath, EditorTextBox.Text, Encoding.UTF8);
            _isModified = false;
            ModifiedIndicator.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la sauvegarde :\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void EditorTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressTextChanged) return;

        _isModified = true;
        ModifiedIndicator.Visibility = Visibility.Visible;

        var text = EditorTextBox.Text;
        UpdatePreview(text);
        UpdateWordCount(text);
    }

    // === Preview ===

    private void UpdatePreview(string markdown)
    {
        if (PreviewBrowser.CoreWebView2 != null)
        {
            var html = MarkdownService.ToHtml(markdown, App.Settings.GetTheme());
            PreviewBrowser.NavigateToString(html);
        }
    }

    private void UpdateWordCount(string text)
    {
        int count = string.IsNullOrWhiteSpace(text)
            ? 0
            : text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        WordCountText.Text = $"{count} mots";
    }

    // === Utilitaires ===

    private string? PromptForName(string title, string message)
    {
        // InputBox WPF simple via une Window customisée
        var dlg = new Window
        {
            Title = title,
            Width = 400,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Application.Current.MainWindow,
            ResizeMode = ResizeMode.NoResize,
            Background = (System.Windows.Media.Brush)Application.Current.Resources["BgMainBrush"]
        };

        var sp = new StackPanel { Margin = new Thickness(20) };

        var label = new TextBlock
        {
            Text = message,
            Foreground = (System.Windows.Media.Brush)Application.Current.Resources["FgPrimaryBrush"],
            FontSize = 14,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var textBox = new TextBox
        {
            FontSize = 14,
            Padding = new Thickness(8, 6, 8, 6),
            Margin = new Thickness(0, 0, 0, 15)
        };

        var okBtn = new Button
        {
            Content = "Créer",
            Padding = new Thickness(20, 8, 20, 8),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        okBtn.Click += (s, e) => { dlg.DialogResult = true; dlg.Close(); };

        sp.Children.Add(label);
        sp.Children.Add(textBox);
        sp.Children.Add(okBtn);
        dlg.Content = sp;

        textBox.Focus();

        if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(textBox.Text))
            return textBox.Text.Trim();

        return null;
    }
}
