using System;
using System.Windows;
using System.Windows.Controls;

namespace Editor;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        DataContext = this;
        InitializeComponent();
        // TODO: Use Binding here and remove ok & cancel buttons
        try
        {
            var modes = editorModeCombo.Items;
            foreach (ComboBoxItem item in modes)
            {
                if ((string)item.Tag == App.Settings.DefaultMode.ToString())
                {
                    editorModeCombo.SelectedItem = item;
                }
            }
            var fonts = equationFontCombo.Items;
            foreach (ComboBoxItem item in fonts)
            {
                if ((string)item.Tag == App.Settings.DefaultFont)
                {
                    equationFontCombo.SelectedItem = item;
                }
            }
        }
        catch { }
    }

    private void okButton_Click(object sender, RoutedEventArgs e)
    {
        App.Settings.DefaultMode = Enum.Parse<EditorMode>(((ComboBoxItem)editorModeCombo.SelectedItem).Tag.ToString());
        App.Settings.DefaultFont = ((ComboBoxItem)equationFontCombo.SelectedItem).Tag.ToString();
        App.Settings.Save();
        Close();
    }

    private void cancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
