using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Velopack;

namespace Editor;

public partial class MainWindowViewModel : ObservableObject, ICultureInfoChanged
{
    private static readonly string ClassName = nameof(MainWindowViewModel);

    public Settings Settings { get; init; }
    public UndoManager UndoManager { get; init; }
    public ClipboardHelper ClipboardHelper { get; init; }
    public UpdateManager UpdateManager { get; init; }
    public IMainWindow MainWindow { get; set; } = null!;
    public EditorControl? Editor { get; set; } = null;
    private MenuItem _recentFileItem = null!;

    private WindowState _windowState;
    private SystemDecorations _systemDecorations;
    private bool _fullScreenModeEntered;
    private readonly Lock _fullScreenModeLock = new();

    [ObservableProperty]
    public partial string MainWindowTitle { get; set; } = null!;

    [ObservableProperty]
    public partial string CurrentLocalFile { get; set; } = null!;

    [ObservableProperty]
    public partial string ShowNestingMenuItemHeader { get; set; } = null!;

    public MainWindowViewModel(Settings settings, UndoManager undoManager, ClipboardHelper clipboardHelper,
        UpdateManager updateManager)
    {
        Settings = settings;
        UndoManager = undoManager;
        ClipboardHelper = clipboardHelper;
        UpdateManager = updateManager;
        TextEditorMode = settings.DefaultMode;
        TextFontType = settings.DefaultFont;
        UpdateMainWindowTitle();
        UpdateShowNestingMenuItemHeader();
        UpdateFullScreenMenuItemHeader();
        UpdateCustomZoomPercentage();
        UpdateStatusBarLeftMessage();
    }

    public List<EditorModeLocalized> AllEditModes { get; } = EditorModeLocalized.GetValues();

    [ObservableProperty]
    public partial EditorMode TextEditorMode { get; set; }

    public List<FontTypeLocalized> AllFontTypes { get; } = FontTypeLocalized.GetValues();

    [ObservableProperty]
    public partial FontType TextFontType { get; set; }

    [ObservableProperty]
    public partial bool InputBold { get; set; }

    [ObservableProperty]
    public partial bool InputItalic { get; set; }

    [ObservableProperty]
    public partial bool InputUnderline { get; set; }

    [ObservableProperty]
    public partial bool IsSelecting { get; set; }

    [ObservableProperty]
    public partial bool UseItalicIntergalOnNew { get; set; }

    [ObservableProperty]
    public partial bool ShowUnderbar { get; set; } = true;

    [ObservableProperty]
    public partial string StatusBarLeftMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StatusBarRightMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int ActiveChildSelectionStartIndex { get; set; } = 0;

    [ObservableProperty]
    public partial int ActiveChildSelectedItems { get; set; } = 0;

    [ObservableProperty]
    public partial bool FullScreenMode { get; set; } = false;

    [ObservableProperty]
    public partial string FullScreenMenuItemHeader { get; set; } = null!;

    [ObservableProperty]
    public partial bool FullScreenButtonVisible { get; set; } = false;

    [ObservableProperty]
    public partial int CustomZoomPercentage { get; set; } = 0;

    [ObservableProperty]
    public partial bool CustomZoomMenuChecked { get; set; } = false;

    [ObservableProperty]
    public partial string CustomZoomMenuHeader { get; set; } = null!;

    public bool IgnoreTextEditorModeChange { get; set; } = false;
    public bool IgnoreTextFontTypeChange { get; set; } = false;
    public bool IgnoreInputBoldChange { get; set; } = false;
    public bool IgnoreInputItalicChange { get; set; } = false;
    public bool IgnoreInputUnderlineChange { get; set; } = false;

    public bool IgnoreEditorZoomPercentageChange { get; set; } = false;

    partial void OnCurrentLocalFileChanged(string value)
    {
        UpdateMainWindowTitle();
    }

    partial void OnTextEditorModeChanged(EditorMode value)
    {
        if (IgnoreTextEditorModeChange) return;
        ChangeEditorMode(value);
    }

    partial void OnTextFontTypeChanged(FontType value)
    {
        if (IgnoreTextFontTypeChange) return;
        ChangeEditorFont(value);
    }

    partial void OnInputBoldChanged(bool value)
    {
        if (IgnoreInputBoldChange) return;
        ChangeInputBold(value);
    }

    partial void OnInputItalicChanged(bool value)
    {
        if (IgnoreInputItalicChange) return;
        ChangeInputItalic(value);
    }

    partial void OnInputUnderlineChanged(bool value)
    {
        if (IgnoreInputUnderlineChange) return;
        ChangeInputUnderline(value);
    }

    public void ChangeEditorMode(EditorMode editorMode)
    {
        if (Editor != null)
        {
            Editor.ChangeFormat(nameof(EditorMode), editorMode, true);
            Editor.Focus();
        }
    }

    public void ChangeEditorFont(FontType fontType)
    {
        if (Editor != null)
        {
            Editor.ChangeFormat(nameof(FontType), fontType, true);
            Editor.Focus();
        }
    }

    public void ChangeInputBold(bool isBold)
    {
        if (Editor != null)
        {
            Editor.ChangeFormat(nameof(Format), Format.Bold, isBold);
            Editor.Focus();
        }
    }

    public void ChangeInputItalic(bool isItalic)
    {
        if (Editor != null)
        {
            Editor.ChangeFormat(nameof(Format), Format.Italic, isItalic);
            Editor.Focus();
        }
    }

    public void ChangeInputUnderline(bool isUnderline)
    {
        if (Editor != null)
        {
            Editor.ChangeFormat(nameof(Format), Format.Underline, isUnderline);
            Editor.Focus();
        }
    }

    partial void OnShowUnderbarChanged(bool value)
    {
        Editor?.ShowUnderbar(value);
    }

    private void UpdateMainWindowTitle()
    {
        var currentLocalFileName = TryGetFileName(CurrentLocalFile);
        if (!string.IsNullOrEmpty(currentLocalFileName))
        {
#if DEBUG
            MainWindowTitle = $"{Constants.MathEditorFullName} v{Constants.Version} ({Constants.Dev}) - {currentLocalFileName}";
#else
            MainWindowTitle = $"{Constants.MathEditorFullName} v{Constants.Version} - {currentLocalFileName}";
#endif
        }
        else
        {
#if DEBUG
            MainWindowTitle = $"{Constants.MathEditorFullName} v{Constants.Version} ({Constants.Dev}) - {Localize.MainWindow_Untitled()}";
#else
            MainWindowTitle = $"{Constants.MathEditorFullName} v{Constants.Version} - {Localize.MainWindow_Untitled()}";
#endif
        }
    }

    private static string TryGetFileName(string filePath)
    {
        var currentLocalFileName = string.Empty;
        try
        {
            currentLocalFileName = Path.GetFileName(filePath);
        }
        catch
        {
            // Ignore
        }
        return currentLocalFileName;
    }

    partial void OnCustomZoomPercentageChanged(int value)
    {
        if (!IgnoreEditorZoomPercentageChange && value != 0)
        {
            Editor?.SetFontSizePercentage(value);
        }
        UpdateCustomZoomPercentage();
    }

    private void UpdateCustomZoomPercentage()
    {
        if (CustomZoomPercentage == 0)
        {
            CustomZoomMenuChecked = false;
            CustomZoomMenuHeader = Localize.MainWindow_Custom();
        }
        else
        {
            CustomZoomMenuChecked = true;
            CustomZoomMenuHeader = Localize.MainWindow_CustomPercentage(CustomZoomPercentage);
            _lastZoomPercentageItem?.IsChecked = false;
            _lastZoomPercentageItem = null;
        }
    }

    private MenuItem? _lastZoomPercentageItem;

    [RelayCommand]
    private void ChangeZoomPercentage(MenuItem item)
    {
        if (item.Tag is string percentage &&
            !string.IsNullOrEmpty(percentage) &&
            int.TryParse(percentage, out var zoomPercentage))
        {
            Editor?.SetFontSizePercentage(zoomPercentage);
            SetLastZoomPercentageItem(item);
            // Reset the custom zoom percentage
            IgnoreEditorZoomPercentageChange = true;
            CustomZoomPercentage = 0;
            IgnoreEditorZoomPercentageChange = false;
        }
    }

    public void SetLastZoomPercentageItem(MenuItem item)
    {
        // Uncheck the last selected zoom percentage item
        if (_lastZoomPercentageItem != null && item != _lastZoomPercentageItem)
        {
            _lastZoomPercentageItem.IsChecked = false;
        }
        // Check the current selected zoom percentage item
        item.IsChecked = true;
        _lastZoomPercentageItem = item;
    }

    partial void OnActiveChildSelectionStartIndexChanged(int value)
    {
        UpdateStatusBarLeftMessage();
    }

    partial void OnActiveChildSelectedItemsChanged(int value)
    {
        UpdateStatusBarLeftMessage();
    }

    private void UpdateStatusBarLeftMessage()
    {
        StatusBarLeftMessage = Localize.MainWindow_StatusBarLeftMessage(ActiveChildSelectionStartIndex,
            ActiveChildSelectedItems);
    }

    [RelayCommand]
    private void ToggleShowNesting()
    {
        Settings.ShowNesting = !Settings.ShowNesting;
        UpdateShowNestingMenuItemHeader();
        Editor?.InvalidateVisual();
    }

    private void UpdateShowNestingMenuItemHeader()
    {
        ShowNestingMenuItemHeader = Settings.ShowNesting ?
            Localize.MainWindow_HideNesting() : Localize.MainWindow_ShowNesting();
    }

    [RelayCommand]
    private void ToggleFullScreenMode()
    {
        FullScreenMode = !FullScreenMode;
    }

    partial void OnFullScreenModeChanged(bool value)
    {
        if (value)
        {
            EnterFullScreen();
        }
        else
        {
            ExitFullScreen();
        }
        FullScreenButtonVisible = value;
        UpdateFullScreenMenuItemHeader();
    }

    public void EnterFullScreen()
    {
        lock (_fullScreenModeLock)
        {
            if (_fullScreenModeEntered) return;

            _windowState = MainWindow.WindowState;
            _systemDecorations = MainWindow.SystemDecorations;
            MainWindow.WindowState = WindowState.FullScreen;
            MainWindow.SystemDecorations = SystemDecorations.None;

            _fullScreenModeEntered = true;
        }
    }

    public void ExitFullScreen()
    {
        ExitFullScreen(_windowState);
    }

    public void ExitFullScreen(WindowState windowState)
    {
        lock (_fullScreenModeLock)
        {
            if (!_fullScreenModeEntered) return;

            MainWindow.WindowState = windowState;
            MainWindow.SystemDecorations = _systemDecorations;

            _fullScreenModeEntered = false;
        }
    }

    private void UpdateFullScreenMenuItemHeader()
    {
        FullScreenMenuItemHeader = FullScreenMode ? Localize.MainWindow_NormalScreen() : Localize.MainWindow_FullScreen();
    }

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

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        if (!UpdateManager.IsInstalled)
        {
            await MessageBox.ShowAsync(Localize.MainWindow_DownloadUpdatesManually(), Localize.MainWindow_CheckForUpdates(),
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        UpdateInfo? newVersion = null;
        try
        {
            newVersion = await Task.Run(() => UpdateManager.CheckForUpdates());
        }
        catch (Exception ex)
        {
            EditorLogger.Error(ClassName, "Failed to check for updates", ex);
            await MessageBox.ShowAsync(Localize.MainWindow_FailToCheckForUpdates(), Localize.MainWindow_CheckForUpdates(),
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (newVersion == null)
        {
            await MessageBox.ShowAsync(Localize.MainWindow_UpToDate(), Localize.MainWindow_CheckForUpdates(),
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var updateResult = await MessageBox.ShowAsync(
            Localize.MainWindow_NewVersionAvailable(newVersion.BaseRelease?.Version.ToString() ?? Localize.MainWindow_Unknown()),
            Localize.MainWindow_CheckForUpdates(), MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (updateResult == MessageBoxResult.Yes)
        {
            try
            {
                await Task.Run(() => UpdateManager.DownloadUpdates(newVersion));
            }
            catch (Exception ex)
            {
                EditorLogger.Error(ClassName, "Failed to download updates", ex);
                await MessageBox.ShowAsync(Localize.MainWindow_FailToDownloadUpdates(), Localize.MainWindow_CheckForUpdates(),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var restartResult = await MessageBox.ShowAsync(Localize.MainWindow_UpdateDownloaded(),
                Localize.MainWindow_CheckForUpdates(), MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (restartResult == MessageBoxResult.Yes)
            {
                UpdateManager.ApplyUpdatesAndRestart(newVersion);
            }
            else
            {
                UpdateManager.WaitExitThenApplyUpdates(newVersion);
            }
        }
    }

    [RelayCommand]
    private void New()
    {
        var mainWindow = new MainWindow(string.Empty);
        mainWindow.Show();
    }

    [RelayCommand]
    private async Task OpenAsync()
    {
        if (!await CheckSaveCurrentDocumentAsync())
        {
            return;
        }

        var files = await MainWindow.TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            Title = Localize.MainWindow_OpenMathEditorFile(),
            FileTypeFilter =
            [
                new FilePickerFileType(Localize.MainWindow_MedFile())
                {
                    Patterns = [$"*{Constants.MedExtension}"]
                },
                new FilePickerFileType(Localize.MainWindow_AllFiles())
                {
                    Patterns = ["*.*"]
                }
            ]
        });

        if (files.Count >= 1)
        {
            await OpenFileAsync(files[0].Path.LocalPath);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await ProcessFileSaveAsync();
    }

    [RelayCommand]
    private async Task SaveAsAsync()
    {
        var result = await ShowSaveFileDialog(Localize.MainWindow_SaveMathEditorFile(), Localize.MainWindow_MedFile(),
            Constants.MedExtension);
        if (!string.IsNullOrEmpty(result))
        {
            await SaveFileAsync(result);
        }
    }

    [RelayCommand]
    private void Cut()
    {
        Editor!.Cut();
    }

    [RelayCommand]
    private void Copy()
    {
        Editor!.Copy();
    }

    [RelayCommand]
    private void Paste()
    {
        Editor!.Paste();
    }

    [RelayCommand]
    private void ZoomOut()
    {
        Editor!.ZoomOut();
    }

    [RelayCommand]
    private void ZoomIn()
    {
        Editor!.ZoomIn();
    }

    [RelayCommand]
    private async Task ExportAsync(string imageExtension)
    {
        var fileName = await ShowSaveFileDialog(Localize.MainWindow_SaveImageFile(), Localize.MainWindow_SaveImageFile(),
            imageExtension);
        if (!string.IsNullOrEmpty(fileName))
        {
            await Editor!.ExportImageAsync(fileName);
        }
    }

    [RelayCommand]
    private async Task PrintAsync()
    {
        /*var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            await Editor!.PrintAsync(printDialog);
        }*/
    }

    [RelayCommand]
    private void SelectAll()
    {
        Editor!.SelectAll();
    }

    [RelayCommand]
    private void Undo()
    {
        Editor!.Undo();
    }

    [RelayCommand]
    private void Redo()
    {
        Editor!.Redo();
    }

    [RelayCommand]
    private void Delete()
    {
        Editor!.DeleteSelection();
    }

    [RelayCommand]
    private void Close()
    {
        MainWindow.Close();
    }

    [RelayCommand]
    private void Exit()
    {
        WindowTracker.GetOwnerWindows().ForEach(window => window.Close());
    }

    [RelayCommand]
    private void ClearRecentFiles()
    {
        Settings.RecentFiles.Clear();
        UpdateRecentFiles();
    }

    [RelayCommand]
    private async Task OpenRecentFileAsync(string file)
    {
        await OpenFileAsync(file);
    }

    public async Task OpenFileAsync(string filePath)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            if (!File.Exists(filePath))
            {
                CurrentLocalFile = string.Empty;
                await ContentDialogHelper.ShowAsync(MainWindow, Localize.EditorControl_CannotFindFile(filePath), Localize.Error(),
                    MessageBoxButton.OK);
                return;
            }
            try
            {
                using (Stream stream = File.OpenRead(filePath))
                {
                    await Editor!.LoadFileAsync(stream);
                }
                CurrentLocalFile = filePath;
                AddRecentFile(filePath);
            }
            catch (Exception e)
            {
                CurrentLocalFile = string.Empty;
                EditorLogger.Fatal(ClassName, "Failed to load file", e);
                await ContentDialogHelper.ShowAsync(MainWindow, Localize.EditorControl_CannotOpenFile(), Localize.Error(),
                    MessageBoxButton.OK);
            }
        }
    }

    private async Task<bool> SaveFileAsync(string filePath)
    {
        try
        {
            using (Stream stream = File.Open(filePath, FileMode.Create))
            {
                await Editor!.SaveFileAsync(stream, filePath);
            }
            CurrentLocalFile = filePath;
            return true;
        }
        catch (Exception e)
        {
            EditorLogger.Fatal(ClassName, "Failed to save file", e);
            await ContentDialogHelper.ShowAsync(MainWindow, Localize.EditorControl_CannotSaveFile(), Localize.Error(),
                MessageBoxButton.OK);
            Editor!.Dirty = true;
        }
        return false;
    }

    public async Task<bool> CheckSaveCurrentDocumentAsync()
    {
        if (Editor!.Dirty)
        {
            var result = await ContentDialogHelper.ShowAsync(MainWindow, Localize.MainWindow_SaveCurrentDocument(),
                Constants.MathEditorFullName, MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Cancel)
            {
                return false;
            }
            else if (result == MessageBoxResult.Yes)
            {
                if (!await ProcessFileSaveAsync())
                {
                    return false;
                }
            }
        }

        return true;
    }

    private async Task<bool> ProcessFileSaveAsync()
    {
        var savePath = CurrentLocalFile;
        if (!File.Exists(savePath))
        {
            var result = await ShowSaveFileDialog(Localize.MainWindow_SaveMathEditorFile(), Localize.MainWindow_MedFile(),
                Constants.MedExtension);
            if (string.IsNullOrEmpty(result))
            {
                return false;
            }
            else
            {
                savePath = result;
            }
        }
        return await SaveFileAsync(savePath);
    }

    private async Task<string?> ShowSaveFileDialog(string title, string extensionName, string extension)
    {
        var files = await MainWindow.TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            FileTypeChoices =
            [
                new FilePickerFileType(extensionName)
                {
                    Patterns = [$"*{extension}"]
                },
                new FilePickerFileType(Localize.MainWindow_AllFiles())
                {
                    Patterns = ["*.*"]
                }
            ]
        });

        return files?.TryGetLocalPath() ?? string.Empty;
    }

    public void InitializeRecentFiles(MenuItem recentFileItem)
    {
        _recentFileItem = recentFileItem;
        UpdateRecentFiles();
    }

    private void AddRecentFile(string filePath)
    {
        const int MaxRecentFiles = 10;

        if (Settings.RecentFiles.Count > 0 && Settings.RecentFiles[0] == filePath)
        {
            return;
        }

        if (Settings.RecentFiles.Count == MaxRecentFiles)
        {
            Settings.RecentFiles.RemoveAt(Settings.RecentFiles.Count - 1);
        }

        Settings.RecentFiles.Insert(0, filePath);
        UpdateRecentFiles();
    }

    private void UpdateRecentFiles()
    {
        _recentFileItem.Items.Clear();
        if (Settings.RecentFiles.Count == 0)
        {
            var menuItem = new MenuItem()
            {
                IsEnabled = false
            };
            menuItem.Bind(
                MenuItem.HeaderProperty,
                new DynamicResourceExtension(nameof(Localize.MainWindow_NoRecentFiles))
            );
            _recentFileItem.Items.Add(menuItem);
        }
        else
        {
            foreach (var file in Settings.RecentFiles)
            {
                _recentFileItem.Items.Add(new MenuItem()
                {
                    Header = file,
                    Command = OpenRecentFileCommand,
                    CommandParameter = file
                });
            }
            _recentFileItem.Items.Add(new Separator());
            var menuItem = new MenuItem()
            {
                Command = ClearRecentFilesCommand
            };
            menuItem.Bind(
                MenuItem.HeaderProperty,
                new DynamicResourceExtension(nameof(Localize.MainWindow_ClearList))
            );
            _recentFileItem.Items.Add(menuItem);
        }
    }

    public void OnCultureInfoChanged(CultureInfo newCultureInfo)
    {
        UpdateMainWindowTitle();
        UpdateShowNestingMenuItemHeader();
        UpdateFullScreenMenuItemHeader();
        UpdateCustomZoomPercentage();
        UpdateStatusBarLeftMessage();
        EditorModeLocalized.UpdateLabels(AllEditModes);
        FontTypeLocalized.UpdateLabels(AllFontTypes);
    }
}
