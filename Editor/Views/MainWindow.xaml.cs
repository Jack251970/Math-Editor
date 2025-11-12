using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Editor;

public partial class MainWindow : Window, ICultureInfoChanged, IContentDialogOwner
{
    public bool IsEditorLoaded { get; private set; } = false;

    public MainWindowViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<MainWindowViewModel>();

    public bool ContentDialogShown { get; private set; } = false;

    public EditorControl Editor { get; set; } = null!;

    private WindowChrome _draggableChrome = null!;
    private WindowChrome _nonDraggableChrome = null!;

    // Guard flag to allow re-entrance after async confirmation
    private bool _canClose;

    public MainWindow(string currentLocalFile)
    {
        ViewModel.CurrentLocalFile = currentLocalFile;
        ViewModel.MainWindow = this;
        DataContext = ViewModel;
        // Initialize all components
        InitializeComponent();
        var editor = new EditorControl(this)
        {
            Background = Brushes.Transparent,
            FocusVisualStyle = null,
            Focusable = true,
        };
        editor.Loaded += Editor_Loaded;
        Editor = editor;
        ScrollViewer.Content = editor;
        // Track this window
        WindowTracker.TrackOwner(this);
        // Add event handlers
        Editor.ZoomChanged += Editor_ZoomChanged;
        CharacterToolBar.CommandCompleted += CharacterToolBar_CommandCompleted;
        EquationToolBar.CommandCompleted += EquationToolBar_CommandCompleted;
    }

    // https://github.com/SuGar0218/NativeLikeCaptionButton-WPF
    private void Window_StateChanged(object sender, EventArgs e)
    {
        switch (WindowState)
        {
            case WindowState.Maximized:
                var left = SystemParameters.ResizeFrameVerticalBorderWidth + SystemParameters.FixedFrameVerticalBorderWidth + SystemParameters.BorderWidth;
                var top = SystemParameters.ResizeFrameHorizontalBorderHeight + SystemParameters.FixedFrameHorizontalBorderHeight + SystemParameters.BorderWidth;
                MainDock.Margin = new Thickness(left, top, left, top);
                break;
            default:
                MainDock.Margin = new Thickness(0);
                break;
        }
        if (WindowState == WindowState.Normal)
        {
            ViewModel.ExitFullScreen(WindowState.Normal);
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _draggableChrome = WindowChrome.GetWindowChrome(this);
        _nonDraggableChrome = (WindowChrome)_draggableChrome.Clone();
        _nonDraggableChrome.CaptionHeight = 0; // Disable caption height for non-draggable chrome
    }

    private async void Editor_Loaded(object sender, RoutedEventArgs e)
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
                HistoryToolBar.AddItem(commandDetails.UnicodeString);
            }
        }
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (Mouse.DirectlyOver != null)
        {
            if (Mouse.DirectlyOver.GetType() == typeof(EditorToolBarButton))
            {
                return;
            }
            else if (Editor.IsMouseOver)
            {
                //Editor.HandleMouseDown();
                Editor.Focus();
            }
            CharacterToolBar.TryHideVisiblePanel();
            EquationToolBar.TryHideVisiblePanel();
        }
    }

    private void Window_TextInput(object sender, TextCompositionEventArgs e)
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

    private async void Window_Closing(object sender, CancelEventArgs e)
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
        _ = Dispatcher.InvokeAsync(Close);
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
        WindowChrome.SetWindowChrome(this, isShown ? _nonDraggableChrome : _draggableChrome);
        ContentDialogShown = isShown;
    }
}
