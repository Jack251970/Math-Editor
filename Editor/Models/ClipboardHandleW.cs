// Copyright (c) 2025 Jack251970
// Licensed under the Apache License. See the LICENSE.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Editor;

/// <summary>
/// ClipboardHandleW is a class that handles the clipboard
/// </summary>
public class ClipboardHandleW : IDisposable
{
    #region Fields

    private static string ClassName => nameof(ClipboardHandleW);

    private static readonly HRESULT CLIPBOARD_E_CANT_OPEN = unchecked((HRESULT)0x800401D0);
    private static readonly HRESULT CLIPBOARD_E_BAD_DATA = unchecked((HRESULT)0x800401D3);
    private static readonly HRESULT RPC_SERVER_UNAVAILABLE = unchecked((HRESULT)0x800706BA);

    private bool _ready;

    private HWND _handle = HWND.Null;

    #endregion

    #region Properties

    /// <summary>
    /// Checks if the handle is ready to monitor the system clipboard.
    /// It is used to provide a final value for use whenever the property
    /// 'ObserveLastEntry' is enabled.
    /// </summary>
    [Browsable(false)]
    internal bool Ready
    {
        get
        {
            if (ClipboardMonitorInstance.ObserveLastEntry)
            {
                _ready = true;
            }
            return _ready;
        }
        set => _ready = value;
    }

    // instant in monitor
    internal ClipboardMonitorW ClipboardMonitorInstance { get; set; } = null!;

    #endregion

    #region Methods

    #region Clipboard Management

    /// <summary>
    /// Starts monitoring the system clipboard.
    /// </summary>
    public void StartMonitoring()
    {
        if (Application.Current.MainWindow.IsLoaded)
        {
            MainWindow_Loaded(null, new RoutedEventArgs());
        }
        else
        {
            Application.Current.MainWindow.Loaded += MainWindow_Loaded;
        }
    }

    private async void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            // Get the handle of the main window.
            var handle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            _handle = new(handle);

            // Add the hook to the window.
            var win = HwndSource.FromHwnd(handle);
            win.AddHook(WndProc);

            // Add clipboard format listener
            if (await RetryActionAsync(AddClipboardFormatListener))
            {
                Ready = true;
            }
        }
        catch (Exception ex)
        {
            EditorLogger.Error(ClassName, "Failed to start clipboard monitoring.", ex);
        }
    }

    /// <summary>
    /// Stops monitoring the system clipboard.
    /// </summary>
    public async void StopMonitoring()
    {
        if (await RetryActionAsync(RemoveClipboardFormatListener))
        {
            Ready = false;
        }
    }

    /// <summary>
    /// Add the clipboard format listener to the system clipboard.
    /// </summary>
    /// <returns>
    /// Returns true if the clipboard format listener was added successfully.
    /// </returns>
    private bool AddClipboardFormatListener()
    {
        if (_handle != HWND.Null)
        {
            var result = PInvoke.AddClipboardFormatListener(_handle);
            EditorLogger.Debug(ClassName, "Clipboard format listener added.");
            return result;
        }

        return false;
    }

    /// <summary>
    /// Retry an action asynchronously.
    /// </summary>
    /// <param name="action">
    /// The action to retry.
    /// </param>
    /// <param name="retryInterval">
    /// The interval between retries.
    /// </param>
    /// <param name="maxAttemptCount">
    /// The maximum count.
    /// </param>
    /// <returns>
    /// Returns a <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    private static async Task<bool> RetryActionAsync(Func<bool> action, int retryInterval = 100, int maxAttemptCount = 3)
    {
        for (int i = 0; i < maxAttemptCount; i++)
        {
            try
            {
                if (action())
                {
                    break;
                }
            }
            catch (Exception)
            {
                if (i == maxAttemptCount - 1)
                {
                    return false;
                }

                await Task.Delay(retryInterval);
            }
        }

        return true;
    }

    /// <summary>
    /// Remove the clipboard format listener to the system clipboard.
    /// </summary>
    /// <returns>
    /// Returns true if the clipboard format listener was removed successfully.
    /// </returns>
    private bool RemoveClipboardFormatListener()
    {
        if (_handle != HWND.Null)
        {
            var result = PInvoke.RemoveClipboardFormatListener(_handle);
            EditorLogger.Debug(ClassName, "Clipboard format listener removed.");
            return result;
        }

        return false;
    }

    /// <summary>
    /// Handles the clipboard update event.
    /// </summary>
    /// <param name="hwnd"> Handle to the window that receives the message. </param>
    /// <param name="msg"> The message. </param>
    /// <param name="wParam"> Additional message information. </param>
    /// <param name="lParam"> Additional message information. </param>
    /// <param name="handled"> Whether the message was handled. </param>
    /// <returns></returns>
    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == PInvoke.WM_CLIPBOARDUPDATE)
        {
            OnClipboardChanged();
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Handles the clipboard data change event.
    /// </summary>
    private async void OnClipboardChanged()
    {
        try
        {
            // If clipboard-monitoring is enabled, proceed to listening.
            if (!Ready || ClipboardMonitorInstance == null || !ClipboardMonitorInstance.MonitorClipboard)

            {
                return;
            }

            if (EquationRoot.CanPasteFromClipboard(out var content))
            {
                ClipboardMonitorInstance.Invoke(true, content);
            }
            else
            {
                ClipboardMonitorInstance.Invoke(false, null);
            }
        }
        catch (AccessViolationException)
        {
            // Use-cases such as Remote Desktop usage might throw this exception.
            // Applications with Administrative privileges can however override
            // this exception when run in a production environment.
        }
        catch (COMException e) when (e.HResult == CLIPBOARD_E_CANT_OPEN)
        {
            // Sometimes the clipboard is locked and cannot be accessed.
            // System.Runtime.InteropServices.COMException (0x800401D0)
            // OpenClipboard Failed (0x800401D0 (CLIPBRD_E_CANT_OPEN))
        }
        catch (COMException e) when (e.HResult == CLIPBOARD_E_BAD_DATA)
        {
            // Sometimes data on clipboard is invalid.
            // System.Runtime.InteropServices.COMException (0x800401D3)
            // Bad data in clipboard (0x800401D3 (CLIPBRD_E_BAD_DATA))
        }
        catch (COMException e) when (e.HResult == RPC_SERVER_UNAVAILABLE)
        {
            // Sometimes the clipboard is locked and cannot be accessed.
            // System.Runtime.InteropServices.COMException (0x800706BA)
            // RPC server is unavailable (0x800706BA (RPC_E_SERVER_UNAVAILABLE))
        }
        catch (Exception e)
        {
            EditorLogger.Error(ClassName, "Clipboard changed event failed.", e);
        }
    }

    #endregion

    #endregion

    #region IDisposable

    private bool _disposed;

    /// <summary>
    /// Disposes of the clipboard-monitoring resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes all the resources associated with this component.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            StopMonitoring();
            _disposed = true;
        }
    }

    #endregion
}
