using System;
using System.Linq;
using Avalonia.Controls;

namespace Editor;

public static class WindowOpener
{
    /// <summary>
    /// Single for the application
    /// </summary>
    public static T OpenSingle<T>(IMainWindow owner, params object[] args) where T : Window
    {
        var window = WindowTracker.GetActiveWindows<T>().FirstOrDefault()
            ?? (T?)Activator.CreateInstance(typeof(T), args)
            ?? throw new ArgumentNullException(null, $"{nameof(Window)} instance could not be created or found");

        WindowTracker.TrackWindow(window, owner);

        return ShowWindow(window, owner);
    }

    /// <summary>
    /// Single for one owner
    /// </summary>
    public static T OpenScoped<T>(IMainWindow owner, params object[] args) where T : Window
    {
        var window = WindowTracker.GetActiveWindows<T>(owner).FirstOrDefault()
            ?? (T?)Activator.CreateInstance(typeof(T), args)
            ?? throw new ArgumentNullException(null, $"{nameof(Window)} instance could not be created or found");

        WindowTracker.TrackWindow(window, owner);

        return ShowWindow(window, owner);
    }

    /// <summary>
    /// Create new every time
    /// </summary>
    public static T OpenTransient<T>(IMainWindow owner, params object[] args) where T : Window
    {
        var window = (T?)Activator.CreateInstance(typeof(T), args)
            ?? throw new ArgumentNullException(null, $"{nameof(Window)} instance could not be created or found");

        WindowTracker.TrackWindow(window, owner);

        return ShowWindow(window, owner);
    }

    /// <summary>
    /// Open dialog for one window
    /// </summary>
    public static T OpenDialog<T>(IMainWindow owner, params object[] args) where T : Window
    {
        var window = (T?)Activator.CreateInstance(typeof(T), args)
            ?? throw new ArgumentNullException(null, $"{nameof(Window)} instance could not be created or found");

        WindowTracker.TrackWindow(window, owner);

        return ShowWindowDialog(window, owner);
    }

    private static T ShowWindow<T>(T window, IMainWindow owner) where T : Window
    {
        // Fix UI bug
        // Add `window.WindowState = WindowState.Normal`
        // If only use `window.Show()`, Settings-window doesn't show when minimized in taskbar 
        // Not sure why this works tho
        // Probably because, when `.Show()` fails, `window.WindowState == Minimized` (not `Normal`) 
        // https://stackoverflow.com/a/59719760/4230390
        // Ensure the window is not minimized before showing it
        if (window.WindowState == WindowState.Minimized)
        {
            window.WindowState = WindowState.Normal;
        }

        // Ensure the window is visible
        window.Show((Window)owner);

        window.Focus();

        return window;
    }

    private static T ShowWindowDialog<T>(T window, IMainWindow owner) where T : Window
    {
        // Fix UI bug
        // Add `window.WindowState = WindowState.Normal`
        // If only use `window.Show()`, Settings-window doesn't show when minimized in taskbar 
        // Not sure why this works tho
        // Probably because, when `.Show()` fails, `window.WindowState == Minimized` (not `Normal`) 
        // https://stackoverflow.com/a/59719760/4230390
        // Ensure the window is not minimized before showing it
        if (window.WindowState == WindowState.Minimized)
        {
            window.WindowState = WindowState.Normal;
        }

        // Show the window dialog
        window.ShowDialog((Window)owner);

        window.Focus();

        return window;
    }
}
