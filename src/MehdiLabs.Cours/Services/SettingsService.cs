using System.IO;
using System.Text;
using System.Text.Json;
using MehdiLabs.Cours.Models;

namespace MehdiLabs.Cours.Services;

/// <summary>
/// Service de gestion des paramètres (config.json + api_keys.txt).
/// Équivalent de settings.py dans la V1.2.
/// </summary>
public class SettingsService
{
    private readonly string _appDir;
    private readonly string _configPath;
    private AppSettings _settings;

    public SettingsService(string appDir)
    {
        _appDir = appDir;
        _configPath = Path.Combine(appDir, "config.json");
        _settings = new AppSettings();
        Load();
    }

    /// <summary>
    /// Charge les paramètres depuis config.json et api_keys.txt.
    /// </summary>
    public void Load()
    {
        // Charger config.json
        if (File.Exists(_configPath))
        {
            try
            {
                var json = File.ReadAllText(_configPath, Encoding.UTF8);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (loaded != null)
                {
                    _settings = loaded;

                    // Décoder les clés API (base64)
                    var decodedKeys = new Dictionary<string, string>();
                    foreach (var kvp in _settings.ApiKeys)
                    {
                        if (!string.IsNullOrWhiteSpace(kvp.Value))
                        {
                            try
                            {
                                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(kvp.Value));
                                decodedKeys[kvp.Key] = decoded;
                            }
                            catch
                            {
                                decodedKeys[kvp.Key] = kvp.Value;
                            }
                        }
                        else
                        {
                            decodedKeys[kvp.Key] = "";
                        }
                    }
                    _settings.ApiKeys = decodedKeys;
                }
            }
            catch (Exception)
            {
                _settings = new AppSettings();
            }
        }
    }

    /// <summary>
    /// Sauvegarde les paramètres dans config.json.
    /// </summary>
    public void Save()
    {
        try
        {
            // Créer une copie pour encoder les clés API en base64
            var toSave = new AppSettings
            {
                DefaultProvider = _settings.DefaultProvider,
                Theme = _settings.Theme,
                Language = _settings.Language,
                AutoSaveInterval = _settings.AutoSaveInterval,
                EditorFontSize = _settings.EditorFontSize,
                Models = new Dictionary<string, string>(_settings.Models),
                RecentFiles = new List<string>(_settings.RecentFiles),
                LastOpenedFile = _settings.LastOpenedFile,
                ApiKeys = new Dictionary<string, string>()
            };

            foreach (var kvp in _settings.ApiKeys)
            {
                toSave.ApiKeys[kvp.Key] = string.IsNullOrWhiteSpace(kvp.Value)
                    ? ""
                    : Convert.ToBase64String(Encoding.UTF8.GetBytes(kvp.Value));
            }

            var json = JsonSerializer.Serialize(toSave, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            Directory.CreateDirectory(_appDir);
            File.WriteAllText(_configPath, json, Encoding.UTF8);
        }
        catch { }
    }

    // --- Accesseurs ---

    public string GetApiKey(string provider)
    {
        return _settings.ApiKeys.GetValueOrDefault(provider, "");
    }

    public void SetApiKey(string provider, string key)
    {
        _settings.ApiKeys[provider] = key;
    }

    public string GetDefaultProvider() => _settings.DefaultProvider;
    public void SetDefaultProvider(string provider) => _settings.DefaultProvider = provider;

    public string GetTheme() => _settings.Theme;
    public void SetTheme(string theme) => _settings.Theme = theme;

    public string GetModel(string provider)
        => _settings.Models.GetValueOrDefault(provider, "");

    public void SetModel(string provider, string model)
        => _settings.Models[provider] = model;

    public int GetEditorFontSize() => _settings.EditorFontSize;
    public void SetEditorFontSize(int size) => _settings.EditorFontSize = size;

    public int GetAutoSaveInterval() => _settings.AutoSaveInterval;
    public void SetAutoSaveInterval(int interval) => _settings.AutoSaveInterval = interval;

    public List<string> GetConfiguredProviders()
        => _settings.ApiKeys.Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                            .Select(kv => kv.Key).ToList();

    public List<string> GetRecentFiles() => _settings.RecentFiles.Take(5).ToList();

    public void AddRecentFile(string filepath)
    {
        _settings.RecentFiles.Remove(filepath);
        _settings.RecentFiles.Insert(0, filepath);
        if (_settings.RecentFiles.Count > 5)
            _settings.RecentFiles = _settings.RecentFiles.Take(5).ToList();
        Save();
    }

    public string GetLastOpenedFile() => _settings.LastOpenedFile;
    public void SetLastOpenedFile(string filepath)
    {
        _settings.LastOpenedFile = filepath;
        Save();
    }

    public string AppDir => _appDir;
}
