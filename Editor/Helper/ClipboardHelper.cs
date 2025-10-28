using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

public class ClipboardHelper : ObservableObject, IDisposable
{
    private readonly ClipboardMonitorW _clipboardMonitorW;

    public bool CanPaste => _clipboardMonitorW.CanPaste;

    public object? PasteObject => _clipboardMonitorW.PasteObject;

    public ClipboardHelper()
    {
        _clipboardMonitorW = new ClipboardMonitorW();
    }

    public void StartMonitoring()
    {
        _clipboardMonitorW.CanPasteChanged += ClipboardMonitorW_CanPasteChanged;
        _clipboardMonitorW.StartMonitoring();
    }

    private void ClipboardMonitorW_CanPasteChanged(object? sender, bool e)
    {
        OnPropertyChanged(nameof(CanPaste));
    }

    public void Dispose()
    {
        _clipboardMonitorW.CanPasteChanged -= ClipboardMonitorW_CanPasteChanged;
        _clipboardMonitorW.Dispose();
    }
}
