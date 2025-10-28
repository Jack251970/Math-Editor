// Copyright (c) 2025 Jack251970
// Licensed under the Apache License. See the LICENSE.

using System;
using System.Windows;
using System.Windows.Threading;

namespace Editor;

/// <summary>
/// ClipboardMonitorW is a class that monitors the clipboard
/// </summary>
public class ClipboardMonitorW
{
    #region Fields

    private static string ClassName => nameof(ClipboardMonitorW);

    private DispatcherTimer _timer = new();
    private ClipboardHandleW _clipboardHandle = new();

    private bool _startMonitoring;
    private bool _canPaste;

    #endregion

    #region Properties

    #region Browsable

    public bool MonitorClipboard
    {
        get; set;
    }

    public bool ObserveLastEntry
    {
        get; set;
    }

    #endregion

    #region Non-browsable

    public bool CanPaste
    {
        get => _canPaste;
        set
        {
            if (_canPaste != value)
            {
                _canPaste = value;
                CanPasteChanged?.Invoke(this, _canPaste);
            }
        }
    }

    public object? PasteObject
    {
        get; set;
    }

    #endregion

    #endregion

    #region Constructors

    public ClipboardMonitorW()
    {
        _timer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 1000),
            IsEnabled = false
        };
        _timer.Tick += Timer_Tick;

        SetDefaults();
    }

    #endregion

    #region Methods

    #region Public

    /// <summary>
    /// Starts the clipboard-monitoring process and
    /// initializes the system clipboard-access handle.
    /// </summary>
    public void StartMonitoring()
    {
        if (!_startMonitoring)
        {
            _timer.Start();
            _timer.IsEnabled = true;
        }
    }

    /// <summary>
    /// Pauses the clipboard-monitoring process.
    /// </summary>
    public void PauseMonitoring()
    {
        if (MonitorClipboard)
        {
            MonitorClipboard = false;
            EditorLogger.Info(ClassName, "Clipboard monitoring paused.");
        }
    }

    /// <summary>
    /// Resumes the clipboard-monitoring process.
    /// </summary>
    public void ResumeMonitoring()
    {
        if (!MonitorClipboard)
        {
            MonitorClipboard = true;
            EditorLogger.Info(ClassName, "Clipboard monitoring resumed.");
        }
    }

    /// <summary>
    /// Ends the clipboard-monitoring process and
    /// shuts the system clipboard-access handle.
    /// </summary>
    public void StopMonitoring()
    {
        if (_startMonitoring)
        {
            _clipboardHandle.StopMonitoring();
            _startMonitoring = false;
            EditorLogger.Info(ClassName, "Clipboard monitoring stopped.");
        }
    }

    /// <summary>
    /// Clears the clipboard of all data.
    /// </summary>
    public void CleanClipboard()
    {
        PasteObject = null;
    }

    #endregion

    #region Private

    /// <summary>
    /// Apply library-default settings and launch code.
    /// </summary>
    private void SetDefaults()
    {
        _clipboardHandle.ClipboardMonitorInstance = this;

        MonitorClipboard = true;
        ObserveLastEntry = true;

        if (EquationRoot.CanPasteFromClipboard(out var content))
        {
            CanPaste = true;
            PasteObject = content;
        }
    }

    internal void Invoke(bool canPaste, object? pasteObject)
    {
        CanPaste = canPaste;
        PasteObject = pasteObject;
    }

    #endregion

    #endregion

    #region Events

    #region Event Handlers

    public event EventHandler<bool>? CanPasteChanged = null;

    #endregion

    #region Private

    private void Timer_Tick(object? sender, EventArgs e)
    {
        // Wait until the dispatcher is ready & main window is initialized
        if (Application.Current.Dispatcher == null)
        {
            return;
        }
        else if (Application.Current.MainWindow == null)
        {
            return;
        }

        // Stop the timer & start monitoring
        _timer.Stop();
        _timer.IsEnabled = false;
        if (!_startMonitoring)
        {
            _clipboardHandle.StartMonitoring();
            _startMonitoring = true;
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

    // <summary>
    /// Disposes all the resources associated with this component.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _clipboardHandle.Dispose();
            _timer.Stop();
            _timer = null!;
            _clipboardHandle = null!;
            CleanClipboard();
            _disposed = true;
        }
    }

    #endregion
}
