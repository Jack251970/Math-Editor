using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace Editor;

public static class WindowTracker
{
    private static readonly List<IMainWindow> _ownerWindows = [];
    private static readonly ConcurrentDictionary<Window, IMainWindow> _activeWindows = [];

    public static void TrackOwner(MainWindow owner)
    {
        owner.Closed += (sender, args) =>
        {
            var windowsToRemove = new List<Window>();
            foreach (var pair in _activeWindows)
            {
                var window = pair.Key;
                var ownerWindow = pair.Value;
                if (ownerWindow == owner)
                {
                    windowsToRemove.Add(window);
                }
            }
            foreach (var window in windowsToRemove)
            {
                window.Close();
                _activeWindows.TryRemove(window, out var _);
            }
            _ownerWindows.Remove(owner);
            if (Application.Current == null)
            {
                return;
            }
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (_ownerWindows.Count == 0)
                {
                    desktop.Shutdown();
                }
            }
            else
            {
                throw new NotSupportedException("Unsupported application lifetime");
            }
        };
        _ownerWindows.Add(owner);
    }

    public static void TrackWindow(Window window, IMainWindow owner)
    {
        if (!_ownerWindows.Contains(owner))
        {
            throw new KeyNotFoundException("The owner window is not registered in the tracker.");
        }
        window.Closed += (sender, args) =>
        {
            _activeWindows.TryRemove(window, out var _);
        };
        _activeWindows.TryAdd(window, owner);
    }

    public static List<Window> GetAllWindows()
    {
        if (Application.Current == null)
        {
            return [];
        }
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return [..desktop.Windows];
        }
        else
        {
            throw new NotSupportedException("Unsupported application lifetime");
        }
    }

    public static List<Window> GetOwnerWindows()
    {
        var result = new List<Window>();
        foreach (var owner in _ownerWindows)
        {
            result.Add((Window)owner);
        }
        return result;
    }

    public static List<Window> GetActiveWindows()
    {
        return [.. _activeWindows.Keys];
    }

    public static List<T> GetActiveWindows<T>() where T : Window
    {
        var result = new List<T>();
        foreach (var pair in _activeWindows)
        {
            var window = pair.Key;
            if (window is T typedWindow)
            {
                result.Add(typedWindow);
            }
        }
        return result;
    }

    public static List<T> GetActiveWindows<T>(IMainWindow owner) where T : Window
    {
        var result = new List<T>();
        foreach (var pair in _activeWindows)
        {
            var window = pair.Key;
            var ownerWindow = pair.Value;
            if (ownerWindow != owner)
            {
                continue;
            }
            if (window is T typedWindow)
            {
                result.Add(typedWindow);
            }
        }
        return result;
    }
}
