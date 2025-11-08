using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Editor;

public partial class UnicodeSelectorWindow : Window, ICultureInfoChanged
{
    public UnicodeSelectorWindowViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<UnicodeSelectorWindowViewModel>();

    public UnicodeSelectorWindow()
    {
        DataContext = ViewModel;
        InitializeComponent();
    }

    private void InsertButton_Click(object sender, RoutedEventArgs e)
    {
        var item = ViewModel.GetSelectedItem();
        if (item != null)
        {
            var commandDetails = new CommandDetails
            {
                UnicodeString = item.UnicodeText,
                CommandType = CommandType.Text
            };
            ((IMainWindow)Owner!).HandleToolBarCommand(commandDetails);
            if (ViewModel.SelectRecentList == false)
            {
                var recentList = App.Settings.RecentUnicodeItems;
                if (recentList.Count >= Constants.MaxSymbols)
                {
                    recentList.RemoveAt(recentList.Count - 1);
                }
                recentList.Insert(0, item);
            }
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_Closing(object sender, WindowClosingEventArgs e)
    {
        App.Settings.Save();
    }

    public void OnCultureInfoChanged(CultureInfo newCultureInfo)
    {
        ViewModel.OnCultureInfoChanged(newCultureInfo);
    }
}
