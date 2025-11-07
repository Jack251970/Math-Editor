using Avalonia.Controls;

namespace Editor;

public interface IMainWindow
{
    KeyboardHelper KeyboardHelper { get; }
    ClipboardHelper ClipboardHelper { get; }
    UndoManager UndoManager { get; }

    TopLevel TopLevel { get; }
    WindowState WindowState { get; set; }
    bool ContentDialogShown { get; }
    bool IsSelecting { get; set; }
    bool IsEditorLoaded { get; }

    EditorMode TextEditorMode { get; set; }
    FontType TextFontType { get; set; }

    bool InputBold { get; set; }
    bool InputItalic { get; set; }
    bool InputUnderline { get; set; }
    bool UseItalicIntergalOnNew { get; set; }
    bool ShowUnderbar { get; set; }

    bool IgnoreTextEditorModeChange { get; set; }
    bool IgnoreTextFontTypeChange { get; set; }
    bool IgnoreInputBoldChange { get; set; }
    bool IgnoreInputItalicChange { get; set; }
    bool IgnoreInputUnderlineChange { get; set; }

    int CustomZoomPercentage { get; set; }

    int ActiveChildSelectionStartIndex { get; set; }
    int ActiveChildSelectedItems { get; set; }
    string StatusBarRightMessage { get; set; }

    void Close();
    void HandleToolBarCommand(CommandDetails commandDetails);
    void HandleUserCommand(CommandDetails commandDetails);
    bool TryHideEquationToolBarVisiblePanel();
    bool TryHideCharacterToolBarVisiblePanel();
}
