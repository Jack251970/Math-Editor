using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Editor;

public partial class MainWindow : Window, ICultureInfoChanged
{
    private static readonly string ClassName = nameof(MainWindow);

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
        OpenFile(ViewModel.CurrentLocalFile);

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
        if (!CheckSaveCurrentDocument())
        {
            e.Cancel = true;
            return;
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
        if (!CheckSaveCurrentDocument())
        {
            return;
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

    #region Open & Save & Save As & Export

    private bool CheckSaveCurrentDocument()
    {
        if (Editor.Dirty)
        {
            var result = MessageBox.Show(Localize.MainWindow_SaveCurrentDocument(),
                Constants.MathEditorFullName, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Cancel)
            {
                return false;
            }
            else if (result == MessageBoxResult.Yes)
            {
                if (!ProcessFileSave())
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void OpenFile(string filePath)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            try
            {
                using (Stream stream = File.OpenRead(filePath))
                {
                    Editor.LoadFile(stream);
                }
                ViewModel.CurrentLocalFile = filePath;
            }
            catch (Exception e)
            {
                ViewModel.CurrentLocalFile = string.Empty;
                EditorLogger.Fatal(ClassName, "Failed to load file", e);
                MessageBox.Show(Localize.EditorControl_CannotOpenFile(), Localize.Error(),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

    private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }

    private void SaveCommandHandler(object sender, ExecutedRoutedEventArgs e)
    {
        ProcessFileSave();
    }

    private bool ProcessFileSave()
    {
        var savePath = ViewModel.CurrentLocalFile;
        if (!File.Exists(savePath))
        {
            var result = ShowSaveFileDialog(Constants.MedExtension,
                Localize.MainWindow_MedFileFilter(Constants.MedExtension));
            if (string.IsNullOrEmpty(result))
            {
                return false;
            }
            else
            {
                savePath = result;
            }
        }
        return SaveFile(savePath);
    }

    private bool SaveFile(string filePath)
    {
        try
        {
            using (Stream stream = File.Open(filePath, FileMode.Create))
            {
                Editor.SaveFile(stream, filePath);
            }
            ViewModel.CurrentLocalFile = filePath;
            return true;
        }
        catch (Exception e)
        {
            EditorLogger.Fatal(ClassName, "Failed to save file", e);
            MessageBox.Show(Localize.EditorControl_CannotSaveFile(), Localize.Error(),
                MessageBoxButton.OK, MessageBoxImage.Error);
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
            SaveFile(result);
        }
    }

    private void ExportMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var imageType = sender is Control control && control.Tag is string imageTypeStr ? imageTypeStr : "png";

        var fileName = ShowSaveFileDialog(imageType, Localize.MainWindow_ImageFileFilter(imageType));
        if (!string.IsNullOrEmpty(fileName))
        {
            var ext = Path.GetExtension(fileName);
            if (ext != "." + imageType)
                fileName += "." + imageType;
            Editor.ExportImage(fileName);
        }
    }

    #endregion

    private MenuItem? lastZoomMenuItem = null;

    private void ZoomMenuItem_Click(object sender, RoutedEventArgs e)
    {
        customZoomMenu.Header = "_Custom";
        customZoomMenu.IsChecked = false;
        if (lastZoomMenuItem != null && sender != lastZoomMenuItem)
        {
            lastZoomMenuItem.IsChecked = false;
        }
        lastZoomMenuItem = (MenuItem)sender;
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

    public void OnCultureInfoChanged(CultureInfo newCultureInfo)
    {
        ViewModel.OnCultureInfoChanged(newCultureInfo);
    }
}
