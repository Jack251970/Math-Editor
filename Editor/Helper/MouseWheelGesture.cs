using Avalonia.Input;

namespace Editor;

// TODO: Do we need this?
public sealed class MouseWheelGesture
{
    public WheelDirection Direction { get; init; }

    public KeyModifiers Modifiers { get; init; }

    public static MouseWheelGesture CtrlDown => new(KeyModifiers.Control) { Direction = WheelDirection.Down };

    public static MouseWheelGesture CtrlUp => new(KeyModifiers.Control) { Direction = WheelDirection.Up };

    public MouseWheelGesture()
        : this(KeyModifiers.None)
    {
    }

    public MouseWheelGesture(KeyModifiers modifiers)
    {
        Modifiers = modifiers;
    }

    public bool Matches(PointerWheelEventArgs e)
    {
        return Matches(null, e);
    }

    public bool Matches(object? targetElement, PointerWheelEventArgs e)
    {
        // Require that all specified modifiers are present
        if ((e.KeyModifiers & Modifiers) != Modifiers)
            return false;

        var dy = e.Delta.Y;
        return Direction switch
        {
            WheelDirection.None => dy == 0,
            WheelDirection.Up => dy > 0,
            WheelDirection.Down => dy < 0,
            _ => false,
        };
    }

    public enum WheelDirection
    {
        None,
        Up,
        Down,
    }
}
