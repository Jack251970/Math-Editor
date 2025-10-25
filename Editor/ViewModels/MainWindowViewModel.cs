using System.Collections.Generic;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Editor;

public partial class MainWindowViewModel(Settings settings, UndoManager undoManager) : ObservableObject, ICultureInfoChanged
{
    public Settings Settings { get; init; } = settings;
    public UndoManager UndoManager { get; init; } = undoManager;
    public MainWindow MainWindow = null!;
    public EditorControl? Editor { get; set; } = null;

    public List<EditorModeLocalized> AllEditModes { get; } = EditorModeLocalized.GetValues();

    [ObservableProperty]
    private EditorMode _textEditorMode = settings.DefaultMode;

    public List<FontTypeLocalized> AllFontTypes { get; } = FontTypeLocalized.GetValues();

    [ObservableProperty]
    private FontType _textFontType = settings.DefaultFont;

    [ObservableProperty]
    private string _showNestingMenuItemHeader = settings.ShowNesting ?
        Localize.MainWindow_HideNesting() : Localize.MainWindow_ShowNesting();

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

    public bool IgnoreTextEditorModeChange { get; set; } = false;
    public bool IgnoreTextFontTypeChange { get; set; } = false;
    public bool IgnoreInputBoldChange { get; set; } = false;
    public bool IgnoreInputItalicChange { get; set; } = false;
    public bool IgnoreInputUnderlineChange { get; set; } = false;

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
    private void NewWindow()
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

    public void OnCultureInfoChanged(CultureInfo newCultureInfo)
    {
        EditorModeLocalized.UpdateLabels(AllEditModes);
        FontTypeLocalized.UpdateLabels(AllFontTypes);
        UpdateShowNestingMenuItemHeader();
    }
}
