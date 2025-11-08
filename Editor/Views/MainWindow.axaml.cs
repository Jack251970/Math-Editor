using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Editor;

public partial class MainWindow : Window, IMainWindow, ICultureInfoChanged, IContentDialogOwner
{
    public MainWindowViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<MainWindowViewModel>();
    public KeyboardHelper KeyboardHelper { get; init; }

    public bool IsEditorLoaded { get; private set; } = false;
    public bool ContentDialogShown { get; private set; } = false;

    public EditorControl Editor { get; init; }
    public CharacterToolBar CharacterToolBar { get; init; }
    public EquationToolBar EquationToolBar { get; init; }
    public HistoryToolBar HistoryToolBar { get; init; }

    // Guard flag to allow re-entrance after async confirmation
    private bool _canClose;

    public MainWindow() : this(string.Empty)
    {
    }

    public MainWindow(string currentLocalFile)
    {
        ViewModel.CurrentLocalFile = currentLocalFile;
        ViewModel.MainWindow = this;
        DataContext = ViewModel;
        // Initialize all components
        InitializeComponent();
        CharacterToolBar = new CharacterToolBar(this)
        {
            Padding = new Thickness(0, 4, 0, 4),
            ZIndex = 100,
            ClipToBounds = false
        };
        EquationToolBar = new EquationToolBar(this)
        {
            Padding = new Thickness(0, 6, 0, 6),
            ZIndex = 99,
            ClipToBounds = false
        };
        HistoryToolBar = new HistoryToolBar(this)
        {
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xB6, 0xB6, 0xB6)),
            BorderThickness = new Thickness(0, 0, 0, 1)
        };
        TopToolBars.Children.Add(CharacterToolBar);
        TopToolBars.Children.Add(EquationToolBar);
        TopToolBars.Children.Add(HistoryToolBar);
        Editor = new EditorControl(this)
        {
            Background = Brushes.Transparent,
            FocusAdorner = null,
            Focusable = true,
        };
        Editor.Loaded += Editor_Loaded;
        ScrollViewer.Content = Editor;
        // Init keyboard tracker
        KeyboardHelper = new(this);
        // Track this window
        WindowTracker.TrackOwner(this);
        // Add event handlers
        Editor.ZoomChanged += Editor_ZoomChanged;
        CharacterToolBar.CommandCompleted += CharacterToolBar_CommandCompleted;
        EquationToolBar.CommandCompleted += EquationToolBar_CommandCompleted;
    }

    private async void Editor_Loaded(object? sender, RoutedEventArgs e)
    {
        ViewModel.Editor = Editor;
        ViewModel.InitializeRecentFiles(RecentFileItem);

        // Check if we have a file to open
        await ViewModel.OpenFileAsync(ViewModel.CurrentLocalFile);

        // Init editor mode & editor font
        ViewModel.ChangeEditorMode(App.Settings.DefaultMode);
        ViewModel.ChangeEditorFont(App.Settings.DefaultFont);
        // No need to change them since they are already false by default
        /*ViewModel.ChangeInputBold(false);
        ViewModel.ChangeInputItalic(false);
        ViewModel.ChangeInputUnderline(false);*/
        Editor.Focus();

        // Init zoom percentage item
        ViewModel.SetLastZoomPercentageItem(DefaultZoomPercentageItem);

        IsEditorLoaded = true;
    }

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        Editor.InvalidateVisual();
    }

    public void HandleToolBarCommand(CommandDetails commandDetails)
    {
        if (commandDetails.CommandType == CommandType.CustomMatrix)
        {
            if (commandDetails.CommandParam is int[] rowsAndColumns && rowsAndColumns.Length == 2)
            {
                WindowOpener.OpenDialog<MatrixInputWindow>(this, rowsAndColumns[0], rowsAndColumns[1]);
            }
        }
        else
        {
            Editor.HandleUserCommand(commandDetails);
            if (commandDetails.CommandType == CommandType.Text)
            {
                var symbol = commandDetails.UnicodeString;

                // Add to used list
                if (!App.Settings.UsedSymbolList.TryAdd(symbol, 1))
                {
                    App.Settings.UsedSymbolList[symbol] += 1;
                }

                // Add to recent list
                var recentList = App.Settings.RecentSymbolList;
                if (recentList.Count >= Constants.MaxSymbols)
                {
                    recentList.RemoveAt(recentList.Count - 1);
                }
                recentList.Insert(0, symbol);
            }
        }
    }

    private void Window_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // TODO: Check if this can work properly
        var source = e.Source;
        if (source != null)
        {
            if (source is EditorToolBarButton)
            {
                return;
            }
            else if (Editor.IsPointerOver)
            {
                //Editor.HandleMouseDown();
                Editor.Focus();
            }
            CharacterToolBar.TryHideVisiblePanel();
            EquationToolBar.TryHideVisiblePanel();
        }
    }

    private void Window_TextInput(object sender, TextInputEventArgs e)
    {
        if (!Editor.IsFocused)
        {
            Editor.Focus();
            //Editor.EditorControl_TextInput(null, e);
            if (!string.IsNullOrEmpty(e.Text))
            {
                // Replace ASCII hyphen-minus '-' with Unicode minus '\u2212'
                // for consistent mathematical notation rendering
                Editor.ConsumeText(e.Text.Replace('-', '\u2212'));
            }
            CharacterToolBar.TryHideVisiblePanel();
            EquationToolBar.TryHideVisiblePanel();
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (!Editor.IsFocused)
        {
            Editor.Focus();
            CharacterToolBar.TryHideVisiblePanel();
            EquationToolBar.TryHideVisiblePanel();
        }
    }

    private void Window_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && ViewModel.FullScreenMode)
        {
            ViewModel.FullScreenMode = false;
        }
    }

    private async void Window_Closing(object sender, WindowClosingEventArgs e)
    {
        // If we already confirmed close, allow it and perform cleanup once
        if (_canClose)
        {
            Editor.ZoomChanged -= Editor_ZoomChanged;
            CharacterToolBar.CommandCompleted -= CharacterToolBar_CommandCompleted;
            EquationToolBar.CommandCompleted -= EquationToolBar_CommandCompleted;
            Editor.Dispose();
            return;
        }

        // Cancel the close first, then run async flow
        e.Cancel = true;

        if (!await ViewModel.CheckSaveCurrentDocumentAsync())
        {
            // User canceled or save failed; keep window open
            return;
        }

        // Mark as confirmable and close again after this handler returns
        _canClose = true;
        _ = Dispatcher.UIThread.InvokeAsync(Close);
    }

    private void Editor_ZoomChanged(object? sender, int number)
    {
        ViewModel.IgnoreEditorZoomPercentageChange = true;
        ViewModel.CustomZoomPercentage = number;
        ViewModel.IgnoreEditorZoomPercentageChange = false;
    }

    private void EquationToolBar_CommandCompleted(object? sender, EventArgs e)
    {
        Editor.Focus();
    }

    private void CharacterToolBar_CommandCompleted(object? sender, EventArgs e)
    {
        Editor.Focus();
    }

    public void OnCultureInfoChanged(CultureInfo newCultureInfo)
    {
        ViewModel.OnCultureInfoChanged(newCultureInfo);
    }

    public void ContentDialogChanged(bool isShown)
    {
        Editor.SetTimer(!isShown);
        // TODO: Set window non-draggable when dialog is shown
        ContentDialogShown = isShown;
    }

    #region IMainWindow Implementation

    public ClipboardHelper ClipboardHelper => ViewModel.ClipboardHelper;
    public UndoManager UndoManager => ViewModel.UndoManager;
    public TopLevel TopLevel => GetTopLevel(this)!;

    public bool IsSelecting
    {
        get => ViewModel.IsSelecting;
        set => ViewModel.IsSelecting = value;
    }

    public EditorMode TextEditorMode
    {
        get => ViewModel.TextEditorMode;
        set => ViewModel.ChangeEditorMode(value);
    }

    public FontType TextFontType
    {
        get => ViewModel.TextFontType;
        set => ViewModel.TextFontType = value;
    }

    public bool InputBold
    {
        get => ViewModel.InputBold;
        set => ViewModel.InputBold = value;
    }

    public bool InputItalic
    {
        get => ViewModel.InputItalic;
        set => ViewModel.InputItalic = value;
    }

    public bool InputUnderline
    {
        get => ViewModel.InputUnderline;
        set => ViewModel.InputUnderline = value;
    }

    public bool UseItalicIntergalOnNew
    {
        get => ViewModel.UseItalicIntergalOnNew;
        set => ViewModel.UseItalicIntergalOnNew = value;
    }

    public bool ShowUnderbar
    {
        get => ViewModel.ShowUnderbar;
        set => ViewModel.ShowUnderbar = value;
    }

    public bool IgnoreTextEditorModeChange
    {
        get => ViewModel.IgnoreTextEditorModeChange;
        set => ViewModel.IgnoreTextEditorModeChange = value;
    }

    public bool IgnoreTextFontTypeChange
    {
        get => ViewModel.IgnoreTextFontTypeChange;
        set => ViewModel.IgnoreTextFontTypeChange = value;
    }

    public bool IgnoreInputBoldChange
    {
        get => ViewModel.IgnoreInputBoldChange;
        set => ViewModel.IgnoreInputBoldChange = value;
    }

    public bool IgnoreInputItalicChange
    {
        get => ViewModel.IgnoreInputItalicChange;
        set => ViewModel.IgnoreInputItalicChange = value;
    }

    public bool IgnoreInputUnderlineChange
    {
        get => ViewModel.IgnoreInputUnderlineChange;
        set => ViewModel.IgnoreInputUnderlineChange = value;
    }

    public int CustomZoomPercentage
    {
        get => ViewModel.CustomZoomPercentage;
        set => ViewModel.CustomZoomPercentage = value;
    }

    public int ActiveChildSelectionStartIndex
    {
        get => ViewModel.ActiveChildSelectionStartIndex;
        set => ViewModel.ActiveChildSelectionStartIndex = value;
    }

    public int ActiveChildSelectedItems
    {
        get => ViewModel.ActiveChildSelectedItems;
        set => ViewModel.ActiveChildSelectedItems = value;
    }

    public string StatusBarRightMessage
    {
        get => ViewModel.StatusBarRightMessage;
        set => ViewModel.StatusBarRightMessage = value;
    }


    public void HandleUserCommand(CommandDetails commandDetails)
    {
        Editor.HandleUserCommand(commandDetails);
    }

    public bool TryHideEquationToolBarVisiblePanel()
    {
        return EquationToolBar.TryHideVisiblePanel();
    }

    public bool TryHideCharacterToolBarVisiblePanel()
    {
        return CharacterToolBar.TryHideVisiblePanel();
    }

    #endregion
}
