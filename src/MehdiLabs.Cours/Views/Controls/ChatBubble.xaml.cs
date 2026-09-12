using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MehdiLabs.Cours.Views.Controls;

public partial class ChatBubble : UserControl
{
    public static readonly DependencyProperty RoleProperty =
        DependencyProperty.Register("Role", typeof(string), typeof(ChatBubble), 
            new PropertyMetadata("user", OnRoleChanged));

    public static readonly DependencyProperty ContentTextProperty =
        DependencyProperty.Register("ContentText", typeof(string), typeof(ChatBubble), 
            new PropertyMetadata("", OnContentChanged));

    public ChatBubble()
    {
        InitializeComponent();
    }

    public string Role
    {
        get => (string)GetValue(RoleProperty);
        set => SetValue(RoleProperty, value);
    }

    public string ContentText
    {
        get => (string)GetValue(ContentTextProperty);
        set => SetValue(ContentTextProperty, value);
    }

    private static void OnRoleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ChatBubble bubble)
        {
            var role = (string)e.NewValue;
            var isUser = role == "user";

            bubble.RootGrid.HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            bubble.BubbleBorder.Background = isUser 
                ? (Brush)Application.Current.Resources["AccentBrush"] 
                : (Brush)Application.Current.Resources["BgSidebarBrush"];
            
            bubble.ContentTextBox.Foreground = isUser 
                ? Brushes.White 
                : (Brush)Application.Current.Resources["FgPrimaryBrush"];
            
            bubble.RoleText.Text = isUser ? "Vous" : "IA";
            bubble.RoleText.Foreground = bubble.ContentTextBox.Foreground;
            bubble.RoleText.HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }
    }

    private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ChatBubble bubble) bubble.ContentTextBox.Text = (string)e.NewValue;
    }
}
