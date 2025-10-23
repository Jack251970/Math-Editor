using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor;

public partial class HistoryToolBar : UserControl
{
    private const int MaxSymbols = 30;

    public HistoryToolBar()
    {
        DataContext = this;
        InitializeComponent();
        recentListBox.ItemsSource = App.Settings.RecentSymbolList;
        recentListBox.FontFamily = FontFactory.GetFontFamily(FontType.STIXGeneral);
    }

    public static void AddItem(string symbol)
    {
        // Add to used list
        if (!App.Settings.UsedSymbolList.TryAdd(symbol, 1))
        {
            App.Settings.UsedSymbolList[symbol] += 1;
        }

        // Add to recent list
        var recentList = App.Settings.RecentSymbolList;
        if (recentList.Count >= MaxSymbols)
        {
            recentList.RemoveAt(recentList.Count - 1);
        }
        recentList.Insert(0, symbol);
    }

    private void symbolClick(object sender, MouseButtonEventArgs e)
    {
        if (((TextBlock)sender).DataContext is string str)
        {
            var commandDetails = new CommandDetails { UnicodeString = str, CommandType = CommandType.Text };
            ((MainWindow)Window.GetWindow(this)).HandleToolBarCommand(commandDetails);
        }
    }
}
