using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Editor;

public partial class MainWindow : Window
{
    public bool IsInialized { get; private set; } = false;
    private readonly MainWindowViewModel _viewModel = Ioc.Default.GetRequiredService<MainWindowViewModel>();

    private string _currentLocalFile = string.Empty;
    private static readonly string MedFileFilter = "Math Editor File (*." + Constants.MedExtension + ")|*." + Constants.MedExtension;

    public MainWindow(string currentLocalFile)
    {
        _currentLocalFile = currentLocalFile;
        DataContext = _viewModel;
        InitializeComponent();
        StatusBarHelper.Init(this);
        characterToolBar.CommandCompleted += (x, y) => { editor.Focus(); };
        equationToolBar.CommandCompleted += (x, y) => { editor.Focus(); };
        SetTitle();
        AddHandler(UIElement.MouseDownEvent, new MouseButtonEventHandler(MainWindow_MouseDown), true);
        UndoManager.CanUndo += (a, b) => { undoButton.IsEnabled = b.ActionPossible; };
        UndoManager.CanRedo += (a, b) => { redoButton.IsEnabled = b.ActionPossible; };
        EquationBase.SelectionAvailable += new EventHandler<EventArgs>(editor_SelectionAvailable);
        EquationBase.SelectionUnavailable += new EventHandler<EventArgs>(editor_SelectionUnavailable);
        underbarToggle.IsChecked = true;
        editor.ZoomChanged += new EventHandler(editor_ZoomChanged);
    }

    private void editor_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel.Editor = editor;

        // Check if we have a file to open
        OpenFile(_currentLocalFile);

        // Init editor mode & editor font
        _viewModel.ChangeEditorMode(App.Settings.DefaultMode);
        _viewModel.ChangeEditorFont(App.Settings.DefaultFont);
        editor.Focus();

        IsInialized = true;
    }

    private void editor_SelectionUnavailable(object? sender, EventArgs e)
    {
        copyMenuItem.IsEnabled = false;
        cutMenuItem.IsEnabled = false;
        deleteMenuItem.IsEnabled = false;
        cutButton.IsEnabled = false;
        copyButton.IsEnabled = false;
    }

    private void editor_SelectionAvailable(object? sender, EventArgs e)
    {
        copyMenuItem.IsEnabled = true;
        cutMenuItem.IsEnabled = true;
        deleteMenuItem.IsEnabled = true;
        cutButton.IsEnabled = true;
        copyButton.IsEnabled = true;
    }

    public void HandleToolBarCommand(CommandDetails commandDetails)
    {
        if (commandDetails.CommandType == CommandType.CustomMatrix)
        {
            if (commandDetails.CommandParam is int[] rowsAndColumns && rowsAndColumns.Length == 2)
            {
                var inputForm = new MatrixInputWindow(rowsAndColumns[0], rowsAndColumns[1]);
                inputForm.ProcessRequest += (x, y) =>
                {
                    var newCommand = new CommandDetails
                    {
                        CommandType = CommandType.Matrix,
                        CommandParam = new int[] { x, y }
                    };
                    editor.HandleUserCommand(newCommand);
                };
                _ = inputForm.ShowDialog();
            }
            else
            {
                throw new Exception("Invalid parameters for CustomMatrix command");
            }
        }
        else
        {
            editor.HandleUserCommand(commandDetails);
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
            else if (editor.IsMouseOver)
            {
                //editor.HandleMouseDown();
                editor.Focus();
            }
            characterToolBar.HideVisiblePanel();
            equationToolBar.HideVisiblePanel();
        }
    }

    private void Window_TextInput(object sender, TextCompositionEventArgs e)
    {
        if (!editor.IsFocused)
        {
            editor.Focus();
            //editor.EditorControl_TextInput(null, e);
            editor.ConsumeText(e.Text);
            characterToolBar.HideVisiblePanel();
            equationToolBar.HideVisiblePanel();
        }
    }

    private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        if (editor.Dirty)
        {
            var result = MessageBox.Show("Do you want to save the current document before closing?", "Please confirm", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
            else if (result == MessageBoxResult.Yes)
            {
                if (!ProcessFileSave())
                {
                    e.Cancel = true;
                }
            }
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        TextEquation.InputPropertyChanged -= _viewModel.TextEquation_InputPropertyChanged;

        // Fix App abnormal exit when App displays MessageBox before MainWindow
        Application.Current.Shutdown();
    }

    private void OpenCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        if (editor.Dirty)
        {
            var mbResult = MessageBox.Show("Do you want to save the current document before closing?", "Please confirm", MessageBoxButton.YesNoCancel);
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
            Filter = MedFileFilter
        };
        var result = ofd.ShowDialog();
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
                editor.LoadFile(stream);
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
            Title = $"{Constants.MathEditorFullName} v{Constants.Version} ({Constants.Dev}) - Untitled 1";
#else
            Title = $"{Constants.MathEditorFullName} v{Constants.Version} - Untitled 1";
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
            var result = ShowSaveFileDialog(Constants.MedExtension, MedFileFilter);
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
                editor.SaveFile(stream, _currentLocalFile);
            }
            SetTitle();
            return true;
        }
        catch
        {
            MessageBox.Show("File could not be saved. Make sure you have permission to write the file to disk.", "Error");
            editor.Dirty = true;
        }
        return false;
    }

    private void SaveAsCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        var result = ShowSaveFileDialog(Constants.MedExtension, MedFileFilter);
        if (!string.IsNullOrEmpty(result))
        {
            _currentLocalFile = result;
            SaveFile();
        }
    }

    private void CutCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        editor.Copy(true);
    }

    private void CopyCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        editor.Copy(false);
    }

    private void PasteCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        editor.Paste();
    }

    private void PrintCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {

    }

    private void UndoCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        editor.Undo();
    }

    private void RedoCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        editor.Redo();
    }

    private void SelectAllCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        editor.SelectAll();
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
            editor.ExportImage(fileName);
        }
    }

    private void showNestingMenuItem_Click(object sender, RoutedEventArgs e)
    {
        TextEquation.ShowNesting = !TextEquation.ShowNesting;
        if (TextEquation.ShowNesting)
        {
            showNestingMenuItem.Header = "Hide Nesting";
        }
        else
        {
            showNestingMenuItem.Header = "Show Nesting";
        }
        editor.InvalidateVisual();
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

    private void UnderbarToggleCheckChanged(object sender, RoutedEventArgs e)
    {
        editor.ShowOverbar(underbarToggle.IsChecked == true);
    }

    private void deleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        editor.DeleteSelection();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (!editor.IsFocused)
        {
            editor.Focus();
            characterToolBar.HideVisiblePanel();
            equationToolBar.HideVisiblePanel();
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
            editor.SetFontSizePercentage(int.Parse(percentage.Replace("%", "")));
        }
    }

    private void editor_ZoomChanged(object? sender, EventArgs e)
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
        editor.SetFontSizePercentage(number);
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

    private void integralItalicCheckbox_Checked(object sender, RoutedEventArgs e)
    {
        EquationRow.UseItalicIntergalOnNew = true;
    }

    private void integralItalicCheckbox_Unchecked(object sender, RoutedEventArgs e)
    {
        EquationRow.UseItalicIntergalOnNew = false;
    }

    private void scrollViwer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        editor.InvalidateVisual();
    }

    private void NewCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        if (editor.Dirty)
        {
            var result = MessageBox.Show("Do you want to save the current document before closing?", "Please confirm", MessageBoxButton.YesNoCancel);
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
        editor.Clear();
    }
}
public static class StatusBarHelper
{
    private static MainWindow? window = null;
    public static void Init(MainWindow _window)
    {
        window = _window;
    }

    public static void PrintStatusMessage(string message)
    {
        // TODO: Implement this method
        window?.SetStatusBarMessage(message);
    }

    public static void ShowCoordinates(string message)
    {
        window?.ShowCoordinates(message);
    }
}
