using System.Windows.Threading;
using MehdiLabs.Cours.ViewModels.Base;

namespace MehdiLabs.Cours.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase _currentViewModel;
    private string _pomodoroDisplay = "25:00";
    private bool _isPomodoroRunning;
    private int _pomodoroSeconds = 25 * 60;
    private readonly DispatcherTimer _timer;

    // ViewModels pour chaque page
    public DashboardViewModel DashboardVM { get; }
    public FileManagerViewModel FilesVM { get; }
    public EditorViewModel EditorVM { get; }
    public ChatbotViewModel ChatVM { get; }
    public SettingsViewModel SettingsVM { get; }

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public string PomodoroDisplay
    {
        get => _pomodoroDisplay;
        set => SetProperty(ref _pomodoroDisplay, value);
    }

    public bool IsPomodoroRunning
    {
        get => _isPomodoroRunning;
        set => SetProperty(ref _isPomodoroRunning, value);
    }

    public string PomodoroIcon => IsPomodoroRunning ? "⏸" : "▶";

    // Commandes de navigation
    public RelayCommand NavigateDashboardCommand { get; }
    public RelayCommand NavigateFilesCommand { get; }
    public RelayCommand NavigateChatCommand { get; }
    public RelayCommand NavigateSettingsCommand { get; }
    public RelayCommand TogglePomodoroCommand { get; }

    public MainViewModel()
    {
        // Instanciation des sous-ViewModels
        DashboardVM = new DashboardViewModel();
        FilesVM = new FileManagerViewModel();
        EditorVM = new EditorViewModel();
        ChatVM = new ChatbotViewModel();
        SettingsVM = new SettingsViewModel();

        // Par défaut, afficher le Dashboard
        _currentViewModel = DashboardVM;

        // Commandes
        NavigateDashboardCommand = new RelayCommand(() => CurrentViewModel = DashboardVM);
        NavigateFilesCommand = new RelayCommand(() => CurrentViewModel = FilesVM);
        NavigateChatCommand = new RelayCommand(() => CurrentViewModel = ChatVM);
        NavigateSettingsCommand = new RelayCommand(() => CurrentViewModel = SettingsVM);

        TogglePomodoroCommand = new RelayCommand(TogglePomodoro);

        // Timer Pomodoro
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;
    }

    private void TogglePomodoro()
    {
        if (IsPomodoroRunning)
        {
            _timer.Stop();
        }
        else
        {
            if (_pomodoroSeconds == 0)
                _pomodoroSeconds = 25 * 60;
            _timer.Start();
        }
        IsPomodoroRunning = !IsPomodoroRunning;
        OnPropertyChanged(nameof(PomodoroIcon));
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_pomodoroSeconds > 0)
        {
            _pomodoroSeconds--;
            int m = _pomodoroSeconds / 60;
            int s = _pomodoroSeconds % 60;
            PomodoroDisplay = $"{m:00}:{s:00}";
        }
        else
        {
            _timer.Stop();
            IsPomodoroRunning = false;
            OnPropertyChanged(nameof(PomodoroIcon));
            PomodoroDisplay = "00:00";
            // TODO: Jouer un son (Console.Beep)
        }
    }
}
