using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Editor;

public sealed class Caret : FrameworkElement, IDisposable
{
    public double CaretLength { get; set; } = 18d;

    private Point location;
    private readonly bool _isHorizontal = false;
    private Point OtherPoint => _isHorizontal ? new Point(Left + CaretLength, Top) : new Point(Left, VerticalCaretBottom);

    public Caret(bool isHorizontal)
    {
        _isHorizontal = isHorizontal;
        Visible = true;
    }

    protected override void OnRender(DrawingContext dc)
    {
        if (Visible)
        {
            dc.DrawLine(PenManager.GetPen(1), Location, OtherPoint);
        }
    }

    public void ToggleVisibility()
    {
        if (!_isDisposed)
        {
            Dispatcher.Invoke(() => { Visible = !Visible; });
        }
    }

    public void ForceVisible(bool visible)
    {
        if (!_isDisposed)
        {
            Dispatcher.Invoke(() => { Visible = visible; });
        }
    }

    public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register("Visible", typeof(bool), typeof(Caret), new FrameworkPropertyMetadata(false /* defaultValue */, FrameworkPropertyMetadataOptions.AffectsRender));

    private bool Visible
    {
        get => (bool)GetValue(VisibleProperty);
        set
        {
            try
            {
                SetValue(VisibleProperty, value);
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
        set => location = new Point(Math.Floor(value.X) + .5, Math.Floor(value.Y) + .5);
    }

    public double Left
    {
        get => location.X;
        set => location = new Point(Math.Floor(value) + .5, location.Y);
    }

    public double Top
    {
        get => location.Y;
        set => location = new Point(location.X, Math.Floor(value) + .5);
    }

    public double VerticalCaretBottom => location.Y + CaretLength;

    #region IDisposable

    private bool _isDisposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(true);
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
