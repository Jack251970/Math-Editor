using System.Windows;
using System.Windows.Media;

namespace Editor
{
    public sealed class DivMathSign : EquationBase
    {
        public bool IsInverted { get; set; }

        public DivMathSign(MainWindow owner, EquationContainer parent)
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
                line = new LineSegment(new Point(Left, Bottom - pen.Thickness / 2), true);
                arc = new ArcSegment(Location, new Size(Width * 4.5, Height), 0, false, SweepDirection.Counterclockwise, true);
            }
            else
            {
                pathFigureStart = new Point(ParentEquation.Right, Top);
                line = new LineSegment(Location, true);
                arc = new ArcSegment(new Point(Left, Bottom), new Size(Width * 4.5, Height), 0, false, SweepDirection.Clockwise, true);
            }
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure(pathFigureStart, [line, arc], false);
            pathGeometry.Figures.Add(pathFigure);
            dc.DrawGeometry(null, pen, pathGeometry);
        }
    }
}
