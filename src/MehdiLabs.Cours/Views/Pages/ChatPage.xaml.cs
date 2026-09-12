using System.Collections.Specialized;
using System.Windows.Controls;
using MehdiLabs.Cours.ViewModels;

namespace MehdiLabs.Cours.Views.Pages;

public partial class ChatPage : Page
{
    private ChatbotViewModel _vm;

    public ChatPage(ChatbotViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;

        // Auto-scroll
        ((INotifyCollectionChanged)_vm.CurrentMessages).CollectionChanged += (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                MessagesScroll.ScrollToBottom();
            }
        };
    }
}
