using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;

namespace Editor;

public static class ContentDialogHelper
{
    public static async Task<MessageBoxResult> ShowAsync(IMainWindow owner, string messageBoxText, string caption, MessageBoxButton button)
    {
        if (owner is not Window windowOwner)
        {
            throw new InvalidOperationException("Owner must be a Window.");
        }
        if (owner is not IContentDialogOwner contentDialogOwner)
        {
            throw new InvalidOperationException("Owner must implement IContentDialogOwner.");
        }

        var dialog = new ContentDialog
        {
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

        contentDialogOwner.ContentDialogChanged(true);
        try
        {
            var result = await dialog.ShowAsync(windowOwner);
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
        finally
        {
            contentDialogOwner.ContentDialogChanged(false);
        }
    }
}
