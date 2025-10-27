using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Editor;

public partial class MainWindow : Window, ICultureInfoChanged
{
    public bool IsEditorLoaded { get; private set; } = false;

    public MainWindowViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<MainWindowViewModel>();

    public EditorControl Editor { get; set; } = null!;

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
        AddHandler(UIElement.MouseDownEvent, new MouseButtonEventHandler(MainWindow_MouseDown), true);
        Editor.ZoomChanged += Editor_ZoomChanged;
        CharacterToolBar.CommandCompleted += CharacterToolBar_CommandCompleted;
        EquationToolBar.CommandCompleted += EquationToolBar_CommandCompleted;
    }

    private void Editor_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Editor = Editor;

        // Check if we have a file to open
        ViewModel.OpenFile(ViewModel.CurrentLocalFile);

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

    private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
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
            CharacterToolBar.HideVisiblePanel();
            EquationToolBar.HideVisiblePanel();
        }
    }

    private void Window_TextInput(object sender, TextCompositionEventArgs e)
    {
        if (!Editor.IsFocused)
        {
            Editor.Focus();
            //Editor.EditorControl_TextInput(null, e);
            Editor.ConsumeText(e.Text);
            CharacterToolBar.HideVisiblePanel();
            EquationToolBar.HideVisiblePanel();
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (!Editor.IsFocused)
        {
            Editor.Focus();
            CharacterToolBar.HideVisiblePanel();
            EquationToolBar.HideVisiblePanel();
        }
    }

    private void Window_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && ViewModel.FullScreenMode)
        {
            ViewModel.FullScreenMode = false;
        }
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        if (!ViewModel.CheckSaveCurrentDocument())
        {
            e.Cancel = true;
            return;
        }

        Editor.ZoomChanged -= Editor_ZoomChanged;
        CharacterToolBar.CommandCompleted -= CharacterToolBar_CommandCompleted;
        EquationToolBar.CommandCompleted -= EquationToolBar_CommandCompleted;
        Editor.Dispose();
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
}
