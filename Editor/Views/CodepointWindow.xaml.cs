using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Editor;

public partial class CodepointWindow : Window
{
    private string numberBase = "10";

    public CodepointWindow()
    {
        InitializeComponent();
        numberBox.Focus();
    }

    private bool ConvertToNumber(string str, out uint number)
    {
        try
        {
            switch (numberBase)
            {
                case "8":
                    number = Convert.ToUInt32(str, 8);
                    return true;
                case "10":
                    number = uint.Parse(str);
                    return true;
                case "16":
                    number = Convert.ToUInt32(str, 16);
                    return true;
            }
        }
        catch { }
        number = 0;
        return false;
    }

    private void numberBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !ConvertToNumber(e.Text, out var temp);
        base.OnPreviewTextInput(e);
    }

    private void insertButton_Click(object sender, RoutedEventArgs e)
    {
        if (ConvertToNumber(numberBox.Text, out var number))
        {
            try
            {
                var commandDetails = new CommandDetails { UnicodeString = Convert.ToChar(number).ToString(), CommandType = Editor.CommandType.Text };
                ((MainWindow)Owner).HandleToolBarCommand(commandDetails);
            }
            catch
            {
                MessageBox.Show("The given value is invalid.", "Input error");
            }
        }
        else
        {
            MessageBox.Show("The entered value is invalid.", "Input error");
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        Hide();
        e.Cancel = true;
    }

    private void closeButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void codeFormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        numberBase = (string)((ComboBoxItem)codeFormatComboBox.SelectedItem).Tag;
        if (!string.IsNullOrEmpty(numberBox.Text))
        {
            try
            {
                if (ConvertToNumber(numberBox.Text, out var number))
                {
                    numberBox.Text = Convert.ToString(number, int.Parse(numberBase));
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                MessageBox.Show("The number is not in correct format");
                numberBox.Text = "";
            }
        }
    }
}
