using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using iNKORE.UI.WPF.Modern.Controls;
using ElapsedEventArgs = System.Timers.ElapsedEventArgs;
using Timer = System.Timers.Timer;

namespace Editor;

public partial class EditorControl : UserControl, IDisposable
{
    private static readonly string ClassName = nameof(EditorControl);

    private readonly Timer timer;
    private const int BlinkPeriod = 600;
    private readonly MainWindow _mainWindow;

    public event EventHandler<int>? ZoomChanged;

    public bool Dirty { get; set; } = false;

    private EquationRoot equationRoot;
    private readonly Caret vCaret = new(false);
    private readonly Caret hCaret = new(true);

    public const double RootFontSize = 40;

    public EditorControl(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        InitializeComponent();
        MainGrid.Children.Add(vCaret);
        MainGrid.Children.Add(hCaret);
        equationRoot = new EquationRoot(mainWindow, vCaret, hCaret)
        {
            FontSize = RootFontSize
        };
        timer = new Timer(BlinkPeriod);
        timer.Elapsed += Timer_Elapsed;
    }

    public void SetTimer(bool enabled)
    {
        if (enabled)
        {
            vCaret.ForceVisible(true);
            hCaret.ForceVisible(true);
            timer.Start();
        }
        else
        {
            timer.Stop();
            vCaret.ForceVisible(false);
            hCaret.ForceVisible(false);
        }
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        vCaret.ToggleVisibility();
        hCaret.ToggleVisibility();
    }

    private void EditorControl_Loaded(object sender, RoutedEventArgs e)
    {
        timer.Start();
        // Here we set Editor later so that equationRoot will not call the methods related to timer
        // which can cause null exception
        equationRoot.Editor = this;
    }

    public void SetFontSizePercentage(int percentage)
    {
        equationRoot.FontSize = RootFontSize * percentage / 100;
        AdjustView();
    }

    public void ShowUnderbar(bool show)
    {
        if (!show)
        {
            hCaret.Visibility = Visibility.Hidden;
        }
        else
        {
            hCaret.Visibility = Visibility.Visible;
        }
    }

    public void HandleUserCommand(CommandDetails commandDetails)
    {
        equationRoot.HandleUserCommand(commandDetails);
        AdjustView();
        Dirty = true;
    }

    public async Task SaveFileAsync(Stream stream, string fileName)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            equationRoot.SaveFile(memoryStream);
            memoryStream.Position = 0;
            ZipStream(memoryStream, stream, Path.GetFileNameWithoutExtension(fileName) + ".xml");
        }
        catch (Exception e)
        {
            EditorLogger.Fatal(ClassName, "Failed to save file", e);
            await ContentDialogHelper.ShowAsync(_mainWindow, Localize.EditorControl_CannotSaveFile(), Localize.Error(),
                MessageBoxButton.OK);
        }
        Dirty = false;
    }

    private static void ZipStream(MemoryStream memStreamIn, Stream outputStream, string zipEntryName)
    {
        var zipStream = new ZipOutputStream(outputStream);
        zipStream.SetLevel(5); //0-9, 9 being the highest level of compression
        var newEntry = new ZipEntry(zipEntryName)
        {
            DateTime = DateTime.Now
        };
        zipStream.PutNextEntry(newEntry);
        StreamUtils.Copy(memStreamIn, zipStream, new byte[4096]);
        zipStream.CloseEntry();
        zipStream.IsStreamOwner = false;	// False stops the Close also Closing the underlying stream.
        zipStream.Close();			// Must finish the ZipOutputStream before using outputMemStream.            
    }

    public async Task LoadFileAsync(Stream stream)
    {
        try
        {
            var zipInputStream = new ZipInputStream(stream);
            var zipEntry = zipInputStream.GetNextEntry();
            var outputStream = new MemoryStream();
            if (zipEntry != null)
            {
                var buffer = new byte[4096];
                StreamUtils.Copy(zipInputStream, outputStream, buffer);
            }
            outputStream.Position = 0;
            using (outputStream)
            {
                await equationRoot.LoadFileAsync(outputStream);
            }
        }
        catch (Exception e)
        {
            EditorLogger.Fatal(ClassName, "Failed to load file from zip stream", e);
            try
            {
                stream.Position = 0;
                await equationRoot.LoadFileAsync(stream);
            }
            catch (Exception e1)
            {
                EditorLogger.Fatal(ClassName, "Failed to load file from stream", e1);
                await ContentDialogHelper.ShowAsync(_mainWindow, Localize.EditorControl_CannotOpenFile(), Localize.Error(),
                    MessageBoxButton.OK);
            }
        }
        AdjustView();
        Dirty = false;
    }

    private bool isDragging = false;
    private bool isDoubleClicked = false;

    private void EditorControl_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var mousePosition = e.GetPosition(this);
        if (equationRoot.ConsumeMouseClick(Mouse.GetPosition(this)))
        {
            InvalidateVisual();
        }
        Focus();
        ForceCaretVisible(false); // When we click, we want to see the caret immediately
        lastMouseLocation = mousePosition;
        isDragging = true;
        if (isDoubleClicked)
        {
            isDoubleClicked = false;
            equationRoot.HandleMouseDoubleClick(mousePosition);
            InvalidateVisual();
        }
    }

    private void EditorControl_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        isDoubleClicked = true;
    }

    private void EditorControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        Focus();

        // Build context menu
        var menu = new ContextMenu();

        var hasSelection = _mainWindow.ViewModel.IsSelecting == true;
        var canPaste = _mainWindow.ViewModel.ClipboardHelper.CanPaste;

        void AddMenuItem(string header, string glyph, bool isEnabled, Action action)
        {
            var mi = new MenuItem
            {
                Header = header,
                Icon = new FontIcon()
                {
                    Glyph = glyph
                },
                IsEnabled = isEnabled
            };
            mi.Click += (s, e) => action();
            menu.Items.Add(mi);
        }

        void AddSeparator()
        {
            menu.Items.Add(new Separator());
        }

        AddMenuItem(Localize.MainWindow_Cut(), "\uE8C6", hasSelection, Cut);

        AddMenuItem(Localize.MainWindow_Copy(), "\uE8C8", hasSelection, Copy);

        AddMenuItem(Localize.MainWindow_Paste(), "\uE77F", canPaste, Paste);

        AddMenuItem(Localize.MainWindow_Delete(), "\uE74D", hasSelection, DeleteSelection);

        // The menu is too long so I remove these two actions here
        /*AddSeparator();

        AddMenuItem(Localize.MainWindow_Undo(), "\uE7A7", _mainWindow.ViewModel.UndoManager.CanUndo, Undo);

        AddMenuItem(Localize.MainWindow_Redo(), "\uE7A6", _mainWindow.ViewModel.UndoManager.CanRedo, Redo);*/

        // It looks like Clear action cannot be added into UndoManager,
        // So we do not add it here
        /*AddSeparator();

        AddMenuItem(Localize.MainWindow_Clear(), "\uE894", true, Clear);*/

        AddSeparator();

        AddMenuItem(Localize.MainWindow_SelectAll(), "\uE8B3", true, SelectAll);

        // Show at mouse position
        var pos = e.GetPosition(this);
        menu.PlacementTarget = this;
        menu.Placement = PlacementMode.RelativePoint;
        menu.HorizontalOffset = pos.X;
        menu.VerticalOffset = pos.Y;
        menu.IsOpen = true;

        e.Handled = true;
    }

    private bool isForceVisible = false;

    public void ForceCaretVisible(bool render)
    {
        if (isForceVisible && !render) return;
        isForceVisible = true;
        // Force the caret visible and then reset the timer
        timer.Stop();
        if (render)
        {
            vCaret.ForceVisible(false);
            hCaret.ForceVisible(false);
        }
        vCaret.ForceVisible(true);
        hCaret.ForceVisible(true);
        timer.Start();
        isForceVisible = false;
    }

    private void EditorControl_MouseUp(object sender, MouseButtonEventArgs e)
    {
        isDragging = false;
    }

    private void EditorControl_MouseEnter(object sender, MouseEventArgs e)
    {
        isDragging = false;
    }

    private void EditorControl_MouseLeave(object sender, MouseEventArgs e)
    {
        _mainWindow.ViewModel.StatusBarRightMessage = string.Empty;
    }

    private Point lastMouseLocation = new();

    private void EditorControl_MouseMove(object sender, MouseEventArgs e)
    {
        var mousePosition = e.GetPosition(this);
        _mainWindow.ViewModel.StatusBarRightMessage = (int)mousePosition.X + ", " + (int)mousePosition.Y;
        if (isDragging)
        {
            if (Math.Abs(lastMouseLocation.X - mousePosition.X) > 2 /*SystemParameters.MinimumHorizontalDragDistance*/ ||
                Math.Abs(lastMouseLocation.Y - mousePosition.Y) > 2 /*SystemParameters.MinimumVerticalDragDistance*/ )
            {
                equationRoot.HandleMouseDrag(mousePosition);
                lastMouseLocation = mousePosition;
                InvalidateVisual();
            }
        }
    }

    public void DeleteSelection()
    {
        equationRoot.RemoveSelection(true);
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        var scrollViewer = Parent as ScrollViewer;
        equationRoot.DrawVisibleRows(drawingContext, scrollViewer!.VerticalOffset, scrollViewer.ViewportHeight + scrollViewer.VerticalOffset, false);
    }

    public void EditorControl_TextInput(object sender, TextCompositionEventArgs e)
    {
        ConsumeText(e.Text.Replace('-', '\u2212'));
    }

    public void ConsumeText(string text)
    {
        equationRoot.ConsumeText(text);
        AdjustView();
        Dirty = true;
    }

    private void EditorControl_KeyDown(object sender, KeyEventArgs e)
    {
        var handled = false;
        if (e.Key == Key.Tab)
        {
            equationRoot.ConsumeText("    ");
            handled = true;
        }
        else if (equationRoot.ConsumeKey(e.Key))
        {
            handled = true;
        }
        if (handled)
        {
            e.Handled = true;
            AdjustView();
            Dirty = true;
        }
    }

    private void AdjustView()
    {
        DetermineSize();
        AdjustScrollViewer();
        InvalidateVisual();
    }

    private void DetermineSize()
    {
        MinWidth = equationRoot.Width + 50;
        MinHeight = equationRoot.Height + 20;
    }

    private void AdjustScrollViewer()
    {
        if (Parent is ScrollViewer scrollViewer)
        {
            var left = scrollViewer.HorizontalOffset;
            var top = scrollViewer.VerticalOffset;
            var right = scrollViewer.ViewportWidth + scrollViewer.HorizontalOffset;
            var bottom = scrollViewer.ViewportHeight + scrollViewer.VerticalOffset;
            double hOffset = 0;
            double vOffset = 0;
            var rightDone = false;
            var bottomDone = false;
            while (vCaret.Left > right - 8)
            {
                hOffset += 8;
                right += 8;
                rightDone = true;
            }
            while (vCaret.VerticalCaretBottom > bottom - 10)
            {
                vOffset += 10;
                bottom += 10;
                bottomDone = true;
            }
            while (vCaret.Left < left + 8 && !rightDone)
            {
                hOffset -= 8;
                left -= 8;
            }
            while (vCaret.Top < top + 10 && !bottomDone)
            {
                vOffset -= 10;
                top -= 10;
            }
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + hOffset);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + vOffset);
        }
    }

    public void Undo()
    {
        _mainWindow.ViewModel.UndoManager.Undo();
        AdjustView();
        Dirty = true;
        equationRoot.AdjustCarets();
    }

    public void Redo()
    {
        _mainWindow.ViewModel.UndoManager.Redo();
        AdjustView();
        Dirty = true;
        equationRoot.AdjustCarets();
    }

    public async Task ExportImageAsync(string filePath)
    {
        await equationRoot.SaveImageToFileAsync(filePath);
    }

    public async Task PrintAsync(PrintDialog printDialog)
    {
        await equationRoot.PrintAsync(printDialog);
    }

    public void ZoomOut()
    {
        equationRoot.ZoomOut(4);
        var percentage = (int)(equationRoot.FontSize * 100 / RootFontSize);
        ZoomChanged?.Invoke(this, percentage);
        AdjustView();
    }

    public void ZoomIn()
    {
        equationRoot.ZoomIn(4);
        var percentage = (int)(equationRoot.FontSize * 100 / RootFontSize);
        ZoomChanged?.Invoke(this, percentage);
        AdjustView();
    }

    private void ZoomOutHandler(object sender, ExecutedRoutedEventArgs e)
    {
        ZoomOut();
    }

    private void ZoomInHandler(object sender, ExecutedRoutedEventArgs e)
    {
        ZoomIn();
    }

    public void Cut()
    {
        Copy(true);
    }

    public void Copy()
    {
        Copy(false);
    }

    private void Copy(bool cut)
    {
        equationRoot.Copy(cut);
        if (cut)
        {
            AdjustView();
        }
    }

    public void Paste()
    {
        if (equationRoot.PasteFromClipBoard())
        {
            AdjustView();
            Dirty = true;
        }
    }

    public void SelectAll()
    {
        equationRoot.SelectAll();
        InvalidateVisual();
    }

    public void ChangeFont(FontType fontType)
    {
        equationRoot.ChangeFont(fontType);
        InvalidateVisual();
    }

    public void ChangeFormat(string operation, object argument, bool applied)
    {
        equationRoot.ModifySelection(operation, argument, applied, true);
        AdjustView();
        if (_mainWindow.IsEditorLoaded)
        {
            Dirty = true;
        }
    }

    public void Clear()
    {
        equationRoot = new EquationRoot(_mainWindow, vCaret, hCaret)
        {
            FontSize = RootFontSize
        };
        Dirty = false;
        AdjustView();
    }

    #region IDisposable

    private bool _isDisposed = false;

    ~EditorControl()
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
            vCaret.Dispose();
            hCaret.Dispose();
            timer.Dispose();
            _isDisposed = true;
        }
    }

    #endregion

    private void EditorControl_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_mainWindow.ContentDialogShown)
        {
            e.Handled = true;
        }
    }

    private void EditorControl_PreviewKeyUp(object sender, KeyEventArgs e)
    {
        if (_mainWindow.ContentDialogShown)
        {
            e.Handled = true;
        }
    }
}
