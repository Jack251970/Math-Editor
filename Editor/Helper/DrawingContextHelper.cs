using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace Editor
{
    public static class DrawingContextHelper
    {
        public static void DrawPolyline(this DrawingContext dc, Point startPoint, List<Point> points, Pen pen)
        {
            var geometry = new PathGeometry();
            var segment = new PolyLineSegment
            {
                Points = points
            };
            var fig = new PathFigure
            {
                StartPoint = startPoint,
                Segments = [segment],
                IsClosed = false
            };
            geometry.Figures?.Add(fig);
            dc.DrawGeometry(null, pen, geometry);
        }

        public static void FillPolylineGeometry(this DrawingContext dc, Point startPoint, List<Point> points, bool forceBlackBrush)
        {
            var geometry = new PathGeometry();
            var segment = new PolyLineSegment
            {
                Points = points
            };
            var fig = new PathFigure
            {
                StartPoint = startPoint,
                Segments = [segment],
                IsClosed = true
            };
            geometry.Figures?.Add(fig);
            dc.DrawGeometry(forceBlackBrush ? Brushes.Black : PenManager.TextFillColorPrimaryBrush, null, geometry);
        }
    }
}
