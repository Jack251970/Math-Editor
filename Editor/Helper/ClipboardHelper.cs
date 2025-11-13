using System;
using System.Threading;
using System.Timers;
using System.Xml.Linq;
using Avalonia.Media.Imaging;
using Clowd.Clipboard;
using Clowd.Clipboard.Formats;
using CommunityToolkit.Mvvm.ComponentModel;
using Timer = System.Timers.Timer;

namespace Editor;

public class ClipboardHelper : ObservableObject, IDisposable
{
    private static readonly string ClassName = nameof(ClipboardHelper);

    private static readonly ClipboardFormat<string> ClipboardXmlFormat
        = ClipboardFormat.CreateCustomFormat(
            $"{typeof(MathEditorData).FullName}.{nameof(MathEditorData.XmlString)}", new TextUtf8Converter());

    private readonly Timer _timer = new(1000);

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
            var canPaste = CanPasteFromClipboard(out var data);
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

    private static bool CanPasteFromClipboard(out object? data)
    {
        data = null;
        try
        {
            using var handle = ClipboardAvalonia.Open();
            if (handle.ContainsFormat(ClipboardXmlFormat))
            {
                var xmlString = handle.GetFormatType(ClipboardXmlFormat);
                if (!string.IsNullOrEmpty(xmlString))
                {
                    data = new MathEditorData { XmlString = xmlString };
                    return true;
                }
            }
            else if (handle.ContainsText())
            {
                var textString = handle.GetText();
                if (!string.IsNullOrEmpty(textString))
                {
                    data = textString;
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            EditorLogger.Error(ClassName, "Failed to check clipboard data", e);
        }
        return false;
    }

    public async void SetClipboard(XElement element, Bitmap? image, string? text)
    {
        await _updatingLock.WaitAsync();
        try
        {
            var xmlString = element.ToString();

            // Update paste object directly
            PasteObject = new MathEditorData
            {
                XmlString = xmlString
            };
            CanPaste = true;

            // Try to set clipboard contents
            using var handle = ClipboardAvalonia.Open();
            handle.SetFormat(ClipboardXmlFormat, xmlString);
            if (image != null)
            {
                handle.SetImage(image);
            }
            if (text != null)
            {
                handle.SetText(text);
            }
        }
        catch (Exception ex)
        {
            EditorLogger.Error(ClassName, "Error setting clipboard contents", ex);
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
            _timer.Elapsed -= Timer_Elapsed;
            _timer.Dispose();
            _isDisposed = true;
        }
    }

    #endregion
}
