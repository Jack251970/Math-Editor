using System;
using System.Timers;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

public class ClipboardHelper : ObservableObject, IDisposable
{
    private static readonly string ClassName = nameof(ClipboardHelper);

    // TODO: Wait for avalonia to support image type format
    private TopLevel _owner = null!;
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
    private bool _isReading;

    public void StartMonitoring(IMainWindow mainWindow)
    {
        if (_isMonitoring) return;

        _isMonitoring = true;
        _owner = mainWindow.TopLevel;
        _timer.Elapsed += Timer_Elapsed;
        _timer.Start();
    }

    private async void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        if (_isReading) return;

        _isReading = true;
        try
        {
            PasteObject = await EquationRoot.CanPasteFromClipboard();
            CanPaste = PasteObject != null;
        }
        catch (Exception ex)
        {
            EditorLogger.Error(ClassName, "Error checking clipboard contents", ex);
        }
        finally
        {
            _isReading = false;
        }
    }

    public void Dispose()
    {
        _timer.Elapsed -= Timer_Elapsed;
        _timer.Dispose();
    }
}
