using Avalonia;

namespace Editor;

public static class VisualTreeHelper
{
    public static Point GetOffset(this Visual parent, Visual visual)
    {
        return visual.TranslatePoint(new Point(0, 0), parent) ?? new Point(0, 0);
    }
}
