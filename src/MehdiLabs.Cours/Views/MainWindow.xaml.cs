using System.Windows;
using MehdiLabs.Cours.ViewModels;
using MehdiLabs.Cours.Views.Pages;

namespace MehdiLabs.Cours.Views;

public partial class MainWindow : Window
{
    private MainViewModel _viewModel;
    
    // Pages instanciées une seule fois (pour conserver l'état, ex: WebView2)
    private DashboardPage _dashboardPage;
    private FilesPage _filesPage;
    private ChatPage _chatPage;
    private SettingsPage _settingsPage;

    public MainWindow()
    {
        InitializeComponent();
        
        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        // Instancier les pages avec leurs ViewModels respectifs
        _dashboardPage = new DashboardPage(_viewModel.DashboardVM);
        _filesPage = new FilesPage(_viewModel.FilesVM, _viewModel.EditorVM);
        _chatPage = new ChatPage(_viewModel.ChatVM);
        _settingsPage = new SettingsPage(_viewModel.SettingsVM);

        // S'abonner aux changements de ViewModel pour naviguer
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        
        // Navigation initiale
        NavigateToCurrentViewModel();
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CurrentViewModel))
        {
            NavigateToCurrentViewModel();
        }
    }

    private void NavigateToCurrentViewModel()
    {
        var vm = _viewModel.CurrentViewModel;
        
        if (vm is DashboardViewModel)
        {
            _viewModel.DashboardVM.Refresh();
            MainFrame.Navigate(_dashboardPage);
        }
        else if (vm is FileManagerViewModel)
            MainFrame.Navigate(_filesPage);
        else if (vm is ChatbotViewModel)
        {
            _viewModel.ChatVM.UpdateProvider();
            MainFrame.Navigate(_chatPage);
        }
        else if (vm is SettingsViewModel)
            MainFrame.Navigate(_settingsPage);
    }
}
