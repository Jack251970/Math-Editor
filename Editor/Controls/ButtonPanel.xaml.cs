using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Editor;

public partial class ButtonPanel : UserControl
{
    public event EventHandler ButtonClick = (x, y) => { };

    private readonly List<CommandDetails> _commandDetails;

    public ButtonPanel(MainWindow mainWindow, List<CommandDetails> listCommandDetails, int columns, int buttonMargin)
    {
        InitializeComponent();
        _commandDetails = listCommandDetails;
        MainGrid.Columns = columns;//listButtonDetails.Count < 5 ? listButtonDetails.Count : 5;
        MainGrid.Rows = (int)Math.Ceiling(listCommandDetails.Count / (double)MainGrid.Columns);
        MainGrid.Width = 30 * MainGrid.Columns;
        MainGrid.Height = 30 * MainGrid.Rows;
        for (var i = 0; i < _commandDetails.Count; i++)
        {
            var button = new EditorToolBarButton(mainWindow, _commandDetails[i])
            {
                Margin = new Thickness(buttonMargin)
            };
            button.Click += new RoutedEventHandler(PanelButton_Click);
            button.Style = (Style)FindResource("MathToolBarButtonStyle");
            button.SetValue(Grid.ColumnProperty, i % MainGrid.Columns);
            button.SetValue(Grid.RowProperty, i / MainGrid.Columns);
            button.FontFamily = FontFactory.GetFontFamily(FontType.STIXGeneral);
            //b.FontSize = 10;
            if (_commandDetails[i].Image != null)
            {
                button.Content = _commandDetails[i].Image;
            }
            else
            {
                button.Content = _commandDetails[i].UnicodeString;
            }
            MainGrid.Children.Add(button);
            if (_commandDetails[i].CommandType == CommandType.None) //This is an ugly kludge!
            {
                button.Visibility = Visibility.Hidden;
            }
        }
    }

    private void PanelButton_Click(object sender, RoutedEventArgs e)
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
