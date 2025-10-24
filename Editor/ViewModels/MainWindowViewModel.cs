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

    public bool InputBold
    {
        get => TextEquation.InputBold;
        set
        {
            TextEquation.InputPropertyChanged -= TextEquation_InputPropertyChanged;
            TextEquation.InputBold = value;
            Editor?.ChangeFormat(nameof(Format), Format.Bold, value);
            TextEquation.InputPropertyChanged += TextEquation_InputPropertyChanged;
        }
    }

    public bool InputItalic
    {
        get => TextEquation.InputItalic;
        set
        {
            TextEquation.InputPropertyChanged -= TextEquation_InputPropertyChanged;
            TextEquation.InputItalic = value;
            Editor?.ChangeFormat(nameof(Format), Format.Italic, value);
            TextEquation.InputPropertyChanged += TextEquation_InputPropertyChanged;
        }
    }

    public bool InputUnderline
    {
        get => TextEquation.InputUnderline;
        set
        {
            TextEquation.InputPropertyChanged -= TextEquation_InputPropertyChanged;
            TextEquation.InputUnderline = value;
            Editor?.ChangeFormat(nameof(Format), Format.Underline, value);
            TextEquation.InputPropertyChanged += TextEquation_InputPropertyChanged;
        }
    }

    public void TextEquation_InputPropertyChanged(object? sender, string e)
    {
        switch (e)
        {
            case nameof(EditorMode):
                TextEditorMode = TextEquation.EditorMode;
                break;
            case nameof(FontType):
                TextFontType = TextEquation.FontType;
                break;
        }
    }

    partial void OnTextEditorModeChanged(EditorMode value)
    {
        ChangeEditorMode(value);
    }

    public void ChangeEditorMode(EditorMode editorMode)
    {
        if (Editor != null)
        {
            TextEquation.InputPropertyChanged -= TextEquation_InputPropertyChanged;
            TextEquation.EditorMode = editorMode;
            Editor.ChangeFormat(nameof(EditorMode), editorMode, true);
            TextEquation.InputPropertyChanged += TextEquation_InputPropertyChanged;
            Editor.Focus();
        }
    }

    partial void OnTextFontTypeChanged(FontType value)
    {
        ChangeEditorFont(value);
    }

    public void ChangeEditorFont(FontType fontType)
    {
        if (Editor != null)
        {
            TextEquation.InputPropertyChanged -= TextEquation_InputPropertyChanged;
            TextEquation.FontType = fontType;
            Editor.ChangeFormat(nameof(FontType), fontType, true);
            TextEquation.InputPropertyChanged += TextEquation_InputPropertyChanged;
            Editor.Focus();
        }
    }
}
