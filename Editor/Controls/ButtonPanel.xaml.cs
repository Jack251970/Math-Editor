using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Editor;

public partial class ButtonPanel : UserControl
{
    public event EventHandler ButtonClick = (x, y) => { };

    private MainWindow _mainWindow = null!;
    private readonly List<CommandDetails> _commandDetails;
    private readonly int _columns;
    private readonly int _buttonMargin;

    public ButtonPanel(List<CommandDetails> listCommandDetails, int columns, int buttonMargin)
    {
        _commandDetails = listCommandDetails;
        _columns = columns;
        _buttonMargin = buttonMargin;
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        _mainWindow = (MainWindow)Window.GetWindow(this);
        mainGrid.Columns = _columns;//listButtonDetails.Count < 5 ? listButtonDetails.Count : 5;
        mainGrid.Rows = (int)Math.Ceiling(_commandDetails.Count / (double)mainGrid.Columns);
        mainGrid.Width = 30 * mainGrid.Columns;
        mainGrid.Height = 30 * mainGrid.Rows;

        for (var i = 0; i < _commandDetails.Count; i++)
        {
            var b = new EditorToolBarButton(_mainWindow, _commandDetails[i])
            {
                Margin = new Thickness(_buttonMargin)
            };
            b.Click += new RoutedEventHandler(panelButton_Click);
            b.Style = (Style)FindResource("MathToolBarButtonStyle");
            b.SetValue(Grid.ColumnProperty, i % mainGrid.Columns);
            b.SetValue(Grid.RowProperty, i / mainGrid.Columns);
            b.FontFamily = FontFactory.GetFontFamily(FontType.STIXGeneral);
            //b.FontSize = 10;
            if (_commandDetails[i].Image != null)
            {
                b.Content = _commandDetails[i].Image;
            }
            else
            {
                b.Content = _commandDetails[i].UnicodeString;
            }
            mainGrid.Children.Add(b);
            if (_commandDetails[i].CommandType == CommandType.None) //This is an ugly kludge!
            {
                b.Visibility = Visibility.Hidden;
            }
        }
    }

    private void panelButton_Click(object sender, RoutedEventArgs e)
    {
        Visibility = Visibility.Collapsed;
        ButtonClick(this, EventArgs.Empty);
    }
}

public sealed class EditorToolBarButton(MainWindow mainWindow, CommandDetails commandDetails) : Button
{
    private readonly MainWindow _mainWindow = mainWindow;
    private readonly CommandDetails _commandDetails = commandDetails;

    protected override void OnClick()
    {
        base.OnClick();
        _mainWindow.HandleToolBarCommand(_commandDetails);
    }
}
