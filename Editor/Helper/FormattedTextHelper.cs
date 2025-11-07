using Avalonia;
using Avalonia.Media;

namespace Editor
{
    public static class FormattedTextHelper
    {
        public static void DrawTextTopLeftAligned(this FormattedText text, DrawingContext dc, Point topLeft, bool forceBlackBrush)
        {
            //double descent = text.Height - text.Baseline + text.OverhangAfter;
            //double topExtra = text.Baseline - text.Extent + descent;
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(Brushes.Black);
            }
            var topExtra = text.Height + text.OverhangAfter - text.Extent;
            double padding = 0;
            // TODO: Review whether this condition is necessary
            /*if (text.Text.Length > 0 && !char.IsWhiteSpace(text.Text[0]))
            {
                padding = text.OverhangLeading;
            }*/
            padding = text.OverhangLeading;
            dc.DrawText(text, new Point(topLeft.X - padding, topLeft.Y - topExtra));
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(PenManager.TextFillColorPrimaryBrush);
            }
        }

        public static void DrawText(this FormattedText text, DrawingContext dc, Point topLeft, bool forceBlackBrush)
        {
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(Brushes.Black);
            }
            // TODO: Review whether this condition is necessary
            /*if (text.Text.Length > 0)
            {
                dc.DrawText(text, new Point(topLeft.X, topLeft.Y));
            }*/
            dc.DrawText(text, new Point(topLeft.X, topLeft.Y));
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(PenManager.TextFillColorPrimaryBrush);
            }
        }

        public static void DrawTextLeftAligned(this FormattedText text, DrawingContext dc, Point topLeft, bool forceBlackBrush)
        {
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(Brushes.Black);
            }
            // TODO: Review whether this condition is necessary
            /*if (text.Text.Length > 0)
            {
                if (char.IsWhiteSpace(text.Text[0]))
                {
                    dc.DrawText(text, new Point(topLeft.X, topLeft.Y));
                }
                else
                {
                    dc.DrawText(text, new Point(topLeft.X - text.OverhangLeading, topLeft.Y));
                }
            }*/
            dc.DrawText(text, new Point(topLeft.X, topLeft.Y));
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(PenManager.TextFillColorPrimaryBrush);
            }
        }

        public static void DrawTextCenterAligned(this FormattedText text, DrawingContext dc, Point hCenter, bool forceBlackBrush)
        {
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(Brushes.Black);
            }
            // TODO: Review whether this condition is necessary
            /*if (text.Text.Length > 0)
            {
                var width = text.GetFullWidth();
                dc.DrawText(text, new Point(hCenter.X - width / 2 - text.OverhangLeading, hCenter.Y));
            }*/
            var width = text.GetFullWidth();
            dc.DrawText(text, new Point(hCenter.X - width / 2 - text.OverhangLeading, hCenter.Y));
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(PenManager.TextFillColorPrimaryBrush);
            }
        }

        public static void DrawTextRightAligned(this FormattedText text, DrawingContext dc, Point topRight, bool forceBlackBrush)
        {
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(Brushes.Black);
            }
            dc.DrawText(text, new Point(topRight.X - text.GetFullWidth(), topRight.Y));
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(PenManager.TextFillColorPrimaryBrush);
            }
        }

        public static void DrawTextTopRightAligned(this FormattedText text, DrawingContext dc, Point topRight, bool forceBlackBrush)
        {
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(Brushes.Black);
            }
            var descent = text.Height - text.Baseline + text.OverhangAfter;
            var topExtra = text.Baseline - text.Extent + descent;
            dc.DrawText(text, new Point(topRight.X - text.GetFullWidth() - text.OverhangLeading, topRight.Y - topExtra));
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(PenManager.TextFillColorPrimaryBrush);
            }
        }

        public static void DrawTextBottomLeftAligned(this FormattedText text, DrawingContext dc, Point bottomLeft, bool forceBlackBrush)
        {
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(Brushes.Black);
            }
            var descent = text.Height - text.Baseline + text.OverhangAfter;
            var topExtra = text.Baseline - text.Extent + descent;
            dc.DrawText(text, new Point(bottomLeft.X - text.OverhangLeading, bottomLeft.Y - topExtra - text.Extent));
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(PenManager.TextFillColorPrimaryBrush);
            }
        }

        public static void DrawTextBottomCenterAligned(this FormattedText text, DrawingContext dc, Point bottomCenter, bool forceBlackBrush)
        {
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(Brushes.Black);
            }
            var descent = text.Height - text.Baseline + text.OverhangAfter;
            var topExtra = text.Baseline - text.Extent + descent;
            dc.DrawText(text, new Point(bottomCenter.X - text.OverhangLeading - text.GetFullWidth() / 2, bottomCenter.Y - topExtra - text.Extent));
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(PenManager.TextFillColorPrimaryBrush);
            }
        }

        public static void DrawTextTopCenterAligned(this FormattedText text, DrawingContext dc, Point topCenter, bool forceBlackBrush)
        {
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(Brushes.Black);
            }
            var descent = text.Height - text.Baseline + text.OverhangAfter;
            var topExtra = text.Baseline - text.Extent + descent;
            dc.DrawText(text, new Point(topCenter.X - text.OverhangLeading - text.GetFullWidth() / 2, topCenter.Y - topExtra));
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(PenManager.TextFillColorPrimaryBrush);
            }
        }

        public static void DrawTextBottomRightAligned(this FormattedText text, DrawingContext dc, Point bottomRight, bool forceBlackBrush)
        {
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(Brushes.Black);
            }
            var descent = text.Height - text.Baseline + text.OverhangAfter;
            var topExtra = text.Baseline - text.Extent + descent;
            dc.DrawText(text, new Point(bottomRight.X - text.GetFullWidth() - text.OverhangLeading, bottomRight.Y - topExtra - text.Extent));
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(PenManager.TextFillColorPrimaryBrush);
            }
        }

        public static double GetFullWidth(this FormattedText ft)
        {
            // TODO: Review whether this condition is necessary
            /*if (ft.Text.Length > 0)
            {
                if (char.IsWhiteSpace(ft.Text[0]) && char.IsWhiteSpace(ft.Text[^1]))
                {
                    return ft.WidthIncludingTrailingWhitespace;
                }
                else if (char.IsWhiteSpace(ft.Text[0]))
                {
                    return ft.WidthIncludingTrailingWhitespace - ft.OverhangTrailing;
                }
                else if (char.IsWhiteSpace(ft.Text[^1]))
                {
                    return ft.WidthIncludingTrailingWhitespace - ft.OverhangLeading;
                }
                else
                {
                    return ft.WidthIncludingTrailingWhitespace - ft.OverhangLeading - ft.OverhangTrailing;
                }
            }
            else
            {
                return 0;
            }*/
            return ft.WidthIncludingTrailingWhitespace - ft.OverhangLeading - ft.OverhangTrailing;
        }

        /*
        //double width = formattedText.WidthIncludingTrailingWhitespace;
        //if (formattedText.Text.Length > 0)
        //{
        //    if (!char.IsSeparator(formattedText.Text[0]))
        //    {
        //        if (formattedText.OverhangLeading < 0)
        //        {
        //            width -= formattedText.OverhangLeading;
        //        }
        //    }
        //    if (!char.IsSeparator(formattedText.Text[formattedText.Text.Length - 1]))
        //    {
        //        width -= formattedText.OverhangTrailing;
        //    }
        //}
        //return width;
        */

        public static double Descent(this FormattedText ft)
        {
            return ft.Height - ft.Baseline + ft.OverhangAfter;
        }

        public static double TopExtra(this FormattedText ft)
        {
            // = ft.Baseline - ft.Extent + ft.Descent() 
            // = ft.Baseline - ft.Extent + (ft.Height - ft.Baseline + ft.OverhangAfter)
            return ft.Height - ft.Extent + ft.OverhangAfter;
        }

        public static double GetRight(this FormattedText ft)
        {
            // TODO: Review whether this condition is necessary
            /*if (ft.Text.Length > 0)
            {
                if (char.IsWhiteSpace(ft.Text[^1]))
                {
                    return ft.WidthIncludingTrailingWhitespace;
                }
                else
                {
                    return ft.WidthIncludingTrailingWhitespace - ft.OverhangTrailing;
                }
            }
            else
            {
                return 0;
            }*/
            return ft.WidthIncludingTrailingWhitespace - ft.OverhangTrailing;
        }
    }
}
