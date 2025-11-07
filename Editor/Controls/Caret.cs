using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace Editor;

public sealed class Caret : Control, IDisposable
{
    public static readonly StyledProperty<double> CaretLengthProperty =
        AvaloniaProperty.Register<Caret, double>(nameof(CaretLength), 18d);

    public static readonly StyledProperty<bool> IsCaretVisibleProperty =
        AvaloniaProperty.Register<Caret, bool>(nameof(IsCaretVisible), true);

    static Caret()
    {
        AffectsRender<Caret>(CaretLengthProperty, IsCaretVisibleProperty);
    }

    public double CaretLength
    {
        get => GetValue(CaretLengthProperty);
        set => SetValue(CaretLengthProperty, value);
    }

    private Point location;
    private readonly bool _isHorizontal = false;
    private Point OtherPoint => _isHorizontal ? new Point(Left + CaretLength, Top) : new Point(Left, VerticalCaretBottom);

    public Caret(bool isHorizontal)
    {
        _isHorizontal = isHorizontal;
        IsCaretVisible = true;
    }

    public override void Render(DrawingContext context)
    {
        if (IsCaretVisible)
        {
            context.DrawLine(PenManager.GetPen(1), Location, OtherPoint);
        }
    }

    public void ToggleVisibility()
    {
        if (!_isDisposed)
        {
            Dispatcher.UIThread.Invoke(() => { IsCaretVisible = !IsCaretVisible; });
        }
    }

    public void ForceVisible(bool visible)
    {
        if (!_isDisposed)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                try
                {
                    IsCaretVisible = visible;
                }
                catch (TaskCanceledException)
                {
                    // when the object got disposed, the SetValue operation will fail
                }
            });
        }
    }

    public bool IsCaretVisible
    {
        get => GetValue(IsCaretVisibleProperty);
        set
        {
            try
            {
                SetValue(IsCaretVisibleProperty, value);
            }
            catch (TaskCanceledException)
            {
                // when the object got disposed, the SetValue operation will fail
            }
        }
    }

    public Point Location
    {
        get => location;
        set
        {
            location = new Point(Math.Floor(value.X) + .5, Math.Floor(value.Y) + .5);
            InvalidateVisual();
        }
    }

    public double Left
    {
        get => location.X;
        set
        {
            location = new Point(Math.Floor(value) + .5, location.Y);
            InvalidateVisual();
        }
    }

    public double Top
    {
        get => location.Y;
        set
        {
            location = new Point(location.X, Math.Floor(value) + .5);
            InvalidateVisual();
        }
    }

    public double VerticalCaretBottom => location.Y + CaretLength;

    #region IDisposable

    private bool _isDisposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Caret()
    {
        Dispose(false);
    }

    private void Dispose(bool disposing)
    {
        _isDisposed = true;
    }

    #endregion
}
