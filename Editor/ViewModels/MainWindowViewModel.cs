using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Editor;

public partial class MainWindowViewModel(Settings settings) : ObservableObject
{
    public Settings Settings { get; init; } = settings;
    public MainWindow MainWindow = null!;
    public EditorControl? Editor { get; set; } = null;

    // TODO: Update the localization when languages changes
    public List<EditorModeLocalized> AllEditModes { get; } = EditorModeLocalized.GetValues();

    [ObservableProperty]
    private EditorMode _textEditorMode = settings.DefaultMode;

    public List<FontTypeLocalized> AllFontTypes { get; } = FontTypeLocalized.GetValues();

    [ObservableProperty]
    private FontType _textFontType = settings.DefaultFont;

    [ObservableProperty]
    private bool _inputBold;

    [ObservableProperty]
    private bool _inputItalic;

    [ObservableProperty]
    private bool _inputUnderline;

    public bool IgnoreTextEditorModeChange { get; set; } = false;
    public bool IgnoreTextFontTypeChange { get; set; } = false;
    public bool IgnoreInputBoldChange { get; set; } = false;
    public bool IgnoreInputItalicChange { get; set; } = false;
    public bool IgnoreInputUnderlineChange { get; set; } = false;

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

    private void ChangeInputBold(bool isBold)
    {
        if (Editor != null)
        {
            Editor.ChangeFormat(nameof(Format), Format.Bold, isBold);
            Editor.Focus();
        }
    }

    private void ChangeInputItalic(bool isItalic)
    {
        if (Editor != null)
        {
            Editor.ChangeFormat(nameof(Format), Format.Italic, isItalic);
            Editor.Focus();
        }
    }

    private void ChangeInputUnderline(bool isUnderline)
    {
        if (Editor != null)
        {
            Editor.ChangeFormat(nameof(Format), Format.Underline, isUnderline);
            Editor.Focus();
        }
    }
}
