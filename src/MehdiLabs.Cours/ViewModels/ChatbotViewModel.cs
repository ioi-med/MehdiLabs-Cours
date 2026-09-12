using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using MehdiLabs.Cours.Models;
using MehdiLabs.Cours.Services;
using MehdiLabs.Cours.Services.AI;
using MehdiLabs.Cours.ViewModels.Base;

namespace MehdiLabs.Cours.ViewModels;

public class ChatbotViewModel : ViewModelBase
{
    private readonly ConversationService _convService;
    private readonly CourseContextService _contextService;
    private IAIProvider? _provider;
    
    private Conversation? _currentConversation;
    private string _inputText = "";
    private bool _isGenerating;
    private string _selectedCourseForContext = "";

    public ObservableCollection<Conversation> Conversations { get; } = new();
    public ObservableCollection<ChatMessage> CurrentMessages { get; } = new();
    public ObservableCollection<string> AvailableCourses { get; } = new();

    public Conversation? CurrentConversation
    {
        get => _currentConversation;
        set
        {
            if (SetProperty(ref _currentConversation, value))
            {
                LoadMessages();
            }
        }
    }

    public string InputText
    {
        get => _inputText;
        set => SetProperty(ref _inputText, value);
    }

    public bool IsGenerating
    {
        get => _isGenerating;
        set => SetProperty(ref _isGenerating, value);
    }

    public string SelectedCourseForContext
    {
        get => _selectedCourseForContext;
        set => SetProperty(ref _selectedCourseForContext, value);
    }

    public RelayCommand SendCommand { get; }
    public RelayCommand NewChatCommand { get; }
    public RelayCommand<string> DeleteChatCommand { get; }
    public RelayCommand AttachCourseCommand { get; }

    public ChatbotViewModel()
    {
        _convService = new ConversationService(App.CoursDir);
        _contextService = new CourseContextService(App.CoursDir);

        SendCommand = new RelayCommand(async () => await SendAsync(), () => !IsGenerating && !string.IsNullOrWhiteSpace(InputText));
        NewChatCommand = new RelayCommand(CreateNewChat);
        DeleteChatCommand = new RelayCommand<string>(DeleteChat);
        AttachCourseCommand = new RelayCommand(AttachCourse);

        LoadHistory();
        RefreshCourses();
        UpdateProvider();
    }

    public void UpdateProvider()
    {
        var provName = App.Settings.GetDefaultProvider();
        if (string.IsNullOrEmpty(provName)) return;

        var key = App.Settings.GetApiKey(provName);
        if (!string.IsNullOrEmpty(key))
        {
            try
            {
                _provider = AIProviderFactory.Create(provName, key);
            }
            catch { }
        }
    }

    private void LoadHistory()
    {
        Conversations.Clear();
        foreach (var c in _convService.LoadAll())
            Conversations.Add(c);

        if (Conversations.Any())
            CurrentConversation = Conversations.First();
        else
            CreateNewChat();
    }

    private void RefreshCourses()
    {
        AvailableCourses.Clear();
        AvailableCourses.Add("Tous les cours");
        foreach (var c in _contextService.ListAvailableCourses())
            AvailableCourses.Add(c.RelativePath);
        SelectedCourseForContext = "Tous les cours";
    }

    private void LoadMessages()
    {
        CurrentMessages.Clear();
        if (CurrentConversation != null)
        {
            foreach (var m in CurrentConversation.Messages)
                CurrentMessages.Add(m);
        }
    }

    private void CreateNewChat()
    {
        CurrentConversation = _convService.Create();
        LoadHistory();
    }

    private void DeleteChat(string? id)
    {
        if (id == null) return;
        if (MessageBox.Show("Supprimer cette conversation ?", "Confirmer", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            _convService.Delete(id);
            LoadHistory();
        }
    }

    private void AttachCourse()
    {
        if (SelectedCourseForContext == "Tous les cours")
        {
            var ctx = _contextService.ScanCourses();
            if (!string.IsNullOrEmpty(ctx))
                InputText += $"\n[Contexte attaché : Tous les cours]\n{ctx}\n";
        }
        else
        {
            var files = _contextService.ListAvailableCourses();
            var match = files.FirstOrDefault(f => f.RelativePath == SelectedCourseForContext);
            if (match != default)
            {
                var content = _contextService.ReadCourseContent(match.FullPath);
                if (content != null)
                    InputText += $"\n[Contexte attaché : {match.RelativePath}]\n{content}\n";
            }
        }
    }

    private async Task SendAsync()
    {
        if (CurrentConversation == null || string.IsNullOrWhiteSpace(InputText) || _provider == null)
            return;

        var userText = InputText;
        InputText = "";
        IsGenerating = true;

        var userMsg = new ChatMessage { Role = "user", Content = userText };
        CurrentConversation.Messages.Add(userMsg);
        CurrentMessages.Add(userMsg);
        _convService.Save(CurrentConversation);

        try
        {
            var model = App.Settings.GetModel(_provider.Name);
            var reply = await _provider.SendMessageAsync(CurrentConversation.Messages, string.IsNullOrEmpty(model) ? null : model);

            var botMsg = new ChatMessage { Role = "assistant", Content = reply };
            CurrentConversation.Messages.Add(botMsg);
            CurrentMessages.Add(botMsg);
            _convService.Save(CurrentConversation);
        }
        catch (Exception ex)
        {
            var errorMsg = new ChatMessage { Role = "assistant", Content = $"❌ Erreur : {ex.Message}" };
            CurrentConversation.Messages.Add(errorMsg);
            CurrentMessages.Add(errorMsg);
        }
        finally
        {
            IsGenerating = false;
        }
    }
}
