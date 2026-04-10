using System;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Editor;

public static class ContentDialogHelper
{
    public static async Task<MessageBoxResult> ShowAsync(IMainWindow owner, string messageBoxText, string caption, MessageBoxButton button)
    {
        if (owner is not Window)
        {
            throw new InvalidOperationException("Owner must be a Window.");
        }
        if (owner is not IContentDialogOwner contentDialogOwner)
        {
            throw new InvalidOperationException("Owner must implement IContentDialogOwner.");
        }

        contentDialogOwner.ContentDialogChanged(true);
        try
        {
            return await MessageBox.ShowAsync(messageBoxText, caption, button);
        }
        finally
        {
            contentDialogOwner.ContentDialogChanged(false);
        }
    }
}
