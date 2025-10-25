using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Editor;

public partial class MainWindow : Window
{
    public bool IsEditorLoaded { get; private set; } = false;
    public MainWindowViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<MainWindowViewModel>();

    private string _currentLocalFile = string.Empty;

    public EditorControl Editor { get; set; } = null!;

    public MainWindow(string currentLocalFile)
    {
        _currentLocalFile = currentLocalFile;
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
        // Set title
        SetTitle();
        // Track this window
        WindowTracker.TrackOwner(this);
        // Add event handlers
        AddHandler(UIElement.MouseDownEvent, new MouseButtonEventHandler(MainWindow_MouseDown), true);
        ViewModel.UndoManager.CanUndo += UndoManager_CanUndo;
        ViewModel.UndoManager.CanRedo += UndoManager_CanRedo;
        Editor.ZoomChanged += Editor_ZoomChanged;
        CharacterToolBar.CommandCompleted += CharacterToolBar_CommandCompleted;
        EquationToolBar.CommandCompleted += EquationToolBar_CommandCompleted;
    }

    private void Editor_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Editor = Editor;

        // Check if we have a file to open
        OpenFile(_currentLocalFile);

        // Init editor mode & editor font
        ViewModel.ChangeEditorMode(App.Settings.DefaultMode);
        ViewModel.ChangeEditorFont(App.Settings.DefaultFont);
        // No need to change them since they are already false by default
        /*ViewModel.ChangeInputBold(false);
        ViewModel.ChangeInputItalic(false);
        ViewModel.ChangeInputUnderline(false);*/
        Editor.Focus();

        IsEditorLoaded = true;
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
                //editor.HandleMouseDown();
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
            //editor.EditorControl_TextInput(null, e);
            Editor.ConsumeText(e.Text);
            CharacterToolBar.HideVisiblePanel();
            EquationToolBar.HideVisiblePanel();
        }
    }

    private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        if (Editor.Dirty)
        {
            var result = MessageBox.Show(Localize.MainWindow_SaveCurrentDocument(),
                Constants.MathEditorFullName, MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            else if (result == MessageBoxResult.Yes)
            {
                if (!ProcessFileSave())
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        ViewModel.UndoManager.CanUndo -= UndoManager_CanUndo;
        ViewModel.UndoManager.CanRedo -= UndoManager_CanRedo;
        Editor.ZoomChanged -= Editor_ZoomChanged;
        CharacterToolBar.CommandCompleted -= CharacterToolBar_CommandCompleted;
        EquationToolBar.CommandCompleted -= EquationToolBar_CommandCompleted;
    }

    private void UndoManager_CanUndo(object? sender, UndoEventArgs e)
    {
        ViewModel.UndoButtonIsEnabled = e.ActionPossible;
    }

    private void UndoManager_CanRedo(object? sender, UndoEventArgs e)
    {
        ViewModel.RedoButtonIsEnabled = e.ActionPossible;
    }

    private void EquationToolBar_CommandCompleted(object? sender, EventArgs e)
    {
        Editor.Focus();
    }

    private void CharacterToolBar_CommandCompleted(object? sender, EventArgs e)
    {
        Editor.Focus();
    }

    private void OpenCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        if (Editor.Dirty)
        {
            var mbResult = MessageBox.Show(Localize.MainWindow_SaveCurrentDocument(),
                Constants.MathEditorFullName, MessageBoxButton.YesNoCancel);
            if (mbResult == MessageBoxResult.Cancel)
            {
                return;
            }
            else if (mbResult == MessageBoxResult.Yes)
            {
                if (!ProcessFileSave())
                {
                    return;
                }
            }
        }
        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            CheckPathExists = true,
            Filter = Localize.MainWindow_MedFileFilter(Constants.MedExtension)
        };
        var result = ofd.ShowDialog(this);
        if (result == true)
        {
            OpenFile(ofd.FileName);
        }
    }

    private void OpenFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }
        try
        {
            using (Stream stream = File.OpenRead(fileName))
            {
                Editor.LoadFile(stream);
            }
            _currentLocalFile = fileName;
        }
        catch
        {
            _currentLocalFile = "";
            MessageBox.Show("File is corrupt or inaccessible OR it was created by an incompatible version of Math Editor.", "Error");
        }
        SetTitle();
    }

    private void SetTitle()
    {
        if (!string.IsNullOrEmpty(_currentLocalFile))
        {
#if DEBUG
            Title = $"{Constants.MathEditorFullName} v{Constants.Version} ({Constants.Dev}) - {_currentLocalFile}";
#else
            Title = $"{Constants.MathEditorFullName} v{Constants.Version} - {_currentLocalFile}";
#endif
        }
        else
        {
#if DEBUG
            Title = $"{Constants.MathEditorFullName} v{Constants.Version} ({Constants.Dev}) - {Localize.MainWindow_Untitled()}";
#else
            Title = $"{Constants.MathEditorFullName} v{Constants.Version} - {Localize.MainWindow_Untitled()}";
#endif
        }
    }

    private string? ShowSaveFileDialog(string extension, string filter)
    {
        var sfd = new Microsoft.Win32.SaveFileDialog
        {
            DefaultExt = "." + extension,
            Filter = filter
        };
        var result = sfd.ShowDialog(this);
        if (result == true)
        {
            return Path.GetExtension(sfd.FileName) == "." + extension ? sfd.FileName : sfd.FileName + "." + extension;
        }
        else
        {
            return null;
        }
    }

    private void SaveCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        ProcessFileSave();
    }

    private bool ProcessFileSave()
    {
        if (!File.Exists(_currentLocalFile))
        {
            var result = ShowSaveFileDialog(Constants.MedExtension,
                Localize.MainWindow_MedFileFilter(Constants.MedExtension));
            if (string.IsNullOrEmpty(result))
            {
                return false;
            }
            else
            {
                _currentLocalFile = result;
            }
        }
        return SaveFile();
    }

    private bool SaveFile()
    {
        try
        {
            using (Stream stream = File.Open(_currentLocalFile, FileMode.Create))
            {
                Editor.SaveFile(stream, _currentLocalFile);
            }
            SetTitle();
            return true;
        }
        catch
        {
            MessageBox.Show("File could not be saved. Make sure you have permission to write the file to disk.", "Error");
            Editor.Dirty = true;
        }
        return false;
    }

    private void SaveAsCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        var result = ShowSaveFileDialog(Constants.MedExtension,
            Localize.MainWindow_MedFileFilter(Constants.MedExtension));
        if (!string.IsNullOrEmpty(result))
        {
            _currentLocalFile = result;
            SaveFile();
        }
    }

    private void CutCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        Editor.Copy(true);
    }

    private void CopyCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        Editor.Copy(false);
    }

    private void PasteCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        Editor.Paste();
    }

    private void PrintCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {

    }

    private void UndoCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        Editor.Undo();
    }

    private void RedoCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        Editor.Redo();
    }

    private void SelectAllCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        Editor.SelectAll();
    }

    private void Window_GotFocus(object sender, RoutedEventArgs e)
    {
    }

    private void exportMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var imageType = (string)((Control)sender).Tag ?? "png";
        var fileName = ShowSaveFileDialog(imageType, string.Format("Image File (*.{0})|*.{0}", imageType));
        if (!string.IsNullOrEmpty(fileName))
        {
            var ext = Path.GetExtension(fileName);
            if (ext != "." + imageType)
                fileName += "." + imageType;
            Editor.ExportImage(fileName);
        }
    }

    private void ToolBar_Loaded(object sender, RoutedEventArgs e)
    {
        var toolBar = sender as ToolBar;
        // TODO: This cannot work. We need to find another way to hide OverflowGrid automatically.
        if (toolBar?.Template.FindName("OverflowGrid", toolBar) is FrameworkElement overflowGrid)
        {
            overflowGrid.Visibility = Visibility.Collapsed;
        }
    }

    private void deleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Editor.DeleteSelection();
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
        if (WindowStyle == WindowStyle.None && e.Key == Key.Escape)
        {
            ToggleFullScreen();
        }
    }

    private void ToggleFullScreen()
    {
        if (WindowStyle == WindowStyle.None)
        {
            fullScreenMenuItem.Header = "_Full Screen";
            WindowStyle = WindowStyle.ThreeDBorderWindow;
            WindowState = WindowState.Normal;
            exitFullScreenButton.Visibility = Visibility.Collapsed;
            closeApplictionButton.Visibility = Visibility.Collapsed;
        }
        else
        {
            fullScreenMenuItem.Header = "_Normal Screen";
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Normal; //extral call to be on safe side. windows is funky
            WindowState = WindowState.Maximized;
            exitFullScreenButton.Visibility = Visibility.Visible;
            closeApplictionButton.Visibility = Visibility.Visible;
        }
    }

    private void fullScreenMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ToggleFullScreen();
    }

    private MenuItem? lastZoomMenuItem = null;

    private void ZoomMenuItem_Click(object sender, RoutedEventArgs e)
    {
        customZoomMenu.Header = "_Custom";
        customZoomMenu.IsChecked = false;
        if (lastZoomMenuItem != null && sender != lastZoomMenuItem)
        {
            lastZoomMenuItem.IsChecked = false;
        }
        lastZoomMenuItem = ((MenuItem)sender);
        lastZoomMenuItem.IsChecked = true;
        var percentage = lastZoomMenuItem.Header as string;
        if (!string.IsNullOrEmpty(percentage))
        {
            Editor.SetFontSizePercentage(int.Parse(percentage.Replace("%", "")));
        }
    }

    private void Editor_ZoomChanged(object? sender, EventArgs e)
    {
        customZoomMenu.Header = "_Custom";
        customZoomMenu.IsChecked = false;
        if (lastZoomMenuItem != null)
        {
            lastZoomMenuItem.IsChecked = false;
            lastZoomMenuItem = null;
        }
    }

    public void SetFontSizePercentage(int number)
    {
        customZoomMenu.Header = "_Custom (" + number + "%)";
        customZoomMenu.IsChecked = true;
        if (lastZoomMenuItem != null)
        {
            lastZoomMenuItem.IsChecked = false;
            lastZoomMenuItem = null;
        }
        Editor.SetFontSizePercentage(number);
    }

    private void exitFullScreenButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleFullScreen();
    }

    public void SetStatusBarMessage(string message)
    {
        statusBarLeftLabel.Content = message;
    }

    public void ShowCoordinates(string coordinates)
    {
        statusBarRightLabel.Content = coordinates;
    }

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        Editor.InvalidateVisual();
    }

    private void NewCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        if (Editor.Dirty)
        {
            var result = MessageBox.Show(Localize.MainWindow_SaveCurrentDocument(),
                Constants.MathEditorFullName, MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Cancel)
            {
                return;
            }
            else if (result == MessageBoxResult.Yes)
            {
                if (!ProcessFileSave())
                {
                    return;
                }
            }
        }
        _currentLocalFile = "";
        SetTitle();
        Editor.Clear();
    }
}
