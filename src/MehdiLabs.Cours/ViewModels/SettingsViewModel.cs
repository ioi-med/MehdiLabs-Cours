using System.Collections.ObjectModel;
using System.Windows;
using MehdiLabs.Cours.Services.AI;
using MehdiLabs.Cours.ViewModels.Base;

namespace MehdiLabs.Cours.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    public ObservableCollection<string> Themes { get; } = new() { "dark", "light", "sepia", "hacker" };
    public ObservableCollection<string> AvailableProviders { get; } = new();
    
    private string _selectedTheme = "dark";
    public string SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (SetProperty(ref _selectedTheme, value))
            {
                App.Settings.SetTheme(value);
                App.Settings.Save();
                App.ApplyTheme(value);
            }
        }
    }

    private string _selectedProvider = "";
    public string SelectedProvider
    {
        get => _selectedProvider;
        set
        {
            if (SetProperty(ref _selectedProvider, value))
            {
                App.Settings.SetDefaultProvider(value);
                App.Settings.Save();
                LoadModelsForProvider(value);
            }
        }
    }

    private string _selectedModel = "";
    public string SelectedModel
    {
        get => _selectedModel;
        set
        {
            if (SetProperty(ref _selectedModel, value) && !string.IsNullOrEmpty(_selectedProvider))
            {
                App.Settings.SetModel(_selectedProvider, value);
                App.Settings.Save();
            }
        }
    }

    public ObservableCollection<string> AvailableModels { get; } = new();

    // API Keys
    public string MistralKey { get => App.Settings.GetApiKey("Mistral"); set => SetKey("Mistral", value); }
    public string GeminiKey { get => App.Settings.GetApiKey("Gemini"); set => SetKey("Gemini", value); }
    public string DeepSeekKey { get => App.Settings.GetApiKey("DeepSeek"); set => SetKey("DeepSeek", value); }
    public string GPTKey { get => App.Settings.GetApiKey("GPT"); set => SetKey("GPT", value); }
    public string ClaudeKey { get => App.Settings.GetApiKey("Claude"); set => SetKey("Claude", value); }
    public string GroqKey { get => App.Settings.GetApiKey("Groq"); set => SetKey("Groq", value); }

    public RelayCommand SaveKeysCommand { get; }

    public SettingsViewModel()
    {
        SaveKeysCommand = new RelayCommand(SaveSettings);
        
        SelectedTheme = App.Settings.GetTheme();
        
        foreach (var p in AIProviderFactory.ProviderNames)
            AvailableProviders.Add(p);

        SelectedProvider = App.Settings.GetDefaultProvider();
        if (string.IsNullOrEmpty(SelectedProvider) && AvailableProviders.Any())
            SelectedProvider = AvailableProviders.First();
            
        LoadModelsForProvider(SelectedProvider);
    }

    private void SetKey(string provider, string key)
    {
        App.Settings.SetApiKey(provider, key);
        OnPropertyChanged($"{provider}Key");
    }

    private void LoadModelsForProvider(string provider)
    {
        AvailableModels.Clear();
        if (string.IsNullOrEmpty(provider)) return;

        foreach (var m in AIProviderFactory.GetModels(provider))
            AvailableModels.Add(m);

        var savedModel = App.Settings.GetModel(provider);
        if (!string.IsNullOrEmpty(savedModel) && AvailableModels.Contains(savedModel))
            SelectedModel = savedModel;
        else if (AvailableModels.Any())
            SelectedModel = AvailableModels.First();
    }

    private void SaveSettings()
    {
        App.Settings.Save();
        MessageBox.Show("Paramètres sauvegardés avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
