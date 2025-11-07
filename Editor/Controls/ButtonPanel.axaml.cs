using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Editor;

public partial class ButtonPanel : UserControl
{
    public event EventHandler? ButtonClick;

    private readonly List<CommandDetails> _commandDetails;

    public ButtonPanel() : this(null!, [], 5, 2)
    {
    }

    public ButtonPanel(IMainWindow mainWindow, List<CommandDetails> listCommandDetails, int columns, int buttonPadding)
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
                Padding = new Thickness(buttonPadding),
                // TODO: Add theme-aware styling and remove this
                Foreground = Brushes.Black
            };
            button.Click += PanelButton_Click;
            button.SetValue(Grid.ColumnProperty, i % MainGrid.Columns);
            button.SetValue(Grid.RowProperty, i / MainGrid.Columns);
            button.FontFamily = FontFactory.GetFontFamily(FontType.STIXGeneral);
            if (_commandDetails[i].Image != null)
            {
                button.Content = _commandDetails[i].Image;
            }
            else
            {
                button.Content = _commandDetails[i].UnicodeString;
            }
            MainGrid.Children.Add(button);
            if (_commandDetails[i].CommandType == CommandType.None)
            {
                button.Content = string.Empty;
                button.IsEnabled = false;
            }
        }
    }

    private void PanelButton_Click(object? sender, RoutedEventArgs e)
    {
        IsVisible = false;
        ButtonClick?.Invoke(this, EventArgs.Empty);
    }
}

public sealed class EditorToolBarButton(IMainWindow mainWindow, CommandDetails commandDetails) : Button()
{
    private readonly IMainWindow _mainWindow = mainWindow;
    private readonly CommandDetails _commandDetails = commandDetails;

    public EditorToolBarButton() : this(null!, null!)
    {
    }

    protected override void OnClick()
    {
        base.OnClick();
        _mainWindow.HandleToolBarCommand(_commandDetails);
    }
}
