using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Editor;

public partial class ButtonPanel : UserControl
{
    public event EventHandler ButtonClick = (x, y) => { };

    private readonly List<CommandDetails> commandDetails;
    public ButtonPanel(List<CommandDetails> listCommandDetails, int columns, int buttonMargin)
    {
        InitializeComponent();
        commandDetails = listCommandDetails;
        mainGrid.Columns = columns;//listButtonDetails.Count < 5 ? listButtonDetails.Count : 5;
        mainGrid.Rows = (int)Math.Ceiling(listCommandDetails.Count / (double)mainGrid.Columns);
        mainGrid.Width = 30 * mainGrid.Columns;
        mainGrid.Height = 30 * mainGrid.Rows;

        for (var i = 0; i < commandDetails.Count; i++)
        {
            var b = new EditorToolBarButton(commandDetails[i])
            {
                Margin = new Thickness(buttonMargin)
            };
            b.Click += new RoutedEventHandler(panelButton_Click);
            b.Style = (Style)FindResource("MathToolBarButtonStyle");
            b.SetValue(Grid.ColumnProperty, i % mainGrid.Columns);
            b.SetValue(Grid.RowProperty, i / mainGrid.Columns);
            b.FontFamily = FontFactory.GetFontFamily(FontType.STIXGeneral);
            //b.FontSize = 10;
            if (commandDetails[i].Image != null)
            {
                b.Content = commandDetails[i].Image;
            }
            else
            {
                b.Content = commandDetails[i].UnicodeString;
            }
            mainGrid.Children.Add(b);
            if (commandDetails[i].CommandType == CommandType.None) //This is an ugly kludge!
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

public sealed class EditorToolBarButton(CommandDetails commandDetails) : Button
{
    private readonly CommandDetails commandDetails = commandDetails;

    protected override void OnClick()
    {
        base.OnClick();
        ((MainWindow)Application.Current.MainWindow).HandleToolBarCommand(commandDetails);
    }
}
