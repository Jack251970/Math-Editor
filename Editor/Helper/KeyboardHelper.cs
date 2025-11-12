using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Editor;

public static class Keyboard
{
    public static bool IsKeyDown(IMainWindow window, Key key)
    {
        return window.KeyboardHelper.IsKeyDown(key);
    }
}

public class KeyboardHelper
{
    private readonly HashSet<Key> _pressed = [];

    public KeyboardHelper(Window window)
    {
        window.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
        window.AddHandler(InputElement.KeyUpEvent, OnKeyUp, RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        _pressed.Add(e.Key);
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        _pressed.Remove(e.Key);
    }

    public bool IsKeyDown(Key key)
    {
        return _pressed.Contains(key);
    }
}
