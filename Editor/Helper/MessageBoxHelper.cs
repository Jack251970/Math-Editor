using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Controls;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.ViewModels;

namespace Editor;

public static class MessageBox
{
    private static readonly SemaphoreSlim _messageBoxSlim = new(1, 1);

    public static void Show(string messageBoxText)
    {
        _ = ShowAsync(messageBoxText);
    }

    public static void Show(string messageBoxText, string caption)
    {
        _ = ShowAsync(messageBoxText, caption);
    }

    public static void Show(string messageBoxText, string caption, MessageBoxButton button)
    {
        _ = ShowAsync(messageBoxText, caption, button);
    }

    public static void Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
    {
        _ = ShowAsync(messageBoxText, caption, button, icon);
    }

    public static async Task<MessageBoxResult> ShowAsync(string messageBoxText)
    {
        await _messageBoxSlim.WaitAsync();
        try
        {
            var box = GetEditorMessageBox(messageBoxText, string.Empty, ButtonEnum.Ok);
            return await GetResultAsync(box);
        }
        finally
        {
            _messageBoxSlim.Release();
        }
    }

    public static async Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption)
    {
        await _messageBoxSlim.WaitAsync();
        try
        {
            var box = GetEditorMessageBox(messageBoxText, caption, ButtonEnum.Ok);
            return await GetResultAsync(box);
        }
        finally
        {
            _messageBoxSlim.Release();
        }
    }

    public static async Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button)
    {
        await _messageBoxSlim.WaitAsync();
        try
        {
            var box = button switch
            {
                MessageBoxButton.OK => GetEditorMessageBox(messageBoxText, caption, ButtonEnum.Ok),
                MessageBoxButton.OKCancel => GetEditorMessageBox(messageBoxText, caption, ButtonEnum.OkCancel),
                MessageBoxButton.YesNo => GetEditorMessageBox(messageBoxText, caption, ButtonEnum.YesNo),
                MessageBoxButton.YesNoCancel => GetEditorMessageBox(messageBoxText, caption, ButtonEnum.YesNoCancel),
                _ => GetEditorMessageBox(messageBoxText, caption, ButtonEnum.Ok),
            };
            return await GetResultAsync(box);
        }
        finally
        {
            _messageBoxSlim.Release();
        }
    }

    public static async Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
    {
        await _messageBoxSlim.WaitAsync();
        try
        {
            var box = button switch
            {
                MessageBoxButton.OK => GetEditorMessageBox(messageBoxText, caption, ButtonEnum.Ok, GetIcon(icon)),
                MessageBoxButton.OKCancel => GetEditorMessageBox(messageBoxText, caption, ButtonEnum.OkCancel, GetIcon(icon)),
                MessageBoxButton.YesNo => GetEditorMessageBox(messageBoxText, caption, ButtonEnum.YesNo, GetIcon(icon)),
                MessageBoxButton.YesNoCancel => GetEditorMessageBox(messageBoxText, caption, ButtonEnum.YesNoCancel, GetIcon(icon)),
                _ => GetEditorMessageBox(messageBoxText, caption, ButtonEnum.Ok, GetIcon(icon)),
            };
            return await GetResultAsync(box);
        }
        finally
        {
            _messageBoxSlim.Release();
        }
    }

    private static async Task<MessageBoxResult> GetResultAsync(this IMsBox<ButtonResult> box)
    {
        return await box.ShowAsync() switch
        {
            ButtonResult.Ok => MessageBoxResult.OK,
            ButtonResult.Cancel => MessageBoxResult.Cancel,
            ButtonResult.Yes => MessageBoxResult.Yes,
            ButtonResult.No => MessageBoxResult.No,
            _ => MessageBoxResult.None,
        };
    }

    private static Icon GetIcon(this MessageBoxImage image)
    {
        return image switch
        {
            MessageBoxImage.None => Icon.None,
            MessageBoxImage.Error => Icon.Error,
            MessageBoxImage.Question => Icon.Question,
            MessageBoxImage.Warning => Icon.Warning,
            MessageBoxImage.Information => Icon.Info,
            _ => Icon.None,
        };
    }

    public static IMsBox<ButtonResult> GetEditorMessageBox(string title, string text, ButtonEnum @enum = ButtonEnum.Ok, Icon icon = Icon.None)
    {
        var msBoxStandardViewModel = new MsBoxStandardViewModel(new MessageBoxStandardParams
        {
            ContentTitle = title,
            ContentMessage = text,
            ButtonDefinitions = @enum,
            Icon = icon,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Topmost = true
        });
        return new MsBox<MsBoxStandardView, MsBoxStandardViewModel, ButtonResult>(new MsBoxStandardView
        {
            DataContext = msBoxStandardViewModel
        }, msBoxStandardViewModel);
    }
}

public enum MessageBoxResult
{
    //
    // Summary:
    //     The message box returns no result.
    None = 0,
    //
    // Summary:
    //     The result value of the message box is OK.
    OK = 1,
    //
    // Summary:
    //     The result value of the message box is Cancel.
    Cancel = 2,
    //
    // Summary:
    //     The result value of the message box is Yes.
    Yes = 6,
    //
    // Summary:
    //     The result value of the message box is No.
    No = 7
}

public enum MessageBoxButton
{
    //
    // Summary:
    //     The message box displays an OK button.
    OK = 0,
    //
    // Summary:
    //     The message box displays OK and Cancel buttons.
    OKCancel = 1,
    //
    // Summary:
    //     The message box displays Yes, No, and Cancel buttons.
    YesNoCancel = 3,
    //
    // Summary:
    //     The message box displays Yes and No buttons.
    YesNo = 4
}

public enum MessageBoxImage
{
    //
    // Summary:
    //     The message box contains no symbols.
    None = 0,
    //
    // Summary:
    //     The message box contains a symbol consisting of white X in a circle with a red
    //     background.
    Error = 16,
    //
    // Summary:
    //     The message box contains a symbol consisting of a white X in a circle with a
    //     red background.
    Hand = 16,
    //
    // Summary:
    //     The message box contains a symbol consisting of white X in a circle with a red
    //     background.
    Stop = 16,
    //
    // Summary:
    //     The message box contains a symbol consisting of a question mark in a circle.
    //     The question mark message icon is no longer recommended because it does not clearly
    //     represent a specific type of message and because the phrasing of a message as
    //     a question could apply to any message type. In addition, users can confuse the
    //     question mark symbol with a help information symbol. Therefore, do not use this
    //     question mark symbol in your message boxes. The system continues to support its
    //     inclusion only for backward compatibility.
    Question = 32,
    //
    // Summary:
    //     The message box contains a symbol consisting of an exclamation point in a triangle
    //     with a yellow background.
    Exclamation = 48,
    //
    // Summary:
    //     The message box contains a symbol consisting of an exclamation point in a triangle
    //     with a yellow background.
    Warning = 48,
    //
    // Summary:
    //     The message box contains a symbol consisting of a lowercase letter i in a circle.
    Asterisk = 64,
    //
    // Summary:
    //     The message box contains a symbol consisting of a lowercase letter i in a circle.
    Information = 64
}
