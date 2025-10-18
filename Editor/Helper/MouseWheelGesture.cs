using System.Windows.Input;

namespace Editor;

public sealed class MouseWheelGesture : MouseGesture
{
    public WheelDirection Direction { get; set; }

    public static MouseWheelGesture CtrlDown => new(ModifierKeys.Control) { Direction = WheelDirection.Down };

    public static MouseWheelGesture CtrlUp => new(ModifierKeys.Control) { Direction = WheelDirection.Up };
    public MouseWheelGesture()
        : base(MouseAction.WheelClick)
    {
    }

    public MouseWheelGesture(ModifierKeys modifiers)
        : base(MouseAction.WheelClick, modifiers)
    {
    }

    public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
    {
        if (!base.Matches(targetElement, inputEventArgs)) return false;
        if (inputEventArgs is not MouseWheelEventArgs) return false;
        var args = (MouseWheelEventArgs)inputEventArgs;
        return Direction switch
        {
            WheelDirection.None => args.Delta == 0,
            WheelDirection.Up => args.Delta > 0,
            WheelDirection.Down => args.Delta < 0,
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
