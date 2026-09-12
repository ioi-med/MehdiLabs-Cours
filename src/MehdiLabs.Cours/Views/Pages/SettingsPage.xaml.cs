using System.Windows.Controls;
using MehdiLabs.Cours.ViewModels;

namespace MehdiLabs.Cours.Views.Pages;

public partial class SettingsPage : Page
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
