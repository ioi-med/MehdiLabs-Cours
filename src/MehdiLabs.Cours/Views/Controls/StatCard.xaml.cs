using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MehdiLabs.Cours.Views.Controls;

public partial class StatCard : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(StatCard), 
            new PropertyMetadata("Titre", OnTitleChanged));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(string), typeof(StatCard), 
            new PropertyMetadata("0", OnValueChanged));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register("Icon", typeof(string), typeof(StatCard), 
            new PropertyMetadata("⭐", OnIconChanged));

    public static readonly DependencyProperty IconBackgroundProperty =
        DependencyProperty.Register("IconBackground", typeof(Brush), typeof(StatCard), 
            new PropertyMetadata(Brushes.Gray, OnIconBackgroundChanged));

    public StatCard()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public Brush IconBackground
    {
        get => (Brush)GetValue(IconBackgroundProperty);
        set => SetValue(IconBackgroundProperty, value);
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatCard card) card.TitleText.Text = (string)e.NewValue;
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatCard card) card.ValueText.Text = (string)e.NewValue;
    }

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatCard card) card.IconText.Text = (string)e.NewValue;
    }

    private static void OnIconBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatCard card) card.IconBorder.Background = (Brush)e.NewValue;
    }
}
