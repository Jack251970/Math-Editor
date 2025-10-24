using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Editor;

public partial class MainWindowViewModel(Settings settings) : ObservableObject
{
    public Settings Settings { get; init; } = settings;
    public EditorControl? Editor { get; set; } = null;

    // TODO: Update the localization when languages changes
    public List<EditorModeLocalized> AllEditModes { get; } = EditorModeLocalized.GetValues();

    [ObservableProperty]
    private EditorMode _textEditorMode = settings.DefaultMode;

    public List<FontTypeLocalized> AllFontTypes { get; } = FontTypeLocalized.GetValues();

    [ObservableProperty]
    private FontType _textFontType = settings.DefaultFont;

    [RelayCommand]
    private void OpenSettingsWindow(MenuItem item)
    {
        SingletonWindowOpener.Open<SettingsWindow>(Window.GetWindow(item));
    }

    private UnicodeSelectorWindow? _unicodeSelectorWindow = null;
    [RelayCommand]
    private void OpenUnicodeSelectorWindow(MenuItem item)
    {
        _unicodeSelectorWindow ??= new UnicodeSelectorWindow
        {
            Owner = Window.GetWindow(item)
        };
        _unicodeSelectorWindow.Show();
    }

    [RelayCommand]
    private void OpenCodepointWindow(MenuItem item)
    {
        var codepointWindow = new CodepointWindow
        {
            Owner = Window.GetWindow(item)
        };
        codepointWindow.Show();
    }

    [RelayCommand]
    private void OpenCustomZoomWindow(MenuItem item)
    {
        var customZoomWindow = new CustomZoomWindow
        {
            Owner = Window.GetWindow(item)
        };
        customZoomWindow.Show();
    }

    [RelayCommand]
    private void OpenAboutWindow(MenuItem item)
    {
        SingletonWindowOpener.Open<AboutWindow>(Window.GetWindow(item));
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
