using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor;

public partial class HistoryToolBar : UserControl
{
    private readonly int maxSymbols = 30;
    private readonly ObservableCollection<string> recentList = [];
    private readonly Dictionary<string, int> usedCount = [];

    public HistoryToolBar()
    {
        DataContext = this;
        InitializeComponent();
        recentListBox.ItemsSource = recentList;
        var data = ConfigManager.GetConfigurationValue(KeyName.symbols);
        if (data.Length > 0)
        {
            var list = data.Split(',');
            foreach (var s in list)
            {
                recentList.Add(s);
                usedCount.Add(s, 0);
            }
        }
        recentListBox.FontFamily = FontFactory.GetFontFamily(FontType.STIXGeneral);
    }

    public void AddItem(string symbol)
    {
        if (!usedCount.TryAdd(symbol, 1))
        {
            usedCount[symbol] += 1;
        }
        else
        {
            if (usedCount.Count >= maxSymbols)
            {
                var min = int.MaxValue;
                var s = usedCount.First().Key;
                foreach (var pair in usedCount)
                {
                    if (pair.Value < min)
                    {
                        min = pair.Value;
                        s = pair.Key;
                    }
                }
                recentList.Remove(s);
                usedCount.Remove(s);
            }
            recentList.Insert(0, symbol);
        }
    }

    private void symbolClick(object sender, MouseButtonEventArgs e)
    {
        if (((TextBlock)sender).DataContext is string str && Application.Current?.MainWindow is MainWindow win)
        {
            CommandDetails commandDetails = new CommandDetails { UnicodeString = str, CommandType = CommandType.Text };
            win.HandleToolBarCommand(commandDetails);
        }
    }

    public void Save()
    {
        var data = "";
        foreach (var s in recentList)
        {
            data += s + ",";
        }
        data = data.Trim(',');
        ConfigManager.SetConfigurationValue(KeyName.symbols, data);
    }
}
