using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Editor;

[INotifyPropertyChanged]
public partial class MainWindow : Window
{
    public bool IsInialized { get; private set; } = false;

    private string _currentLocalFile = "";
    private const string MedExtension = "med";
    private static readonly string MedFileFilter = "Math Editor File (*." + MedExtension + ")|*." + MedExtension;
    
    public MainWindow(string currentLocalFile)
    {
        _currentLocalFile = currentLocalFile;
        DataContext = this;
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
        TextEquation.InputPropertyChanged += new PropertyChangedEventHandler(TextEquation_InputPropertyChanged);
        editor.ZoomChanged += new EventHandler(editor_ZoomChanged);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Check if we have a file to open
        OpenFile(_currentLocalFile);

        // TODO: Use Binding here and make sure it follows settings window
        var mode = App.Settings.DefaultMode.ToString();
        var fontName = App.Settings.DefaultFont;

        var modes = editorModeCombo.Items;
        foreach (ComboBoxItem item in modes)
        {
            if ((string)item.Tag == mode)
            {
                editorModeCombo.SelectedItem = item;
            }
        }
        var fonts = equationFontCombo.Items;
        foreach (ComboBoxItem item in fonts)
        {
            if ((string)item.Tag == fontName)
            {
                equationFontCombo.SelectedItem = item;
            }
        }
        ChangeEditorMode();
        ChangeEditorFont();
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
                // TODO: Can history toolbar work?
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
        App.Settings.Save();
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
        bool? result = ofd.ShowDialog();
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
        bool? result = sfd.ShowDialog(this);
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
            var result = ShowSaveFileDialog(MedExtension, MedFileFilter);
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
        var result = ShowSaveFileDialog(MedExtension, MedFileFilter);
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
            string ext = Path.GetExtension(fileName);
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
        if (toolBar?.Template.FindName("OverflowGrid", toolBar) is FrameworkElement overflowGrid)
        {
            overflowGrid.Visibility = Visibility.Collapsed;
        }
    }

    private void UnderbarToggleCheckChanged(object sender, RoutedEventArgs e)
    {
        editor.ShowOverbar(underbarToggle.IsChecked == true);
    }

    private void contentsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        BrowserHelper.Open("https://github.com/Jack251970/Math-Editor/wiki");
    }

    private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Window aboutWindow = new AboutWindow
        {
            Owner = this
        };
        aboutWindow.ShowDialog();
    }

    private void videoMenuItem_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Update link to actual video tutorials
        BrowserHelper.Open("https://github.com/Jack251970/Math-Editor");
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

    private void fbMenuItem_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Update link to actual Facebook page
        BrowserHelper.Open("https://github.com/Jack251970/Math-Editor");
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

    private void CustomZoomMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Window zoomWindow = new CustomZoomWindow
        {
            Owner = this
        };
        zoomWindow.Show();
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

    private Window? symbolWindow = null;
    private void symbolMenuItem_Click(object sender, RoutedEventArgs e)
    {
        symbolWindow ??= new UnicodeSelectorWindow
        {
            Owner = this
        };
        symbolWindow.Show();
        symbolWindow.Activate();
    }

    private Window? codePointWindow = null;
    private void codePointMenuItem_Click(object sender, RoutedEventArgs e)
    {
        codePointWindow ??= new CodepointWindow
        {
            Owner = this
        };
        codePointWindow.Show();
        codePointWindow.Activate();
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

    public IntPtr Handle => new System.Windows.Interop.WindowInteropHelper(this).Handle;

    public bool InputBold
    {
        get => TextEquation.InputBold;
        set
        {
            TextEquation.InputPropertyChanged -= TextEquation_InputPropertyChanged;
            TextEquation.InputBold = value;
            editor.ChangeFormat("format", "bold", value);
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
            editor.ChangeFormat("format", "italic", value);
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
            editor.ChangeFormat("format", "underline", value);
            TextEquation.InputPropertyChanged += TextEquation_InputPropertyChanged;
        }
    }

    private void TextEquation_InputPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "EditorMode")
        {
            var mode = TextEquation.EditorMode.ToString();
            var t = editorModeCombo.Items;
            foreach (ComboBoxItem item in t)
            {
                if ((string)item.Tag == mode)
                {
                    editorModeCombo.SelectedItem = item;
                }
            }
        }
        else if (e.PropertyName == "FontType")
        {
            string fontName = TextEquation.FontType.ToString();
            var t = equationFontCombo.Items;
            foreach (ComboBoxItem item in t)
            {
                if ((string)item.Tag == fontName)
                {
                    equationFontCombo.SelectedItem = item;
                }
            }

        }
        else
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
        }
    }

    private void ChangeEditorFont()
    {
        if (editor != null)
        {
            TextEquation.InputPropertyChanged -= TextEquation_InputPropertyChanged;
            TextEquation.FontType = (FontType)Enum.Parse(typeof(FontType), (string)((ComboBoxItem)equationFontCombo.SelectedItem).Tag);
            editor.ChangeFormat("font", (string)((ComboBoxItem)equationFontCombo.SelectedItem).Tag, true);
            TextEquation.InputPropertyChanged += TextEquation_InputPropertyChanged;
            editor.Focus();
        }
    }

    private void EquationFontCombo_DropDownClosed(object sender, EventArgs e)
    {
        try
        {
            ChangeEditorFont();
        }
        catch
        {
            MessageBox.Show("Cannot switch to the selected font", "Unidentified Error");
        }
    }

    // TODO: Check that we should follow the settings?
    private void ChangeEditorMode()
    {
        if (editor != null)
        {
            var item = (ComboBoxItem)editorModeCombo.SelectedItem;
            var mode = Enum.Parse<EditorMode>(item.Tag.ToString()!);

            TextEquation.InputPropertyChanged -= TextEquation_InputPropertyChanged;
            TextEquation.EditorMode = (EditorMode)Enum.Parse(typeof(EditorMode), (string)((ComboBoxItem)editorModeCombo.SelectedItem).Tag);
            editor.ChangeFormat("mode", mode.ToString().ToLower(), true);
            TextEquation.InputPropertyChanged += TextEquation_InputPropertyChanged;
            editor.Focus();
        }
    }

    private void EditorModeCombo_DropDownClosed(object sender, EventArgs e)
    {
        ChangeEditorMode();
    }

    private void mvHelpMenuItem_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Update link to actual discussion page
        BrowserHelper.Open("https://github.com/Jack251970/Math-Editor/discussions");
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

    private void settingsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Window settingsWindow = new SettingsWindow
        {
            Owner = this
        };
        settingsWindow.Show();
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
