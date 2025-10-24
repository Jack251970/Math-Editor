using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;

namespace Editor;

public static class WindowTracker
{
    private static readonly List<Window> _ownerWindows = [];
    private static readonly ConcurrentDictionary<Window, Window> _activeWindows = [];

    public static void TrackOwner(Window owner)
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
            if (_ownerWindows.Count == 0)
            {
                Application.Current.Shutdown();
            }
        };
        _ownerWindows.Add(owner);
    }

    public static void TrackWindow(Window window, Window owner)
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

    public static List<T> GetActiveWindows<T>(Window owner) where T : Window
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
