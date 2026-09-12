using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MehdiLabs.Cours.ViewModels.Base;

/// <summary>
/// Classe de base pour tous les ViewModels (pattern MVVM).
/// Implémente INotifyPropertyChanged pour le data binding WPF.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Notifie le binding WPF qu'une propriété a changé.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Met à jour un champ et notifie si la valeur a changé.
    /// </summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
