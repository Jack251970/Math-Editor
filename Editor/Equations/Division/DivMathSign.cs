using Avalonia;
using Avalonia.Media;

namespace Editor
{
    public sealed class DivMathSign : EquationBase
    {
        public bool IsInverted { get; set; }

        public DivMathSign(IMainWindow owner, EquationContainer parent)
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
                Width = FontSize * .25 + Height * .03;
            }
        }

        public override void DrawEquation(DrawingContext dc, bool forceBlackBrush)
        {
            LineSegment line;
            ArcSegment arc;
            Point pathFigureStart;
            var pen = forceBlackBrush ? BlackStandardRoundPen : StandardRoundPen;
            if (IsInverted)
            {
                pathFigureStart = new Point(ParentEquation.Right, Bottom - pen.Thickness / 2);
                line = new LineSegment()
                {
                    Point = new Point(Left, Bottom - pen.Thickness / 2),
                    IsStroked = true
                };
                arc = new ArcSegment()
                {
                    Point = Location,
                    Size = new Size(Width * 4.5, Height),
                    RotationAngle = 0,
                    IsLargeArc = false,
                    SweepDirection = SweepDirection.CounterClockwise,
                    IsStroked = true
                };
            }
            else
            {
                pathFigureStart = new Point(ParentEquation.Right, Top);
                line = new LineSegment()
                {
                    Point = Location,
                    IsStroked = true
                };
                arc = new ArcSegment()
                {
                    Point = new Point(Left, Bottom),
                    Size = new Size(Width * 4.5, Height),
                    RotationAngle = 0,
                    IsLargeArc = false,
                    SweepDirection = SweepDirection.Clockwise,
                    IsStroked = true
                };
            }
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure()
            {
                StartPoint = pathFigureStart,
                Segments = [line, arc],
                IsClosed = false
            };
            pathGeometry.Figures!.Add(pathFigure);
            dc.DrawGeometry(null, pen, pathGeometry);
        }
    }
}
