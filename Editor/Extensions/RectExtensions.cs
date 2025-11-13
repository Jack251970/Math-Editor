using Avalonia;

namespace Editor;

public static class RectExtensions
{
    extension(Rect rect)
    {
        public static Rect Empty => default;

        public bool IsEmpty => rect == default;
    }
}
