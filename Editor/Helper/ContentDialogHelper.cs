using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using iNKORE.UI.WPF.Modern.Controls;

namespace Editor;

public static class ContentDialogHelper
{
    public static async Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button)
    {
        var dialog = new ContentDialog
        {
            Owner = owner,
            Title = caption
        };
        switch (button)
        {
            case MessageBoxButton.OK:
                dialog.PrimaryButtonText = Localize.Ok();
                break;
            case MessageBoxButton.YesNo:
                dialog.PrimaryButtonText = Localize.Yes();
                dialog.SecondaryButtonText = Localize.No();
                break;
            case MessageBoxButton.OKCancel:
                dialog.PrimaryButtonText = Localize.Ok();
                dialog.SecondaryButtonText = Localize.Cancel();
                break;
            case MessageBoxButton.YesNoCancel:
                dialog.PrimaryButtonText = Localize.Yes();
                dialog.SecondaryButtonText = Localize.No();
                dialog.CloseButtonText = Localize.Cancel();
                break;
        }
        dialog.DefaultButton = ContentDialogButton.Primary;
        dialog.Content = new TextBlock
        {
            Text = messageBoxText,
            TextWrapping = TextWrapping.Wrap
        };

        var result = await dialog.ShowAsync();
        return button switch
        {
            MessageBoxButton.OK => result switch
            {
                ContentDialogResult.Primary => MessageBoxResult.OK,
                _ => MessageBoxResult.None
            },
            MessageBoxButton.YesNo => result switch
            {
                ContentDialogResult.Primary => MessageBoxResult.Yes,
                ContentDialogResult.Secondary => MessageBoxResult.No,
                _ => MessageBoxResult.None
            },
            MessageBoxButton.OKCancel => result switch
            {
                ContentDialogResult.Primary => MessageBoxResult.OK,
                ContentDialogResult.Secondary => MessageBoxResult.Cancel,
                _ => MessageBoxResult.None
            },
            MessageBoxButton.YesNoCancel => result switch
            {
                ContentDialogResult.Primary => MessageBoxResult.Yes,
                ContentDialogResult.Secondary => MessageBoxResult.No,
                _ => MessageBoxResult.Cancel
            },
            _ => MessageBoxResult.None,
        };
    }
}
