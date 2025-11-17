using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Clowd.Clipboard;
using Clowd.Clipboard.Formats;
using CommunityToolkit.Mvvm.ComponentModel;
using Timer = System.Timers.Timer;

namespace Editor;

public class ClipboardHelper : ObservableObject, IDisposable
{
    private static readonly string ClassName = nameof(ClipboardHelper);

    private static readonly string ClipboardXmlFormatType =
        $"{typeof(MathEditorData).FullName}.{nameof(MathEditorData.XmlString)}";

    private static ClipboardFormat<string> ClipboardXmlFormat
    {
        get => field ??= ClipboardFormat.CreateCustomFormat(ClipboardXmlFormatType, new TextUtf8Converter());
    }

    private static DataFormat<string> ClipboardXmlFormatA
    {
        get => field ??= DataFormat.CreateStringPlatformFormat(ClipboardXmlFormatType);
    }

    private readonly Timer _timer = new(1000);

    public bool CanPaste
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged(nameof(CanPaste));
            }
        }
    }

    public object? PasteObject { get; private set; }

    private TopLevel _topLevel = null!;
    private bool _isMonitoring;
    private readonly SemaphoreSlim _updatingLock = new(1, 1);

    public void StartMonitoring(TopLevel topLevel)
    {
        if (_isMonitoring) return;
        if (Design.IsDesignMode) return;

        _topLevel = topLevel ?? throw new ArgumentNullException(nameof(topLevel));
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
            PasteObject = await CanPasteFromClipboard();
            CanPaste = PasteObject != null;
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

    private async Task<object?> CanPasteFromClipboard()
    {
        object? data = null;
        try
        {
            // Windows: ClipboardAvalonia
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var handle = await ClipboardAvalonia.OpenAsync();
                if (handle == null)
                {
                    data = null;
                }
                else
                {
                    if (handle.ContainsFormat(ClipboardXmlFormat))
                    {
                        var xmlString = handle.GetFormatType(ClipboardXmlFormat);
                        if (!string.IsNullOrEmpty(xmlString))
                        {
                            data = new MathEditorData { XmlString = xmlString };
                        }
                    }
                    else
                    {
                        var textString = handle.GetText();
                        if (!string.IsNullOrEmpty(textString))
                        {
                            data = textString;
                        }
                    }
                }
            }
            // Others: Fallback to Avalonia IClipboard
            else
            {
                ArgumentNullException.ThrowIfNull(_topLevel.Clipboard, nameof(_topLevel.Clipboard));

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    try
                    {
                        using var transfer = await _topLevel.Clipboard.TryGetDataAsync();
                        if (transfer == null)
                        {
                            data = null;
                        }
                        else
                        {
                            if (transfer.Contains(ClipboardXmlFormatA))
                            {
                                var xmlString = await transfer.TryGetValueAsync(ClipboardXmlFormatA);
                                if (!string.IsNullOrEmpty(xmlString))
                                {
                                    data = new MathEditorData { XmlString = xmlString };
                                }
                            }
                            else
                            {
                                var textString = await _topLevel.Clipboard.TryGetTextAsync();
                                if (!string.IsNullOrEmpty(textString))
                                {
                                    data = textString;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        EditorLogger.Error(ClassName, "Failed to check clipboard data", e);
                    }
                });
            }
        }
        catch (Exception e)
        {
            EditorLogger.Error(ClassName, "Failed to check clipboard data", e);
        }
        return data;
    }

    public async void SetClipboard(XElement element, Bitmap? image, string? text)
    {
        await _updatingLock.WaitAsync();
        try
        {
            var xmlString = element.ToString();

            // Update paste object directly
            PasteObject = new MathEditorData { XmlString = xmlString };
            CanPaste = true;

            // Try to set clipboard contents
            // Windows: ClipboardAvalonia
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var handle = await ClipboardAvalonia.OpenAsync();
                if (image != null)
                {
                    handle.SetImage(image);
                }
                if (text != null)
                {
                    handle.SetText(text);
                }
                handle.SetFormat(ClipboardXmlFormat, xmlString);
            }
            // Others: Fallback to Avalonia IClipboard
            else
            {
                ArgumentNullException.ThrowIfNull(_topLevel.Clipboard, nameof(_topLevel.Clipboard));

                using var transfer = new DataTransfer();
                var transferItem = new DataTransferItem();
                // Image clipboard support is currently unavailable in offical Avalonia APIs
                // because Avalonia's IClipboard/DataTransferItem does not support image data.
                // Uncomment and implement when/if cross-platform image clipboard support is added.
                /*if (image != null)
                {
                    transferItem.SetImage(image);
                }*/
                if (text != null)
                {
                    transferItem.SetText(text);
                }
                transferItem.Set(ClipboardXmlFormatA, xmlString);
                transfer.Add(transferItem);
                await _topLevel.Clipboard.SetDataAsync(transfer);
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
