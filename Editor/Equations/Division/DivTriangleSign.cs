using Avalonia;
using Avalonia.Media;

namespace Editor
{
    public sealed class DivTriangleSign : EquationBase
    {
        public DivTriangleSign(IMainWindow owner, EquationContainer parent)
            : base(owner, parent)
        {
            IsStatic = true;
        }

        public override double Height
        {
            get => base.Height;
            set
            {
                base.Height = value;
                Width = FontSize * .25 + Height * .06;
            }
        }

        public override double Bottom
        {
            get => base.Bottom - StandardPen.Thickness / 2; set => base.Bottom = value;
        }

        public override void DrawEquation(DrawingContext dc, bool forceBlackBrush)
        {
            var pen = forceBlackBrush ? BlackStandardRoundPen : StandardRoundPen;
            dc.DrawPolyline(new Point(ParentEquation.Right, Bottom),
            [
                new Point(Left, Bottom),
                new Point(Right, Top)
            ], pen);
        }
    }
}
