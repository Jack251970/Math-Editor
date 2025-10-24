using System.Windows;
using System.Windows.Media;

namespace Editor
{
    public static class DrawingContextHelper
    {
        public static void DrawPolyline(this DrawingContext dc, Point startPoint, PointCollection points, Pen pen)
        {
            var geometry = new PathGeometry();
            var segment = new PolyLineSegment
            {
                Points = points
            };
            var fig = new PathFigure(startPoint, [segment], false);
            geometry.Figures.Add(fig);
            dc.DrawGeometry(null, pen, geometry);
        }

        public static void FillPolylineGeometry(this DrawingContext dc, Point startPoint, PointCollection points)
        {
            var geometry = new PathGeometry();
            var segment = new PolyLineSegment
            {
                Points = points
            };
            var fig = new PathFigure(startPoint, [segment], true);
            geometry.Figures.Add(fig);
            dc.DrawGeometry(PenManager.TextFillColorPrimaryBrush, null, geometry);
        }
    }
}
