using System.IO;
using System.Windows;
using MehdiLabs.Cours.Services;

namespace MehdiLabs.Cours;

/// <summary>
/// Point d'entrée principal de l'application MehdiLabs Cours V2.0.
/// </summary>
public partial class App : Application
{
    public static SettingsService Settings { get; private set; } = null!;
    public static string AppDir { get; private set; } = null!;
    public static string CoursDir { get; private set; } = null!;
    public static string DataDir { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Déterminer les répertoires de l'application (fix for single-file .exe)
        var processPath = Environment.ProcessPath;
        AppDir = !string.IsNullOrEmpty(processPath) 
            ? Path.GetDirectoryName(processPath) ?? AppDomain.CurrentDomain.BaseDirectory 
            : AppDomain.CurrentDomain.BaseDirectory;
            
        DataDir = AppDir;
        CoursDir = Path.Combine(DataDir, "cours");
        Directory.CreateDirectory(CoursDir);

        // Charger les paramètres
        Settings = new SettingsService(DataDir);

        // Appliquer le thème initial
        ApplyTheme(Settings.GetTheme());
    }

    /// <summary>
    /// Applique un thème en remplaçant le ResourceDictionary de thème.
    /// </summary>
    public static void ApplyTheme(string themeName)
    {
        var app = (App)Current;
        var mergedDicts = app.Resources.MergedDictionaries;

        // Retirer l'ancien thème (le premier dictionnaire est toujours le thème)
        if (mergedDicts.Count > 0)
        {
            // Trouver et retirer le dictionnaire de thème existant
            ResourceDictionary? themeDict = null;
            foreach (var dict in mergedDicts)
            {
                if (dict.Source != null && dict.Source.OriginalString.Contains("Theme.xaml"))
                {
                    themeDict = dict;
                    break;
                }
            }
            if (themeDict != null)
                mergedDicts.Remove(themeDict);
        }

        // Charger le nouveau thème
        string themeFile = themeName switch
        {
            "light" => "Themes/LightTheme.xaml",
            "sepia" => "Themes/SepiaTheme.xaml",
            "hacker" => "Themes/HackerTheme.xaml",
            _ => "Themes/DarkTheme.xaml"
        };

        var newTheme = new ResourceDictionary
        {
            Source = new Uri(themeFile, UriKind.Relative)
        };

        // Insérer le thème au début (avant SharedStyles)
        mergedDicts.Insert(0, newTheme);
    }
}
