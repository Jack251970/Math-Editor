using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Editor;

public partial class MainWindowViewModel : ObservableObject, ICultureInfoChanged
{
    public Settings Settings { get; init; }
    public UndoManager UndoManager { get; init; }
    public MainWindow MainWindow { get; set; } = null!;
    public EditorControl? Editor { get; set; } = null;

    [ObservableProperty]
    private string _mainWindowTitle = null!;

    [ObservableProperty]
    private string _currentLocalFile = null!;

    [ObservableProperty]
    private string _showNestingMenuItemHeader = null!;

    public MainWindowViewModel(Settings settings, UndoManager undoManager)
    {
        Settings = settings;
        UndoManager = undoManager;
        TextEditorMode = settings.DefaultMode;
        TextFontType = settings.DefaultFont;
        UpdateMainWindowTitle();
        UpdateShowNestingMenuItemHeader();
        UpdateFullScreenMenuItemHeader();
        UpdateCustomZoomPercentage();
        UpdateStatusBarLeftMessage();
    }

    public List<EditorModeLocalized> AllEditModes { get; } = EditorModeLocalized.GetValues();

    [ObservableProperty]
    private EditorMode _textEditorMode;

    public List<FontTypeLocalized> AllFontTypes { get; } = FontTypeLocalized.GetValues();

    [ObservableProperty]
    private FontType _textFontType;

    [ObservableProperty]
    private bool _inputBold;

    [ObservableProperty]
    private bool _inputItalic;

    [ObservableProperty]
    private bool _inputUnderline;

    [ObservableProperty]
    private bool _isSelecting;

    [ObservableProperty]
    private bool _useItalicIntergalOnNew;

    [ObservableProperty]
    private bool _undoButtonIsEnabled = false;

    [ObservableProperty]
    private bool _redoButtonIsEnabled = false;

    [ObservableProperty]
    private bool _showUnderbar = true;

    [ObservableProperty]
    private string _statusBarLeftMessage = string.Empty;

    [ObservableProperty]
    private string _statusBarRightMessage = string.Empty;

    [ObservableProperty]
    private int _activeChildSelectionStartIndex = 0;

    [ObservableProperty]
    private int _activeChildSelectedItems = 0;

    [ObservableProperty]
    private bool _fullScreenMode = false;

    [ObservableProperty]
    private string _fullScreenMenuItemHeader = null!;

    [ObservableProperty]
    private Visibility _fullScreenButtonVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private int _customZoomPercentage = 0;

    [ObservableProperty]
    private bool _customZoomMenuChecked = false;

    [ObservableProperty]
    private string _customZoomMenuHeader = null!;

    public bool IgnoreTextEditorModeChange { get; set; } = false;
    public bool IgnoreTextFontTypeChange { get; set; } = false;
    public bool IgnoreInputBoldChange { get; set; } = false;
    public bool IgnoreInputItalicChange { get; set; } = false;
    public bool IgnoreInputUnderlineChange { get; set; } = false;

    public bool IgnoreEditorZoomPercentageChange { get; set; } = false;

    partial void OnCurrentLocalFileChanged(string value)
    {
        UpdateMainWindowTitle();
    }

    partial void OnTextEditorModeChanged(EditorMode value)
    {
        if (IgnoreTextEditorModeChange) return;
        ChangeEditorMode(value);
    }

    partial void OnTextFontTypeChanged(FontType value)
    {
        if (IgnoreTextFontTypeChange) return;
        ChangeEditorFont(value);
    }

    partial void OnInputBoldChanged(bool value)
    {
        if (IgnoreInputBoldChange) return;
        ChangeInputBold(value);
    }

    partial void OnInputItalicChanged(bool value)
    {
        if (IgnoreInputItalicChange) return;
        ChangeInputItalic(value);
    }

    partial void OnInputUnderlineChanged(bool value)
    {
        if (IgnoreInputUnderlineChange) return;
        ChangeInputUnderline(value);
    }

    public void ChangeEditorMode(EditorMode editorMode)
    {
        if (Editor != null)
        {
            Editor.ChangeFormat(nameof(EditorMode), editorMode, true);
            Editor.Focus();
        }
    }

    public void ChangeEditorFont(FontType fontType)
    {
        if (Editor != null)
        {
            Editor.ChangeFormat(nameof(FontType), fontType, true);
            Editor.Focus();
        }
    }

    public void ChangeInputBold(bool isBold)
    {
        if (Editor != null)
        {
            Editor.ChangeFormat(nameof(Format), Format.Bold, isBold);
            Editor.Focus();
        }
    }

    public void ChangeInputItalic(bool isItalic)
    {
        if (Editor != null)
        {
            Editor.ChangeFormat(nameof(Format), Format.Italic, isItalic);
            Editor.Focus();
        }
    }

    public void ChangeInputUnderline(bool isUnderline)
    {
        if (Editor != null)
        {
            Editor.ChangeFormat(nameof(Format), Format.Underline, isUnderline);
            Editor.Focus();
        }
    }

    partial void OnShowUnderbarChanged(bool value)
    {
        Editor?.ShowUnderbar(value);
    }

    private void UpdateMainWindowTitle()
    {
        var currentLocalFileName = TryGetFileName(CurrentLocalFile);
        if (!string.IsNullOrEmpty(currentLocalFileName))
        {
#if DEBUG
            MainWindowTitle = $"{Constants.MathEditorFullName} v{Constants.Version} ({Constants.Dev}) - {currentLocalFileName}";
#else
            MainWindowTitle = $"{Constants.MathEditorFullName} v{Constants.Version} - {currentLocalFileName}";
#endif
        }
        else
        {
#if DEBUG
            MainWindowTitle = $"{Constants.MathEditorFullName} v{Constants.Version} ({Constants.Dev}) - {Localize.MainWindow_Untitled()}";
#else
            MainWindowTitle = $"{Constants.MathEditorFullName} v{Constants.Version} - {Localize.MainWindow_Untitled()}";
#endif
        }
    }

    private static string TryGetFileName(string filePath)
    {
        var currentLocalFileName = string.Empty;
        try
        {
            currentLocalFileName = Path.GetFileName(filePath);
        }
        catch
        {
            // Ignore
        }
        return currentLocalFileName;
    }

    partial void OnCustomZoomPercentageChanged(int value)
    {
        if (!IgnoreEditorZoomPercentageChange && value != 0)
        {
            Editor?.SetFontSizePercentage(value);
        }
        UpdateCustomZoomPercentage();
    }

    private void UpdateCustomZoomPercentage()
    {
        if (CustomZoomPercentage == 0)
        {
            CustomZoomMenuChecked = false;
            CustomZoomMenuHeader = Localize.MainWindow_Custom();
        }
        else
        {
            CustomZoomMenuChecked = true;
            CustomZoomMenuHeader = Localize.MainWindow_CustomPercentage(CustomZoomPercentage);
            if (_lastZoomPercentageItem != null)
            {
                _lastZoomPercentageItem.IsChecked = false;
                _lastZoomPercentageItem = null;
            }
        }
    }

    private MenuItem? _lastZoomPercentageItem;

    [RelayCommand]
    private void ChangeZoomPercentage(MenuItem item)
    {
        if (item.Tag is string percentage &&
            !string.IsNullOrEmpty(percentage) &&
            int.TryParse(percentage, out var zoomPercentage))
        {
            Editor?.SetFontSizePercentage(zoomPercentage);
            SetLastZoomPercentageItem(item);
            // Reset the custom zoom percentage
            IgnoreEditorZoomPercentageChange = true;
            CustomZoomPercentage = 0;
            IgnoreEditorZoomPercentageChange = false;
        }
    }

    public void SetLastZoomPercentageItem(MenuItem item)
    {
        // Uncheck the last selected zoom percentage item
        if (_lastZoomPercentageItem != null && item != _lastZoomPercentageItem)
        {
            _lastZoomPercentageItem.IsChecked = false;
        }
        // Check the current selected zoom percentage item
        item.IsChecked = true;
        _lastZoomPercentageItem = item;
    }

    partial void OnActiveChildSelectionStartIndexChanged(int value)
    {
        UpdateStatusBarLeftMessage();
    }

    partial void OnActiveChildSelectedItemsChanged(int value)
    {
        UpdateStatusBarLeftMessage();
    }

    private void UpdateStatusBarLeftMessage()
    {
        StatusBarLeftMessage = Localize.MainWindow_StatusBarLeftMessage(ActiveChildSelectionStartIndex,
            ActiveChildSelectedItems);
    }

    [RelayCommand]
    private void ToggleShowNesting()
    {
        Settings.ShowNesting = !Settings.ShowNesting;
        UpdateShowNestingMenuItemHeader();
        Editor?.InvalidateVisual();
    }

    private void UpdateShowNestingMenuItemHeader()
    {
        ShowNestingMenuItemHeader = Settings.ShowNesting ?
            Localize.MainWindow_HideNesting() : Localize.MainWindow_ShowNesting();
    }

    [RelayCommand]
    private void ToggleFullScreenMode()
    {
        FullScreenMode = !FullScreenMode;
    }

    partial void OnFullScreenModeChanged(bool value)
    {
        if (value)
        {
            MainWindow.WindowStyle = WindowStyle.None;
            MainWindow.WindowState = WindowState.Normal;
            MainWindow.WindowState = WindowState.Maximized;
            FullScreenButtonVisibility = Visibility.Visible;
        }
        else
        {
            MainWindow.WindowStyle = WindowStyle.ThreeDBorderWindow;
            MainWindow.WindowState = WindowState.Normal;
            FullScreenButtonVisibility = Visibility.Collapsed;
        }
        UpdateFullScreenMenuItemHeader();
    }

    private void UpdateFullScreenMenuItemHeader()
    {
        FullScreenMenuItemHeader = FullScreenMode ? Localize.MainWindow_NormalScreen() : Localize.MainWindow_FullScreen();
    }

    [RelayCommand]
    private void OpenSettingsWindow()
    {
        WindowOpener.OpenSingle<SettingsWindow>(MainWindow);
    }

    [RelayCommand]
    private void OpenUnicodeSelectorWindow()
    {
        WindowOpener.OpenScoped<UnicodeSelectorWindow>(MainWindow);
    }

    [RelayCommand]
    private void OpenCodepointWindow()
    {
        WindowOpener.OpenScoped<CodepointWindow>(MainWindow);
    }

    [RelayCommand]
    private void OpenCustomZoomWindow()
    {
        WindowOpener.OpenScoped<CustomZoomWindow>(MainWindow);
    }

    [RelayCommand]
    private void OpenAboutWindow()
    {
        WindowOpener.OpenSingle<AboutWindow>(MainWindow);
    }

    [RelayCommand]
    private void OpenContents()
    {
        BrowserHelper.Open(Constants.WikiUrl);
    }

    [RelayCommand]
    private void New()
    {
        var mainWindow = new MainWindow(string.Empty);
        mainWindow.Show();
    }

    [RelayCommand]
    private void Cut()
    {
        Editor?.Copy(true);
    }

    [RelayCommand]
    private void Copy()
    {
        Editor?.Copy(false);
    }

    [RelayCommand]
    private void Paste()
    {
        Editor?.Paste();
    }

    [RelayCommand]
    private void Print()
    {
        // TODO: Add support for print
    }

    [RelayCommand]
    private void SelectAll()
    {
        Editor?.SelectAll();
    }

    [RelayCommand]
    private void Undo()
    {
        Editor?.Undo();
    }

    [RelayCommand]
    private void Redo()
    {
        Editor?.Redo();
    }

    [RelayCommand]
    private void Delete()
    {
        Editor?.DeleteSelection();
    }

    [RelayCommand]
    private void Exit()
    {
        WindowTracker.GetOwnerWindows().ForEach(window => window.Close());
    }

    public void OnCultureInfoChanged(CultureInfo newCultureInfo)
    {
        UpdateMainWindowTitle();
        UpdateShowNestingMenuItemHeader();
        UpdateFullScreenMenuItemHeader();
        UpdateCustomZoomPercentage();
        UpdateStatusBarLeftMessage();
        EditorModeLocalized.UpdateLabels(AllEditModes);
        FontTypeLocalized.UpdateLabels(AllFontTypes);
    }
}
