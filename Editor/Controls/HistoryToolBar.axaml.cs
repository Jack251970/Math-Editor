using Avalonia.Controls;

namespace Editor;

// TODO: Add support for overflow items
public partial class HistoryToolBar : UserControl
{
    private readonly IMainWindow _mainWindow = null!;

    public HistoryToolBar() : this(null!)
    {
    }

    public HistoryToolBar(IMainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        DataContext = this;
        InitializeComponent();
        RecentListBox.ItemsSource = App.Settings.RecentSymbolList;
        RecentListBox.FontFamily = FontFactory.GetFontFamily(FontType.STIXGeneral);
    }

    private void RecentListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (RecentListBox.SelectedItem is string str)
        {
            var commandDetails = new CommandDetails { UnicodeString = str, CommandType = CommandType.Text };
            _mainWindow.HandleToolBarCommand(commandDetails);
            _mainWindow.TryHideCharacterToolBarVisiblePanel();
            _mainWindow.TryHideEquationToolBarVisiblePanel();
            RecentListBox.SelectionChanged -= RecentListBox_SelectionChanged;
            RecentListBox.SelectedItem = null;
            RecentListBox.SelectionChanged += RecentListBox_SelectionChanged;
        }
    }
}
