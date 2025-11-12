using Avalonia;
using Avalonia.Media;

namespace Editor
{
    public static class FormattedTextExtendedHelper
    {
        public static void DrawTextTopLeftAligned(this FormattedTextExtended text, DrawingContext dc, Point topLeft, bool forceBlackBrush)
        {
            //double descent = text.Height - text.Baseline + text.OverhangAfter;
            //double topExtra = text.Baseline - text.Extent + descent;
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(Brushes.Black);
            }
            var topExtra = text.Height + text.OverhangAfter - text.Extent;
            double padding = 0;
            if (text.Text.Length > 0 && !char.IsWhiteSpace(text.Text[0]))
            {
                padding = text.OverhangLeading;
            }
            dc.DrawText(text, new Point(topLeft.X - padding, topLeft.Y - topExtra));
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(PenManager.TextFillColorPrimaryBrush);
            }
        }

        public static void DrawText(this FormattedTextExtended text, DrawingContext dc, Point topLeft, bool forceBlackBrush)
        {
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(Brushes.Black);
            }
            if (text.Text.Length > 0)
            {
                dc.DrawText(text, new Point(topLeft.X, topLeft.Y));
            }
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(PenManager.TextFillColorPrimaryBrush);
            }
        }

        public static void DrawTextLeftAligned(this FormattedTextExtended text, DrawingContext dc, Point topLeft, bool forceBlackBrush)
        {
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(Brushes.Black);
            }
            if (text.Text.Length > 0)
            {
                if (char.IsWhiteSpace(text.Text[0]))
                {
                    dc.DrawText(text, new Point(topLeft.X, topLeft.Y));
                }
                else
                {
                    dc.DrawText(text, new Point(topLeft.X - text.OverhangLeading, topLeft.Y));
                }
            }
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(PenManager.TextFillColorPrimaryBrush);
            }
        }

        public static void DrawTextCenterAligned(this FormattedTextExtended text, DrawingContext dc, Point hCenter, bool forceBlackBrush)
        {
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(Brushes.Black);
            }
            if (text.Text.Length > 0)
            {
                var width = text.GetFullWidth();
                dc.DrawText(text, new Point(hCenter.X - width / 2 - text.OverhangLeading, hCenter.Y));
            }
            if (forceBlackBrush)
            {
                text.SetForegroundBrush(PenManager.TextFillColorPrimaryBrush);
            }
        }

        public static void DrawTextRightAligned(this FormattedTextExtended text, DrawingContext dc, Point topRight, bool forceBlackBrush)
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

        public static void DrawTextTopRightAligned(this FormattedTextExtended text, DrawingContext dc, Point topRight, bool forceBlackBrush)
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

        public static void DrawTextBottomLeftAligned(this FormattedTextExtended text, DrawingContext dc, Point bottomLeft, bool forceBlackBrush)
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

        public static void DrawTextBottomCenterAligned(this FormattedTextExtended text, DrawingContext dc, Point bottomCenter, bool forceBlackBrush)
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

        public static void DrawTextTopCenterAligned(this FormattedTextExtended text, DrawingContext dc, Point topCenter, bool forceBlackBrush)
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

        public static void DrawTextBottomRightAligned(this FormattedTextExtended text, DrawingContext dc, Point bottomRight, bool forceBlackBrush)
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

        public static double GetFullWidth(this FormattedTextExtended ft)
        {
            if (ft.Text.Length > 0)
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
            }
        }
        /*
        //double width = formattedTextExtended.WidthIncludingTrailingWhitespace;
        //if (formattedTextExtended.Text.Length > 0)
        //{
        //    if (!char.IsSeparator(formattedTextExtended.Text[0]))
        //    {
        //        if (formattedTextExtended.OverhangLeading < 0)
        //        {
        //            width -= formattedTextExtended.OverhangLeading;
        //        }
        //    }
        //    if (!char.IsSeparator(formattedTextExtended.Text[formattedTextExtended.Text.Length - 1]))
        //    {
        //        width -= formattedTextExtended.OverhangTrailing;
        //    }
        //}
        //return width;
        */

        public static double Descent(this FormattedTextExtended ft)
        {
            return ft.Height - ft.Baseline + ft.OverhangAfter;
        }

        public static double TopExtra(this FormattedTextExtended ft)
        {
            // = ft.Baseline - ft.Extent + ft.Descent() 
            // = ft.Baseline - ft.Extent + (ft.Height - ft.Baseline + ft.OverhangAfter)
            return ft.Height - ft.Extent + ft.OverhangAfter;
        }

        public static double GetRight(this FormattedTextExtended ft)
        {
            if (ft.Text.Length > 0)
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
            }
        }
    }
}
