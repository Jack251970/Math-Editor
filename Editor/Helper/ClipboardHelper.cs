using System;
using System.Threading;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using Timer = System.Timers.Timer;

namespace Editor;

public class ClipboardHelper : ObservableObject, IDisposable
{
    private static readonly string ClassName = nameof(ClipboardHelper);

    private readonly Timer _timer = new(500);

    private bool _canPaste;
    public bool CanPaste
    {
        get => _canPaste;
        set
        {
            if (_canPaste != value)
            {
                _canPaste = value;
                OnPropertyChanged(nameof(CanPaste));
            }
        }
    }

    public object? PasteObject { get; private set; }

    private bool _isMonitoring;
    private readonly SemaphoreSlim _updatingLock = new(1, 1);

    public void StartMonitoring()
    {
        if (_isMonitoring) return;

        _isMonitoring = true;
        _timer.Elapsed += Timer_Elapsed;
        _timer.Start();
    }

    private async void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        // If another update is in progress, skip this tick
        if (_updatingLock.CurrentCount == 0)
        {
            return;
        }

        await _updatingLock.WaitAsync();
        try
        {
            var canPaste = EquationRoot.CanPasteFromClipboard(out var data);
            PasteObject = data;
            CanPaste = canPaste;
        }
        catch (Exception ex)
        {
            EditorLogger.Error(ClassName, "Error checking clipboard contents", ex);
        }
        finally
        {
            _updatingLock.Release();
        }
    }

    #region IDisposable

    private bool _isDisposed = false;

    ~ClipboardHelper()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            _timer.Elapsed -= Timer_Elapsed;
            _timer.Dispose();
        }
    }

    #endregion
}
