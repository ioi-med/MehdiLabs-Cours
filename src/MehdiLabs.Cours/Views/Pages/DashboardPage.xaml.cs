using System.Windows.Controls;
using MehdiLabs.Cours.ViewModels;

namespace MehdiLabs.Cours.Views.Pages;

public partial class DashboardPage : Page
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
