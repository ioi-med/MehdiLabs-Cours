using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using MehdiLabs.Cours.Models;
using MehdiLabs.Cours.Services;
using MehdiLabs.Cours.ViewModels.Base;

namespace MehdiLabs.Cours.ViewModels;

public class FileManagerViewModel : ViewModelBase
{
    private readonly FileManagerService _service;
    private string _searchQuery = "";
    private CourseFile? _selectedFile;

    public ObservableCollection<CourseFile> Files { get; } = new();

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
                LoadTree();
        }
    }

    public CourseFile? SelectedFile
    {
        get => _selectedFile;
        set
        {
            if (SetProperty(ref _selectedFile, value))
            {
                // Signaler à l'éditeur qu'un fichier a été sélectionné
                if (value != null && !value.IsDirectory && value.FullPath.EndsWith(".md"))
                {
                    FileSelected?.Invoke(this, value.FullPath);
                    App.Settings.AddRecentFile(value.FullPath);
                }
            }
        }
    }

    public event EventHandler<string>? FileSelected;

    public RelayCommand CreateFileCommand { get; }
    public RelayCommand CreateFolderCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public FileManagerViewModel()
    {
        _service = new FileManagerService(App.CoursDir);

        CreateFileCommand = new RelayCommand(CreateFile);
        CreateFolderCommand = new RelayCommand(CreateFolder);
        DeleteCommand = new RelayCommand(DeleteFile, () => SelectedFile != null);
        RefreshCommand = new RelayCommand(LoadTree);

        LoadTree();
    }

    public void LoadTree()
    {
        Files.Clear();
        var tree = _service.GetFileTree("Nom", SearchQuery);
        foreach (var item in tree)
            Files.Add(item);
    }

    private void CreateFile()
    {
        // Idéalement on ouvrirait une InputBox WPF, mais pour simplifier ici :
        string parent = SelectedFile?.IsDirectory == true ? SelectedFile.FullPath : _service.CoursDir;
        string name = $"Nouveau cours {DateTime.Now.Ticks}.md"; // Nom auto
        var path = _service.CreateFile(parent, name);
        if (path != null)
        {
            LoadTree();
            FileSelected?.Invoke(this, path);
        }
    }

    private void CreateFolder()
    {
        string parent = SelectedFile?.IsDirectory == true ? SelectedFile.FullPath : _service.CoursDir;
        string name = $"Nouveau dossier {DateTime.Now.Ticks}";
        if (_service.CreateFolder(parent, name))
            LoadTree();
    }

    private void DeleteFile()
    {
        if (SelectedFile != null)
        {
            var res = MessageBox.Show($"Mettre '{SelectedFile.Name}' à la corbeille ?", "Confirmation", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                _service.MoveToTrash(SelectedFile.FullPath);
                SelectedFile = null;
                LoadTree();
            }
        }
    }
}
