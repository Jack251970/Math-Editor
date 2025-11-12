using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace Editor
{
    public sealed class DecorationDrawing : EquationBase
    {
        private readonly DecorationType _decorationType;
        private FormattedTextExtended _firstSign = null!; //only used by certain decorations
        private FormattedTextExtended _secondSign = null!; //only used by certain decorations
        private FormattedTextExtended _bar = null!;

        public DecorationDrawing(IMainWindow owner, EquationContainer parent, DecorationType decorationType)
            : base(owner, parent)
        {
            _decorationType = decorationType;
            IsStatic = true;
            CreateDecorations(false);
            DetermineHeight(false);
        }

        private void CreateDecorations(bool forceBlackBrush)
        {
            switch (_decorationType)
            {
                case DecorationType.DoubleArrow:
                    _firstSign = FontFactory.GetFormattedTextExtended("\u02C2", FontType.STIXGeneral, FontSize * .7, forceBlackBrush);
                    _secondSign = FontFactory.GetFormattedTextExtended("\u02C3", FontType.STIXGeneral, FontSize * .7, forceBlackBrush);
                    break;
                case DecorationType.LeftArrow:
                    _firstSign = FontFactory.GetFormattedTextExtended("\u02C2", FontType.STIXGeneral, FontSize * .7, forceBlackBrush);
                    break;
                case DecorationType.RightArrow:
                    _firstSign = FontFactory.GetFormattedTextExtended("\u02C3", FontType.STIXGeneral, FontSize * .7, forceBlackBrush);
                    break;
                // It looks like it is unnecessary?
                //case DecorationType.RightHarpoonUpBarb:
                //case DecorationType.LeftHarpoonUpBarb:
                //case DecorationType.RightHarpoonDownBarb:
                //case DecorationType.LeftHarpoonDownBarb:
                //    _firstSign = FontFactory.GetFormattedTextExtended("\u21BC", FontType.STIXGeneral, FontSize);
                //    break;
                case DecorationType.Parenthesis:
                    CreateParenthesisSigns(forceBlackBrush);
                    break;
                case DecorationType.Tilde:
                    CreateTildeText(forceBlackBrush);
                    break;
            }
        }

        private void CreateParenthesisSigns(bool forceBlackBrush)
        {
            if (Width < FontSize * .8)
            {
                FitFirstSignToWidth(FontType.STIXGeneral, "\u23DC", FontWeight.Bold, forceBlackBrush);
            }
            else if (Width < FontSize * 2)
            {
                FitFirstSignToWidth(FontType.STIXSizeOneSym, "\u23DC", forceBlackBrush);
            }
            else if (Width < FontSize * 3)
            {
                FitFirstSignToWidth(FontType.STIXSizeTwoSym, "\u23DC", forceBlackBrush);
            }
            else
            {
                _firstSign = FontFactory.GetFormattedTextExtended("\uE142", FontType.STIXNonUnicode, FontSize * .55, forceBlackBrush);
                _secondSign = FontFactory.GetFormattedTextExtended("\uE143", FontType.STIXNonUnicode, FontSize * .55, forceBlackBrush);
                _bar = FontFactory.GetFormattedTextExtended("\uE14A", FontType.STIXNonUnicode, FontSize * .55, forceBlackBrush);
            }
        }

        public override double Width
        {
            get => base.Width;
            set
            {
                base.Width = value;
                if (_decorationType is DecorationType.Tilde or DecorationType.Parenthesis or
                    DecorationType.Hat)
                {
                    CreateDecorations(false);
                    DetermineHeight(false);
                }
            }
        }

        private void CreateTildeText(bool forceBlackBrush)
        {
            if (Width < FontSize / 2)
            {
                FitFirstSignToWidth(FontType.STIXGeneral, "\u0303", forceBlackBrush);
            }
            else if (Width < FontSize)
            {
                FitFirstSignToWidth(FontType.STIXSizeOneSym, "\u0303", forceBlackBrush);
            }
            else if (Width < FontSize * 2)
            {
                FitFirstSignToWidth(FontType.STIXSizeTwoSym, "\u0303", forceBlackBrush);
            }
            else if (Width < FontSize * 3)
            {
                FitFirstSignToWidth(FontType.STIXSizeThreeSym, "\u0303", forceBlackBrush);
            }
            else if (Width < FontSize * 4)
            {
                FitFirstSignToWidth(FontType.STIXSizeFourSym, "\u0303", forceBlackBrush);
            }
            else
            {
                FitFirstSignToWidth(FontType.STIXSizeFiveSym, "\u0303", forceBlackBrush);
            }
        }

        private void FitFirstSignToWidth(FontType fontType, string unicodeChar, bool forceBlackBrush)
        {
            FitFirstSignToWidth(fontType, unicodeChar, FontWeight.Normal, forceBlackBrush);
        }

        private void FitFirstSignToWidth(FontType fontType, string unicodeChar, FontWeight weight, bool forceBlackBrush)
        {
            var factor = .1;
            do
            {
                _firstSign = FontFactory.GetFormattedTextExtended(unicodeChar, fontType, FontSize * factor, forceBlackBrush);
                factor += .1;
            }
            while (Width > _firstSign.Width - _firstSign.OverhangLeading - _firstSign.OverhangTrailing);
        }

        protected override void CalculateHeight()
        {
            DetermineHeight(false);
        }

        public override double Left
        {
            get => base.Left; set => base.Left = Math.Floor(value) + .5;
        }
        public override double Top
        {
            get => base.Top; set => base.Top = Math.Floor(value) + .5;
        }

        public override double Bottom
        {
            get => Math.Floor(base.Bottom) + .5; set => base.Bottom = value;
        }

        public override double FontSize
        {
            get => base.FontSize;
            set
            {
                base.FontSize = value;
                CreateDecorations(false);
                DetermineHeight(false);
            }
        }

        private void DetermineHeight(bool forceBlackBrush)
        {
            var pen = forceBlackBrush ? BlackThinPen : ThinPen;
            switch (_decorationType)
            {
                case DecorationType.Cross:
                case DecorationType.LeftCross:
                case DecorationType.RightCross:
                case DecorationType.StrikeThrough:
                    Height = 0;
                    break;
                case DecorationType.Bar:
                    Height = pen.Thickness;
                    break;
                case DecorationType.DoubleBar:
                    Height = pen.Thickness * 2 + FontSize * .1;
                    break;
                case DecorationType.Hat:
                    Height = FontSize * .2 + Width * .02;
                    break;
                case DecorationType.LeftArrow:
                case DecorationType.RightArrow:
                case DecorationType.DoubleArrow:
                case DecorationType.Parenthesis:
                case DecorationType.Tilde:
                    Height = _firstSign.Extent;
                    break;
                case DecorationType.RightHarpoonUpBarb:
                case DecorationType.LeftHarpoonUpBarb:
                case DecorationType.RightHarpoonDownBarb:
                case DecorationType.LeftHarpoonDownBarb:
                    Height = FontSize * .2;
                    break;
                case DecorationType.Tortoise:
                    if (Width > FontSize * .333)
                    {
                        Height = FontSize * .25;
                    }
                    else
                    {
                        Height = FontSize * .2;
                    }
                    break;
            }
        }

        public override void DrawEquation(DrawingContext dc, bool forceBlackBrush)
        {
            var pen = forceBlackBrush ? BlackThinPen : ThinPen;
            switch (_decorationType)
            {
                case DecorationType.Bar:
                    dc.DrawLine(pen, Location, new Point(Right, Top));
                    break;
                case DecorationType.DoubleBar:
                    dc.DrawLine(pen, Location, new Point(Right, Top));
                    dc.DrawLine(pen,
                        new Point(Left, Bottom - pen.Thickness), new Point(Right, Bottom - pen.Thickness));
                    break;
                case DecorationType.Hat:
                    dc.DrawPolyline(new Point(Left, Bottom - FontSize * .02),
                        [
                            new Point(MidX, Top + FontSize * .03),
                            new Point(Right, Bottom - FontSize * .02)
                        ],
                        pen);
                    break;
                case DecorationType.LeftArrow:
                    _firstSign.DrawTextTopLeftAligned(dc, Location, forceBlackBrush);
                    dc.DrawLine(pen,
                        new Point(Left + FontSize * .06, MidY), new Point(Right, MidY));
                    break;
                case DecorationType.RightArrow:
                    _firstSign.DrawTextTopRightAligned(dc, new Point(Right, Top), forceBlackBrush);
                    dc.DrawLine(pen,
                        new Point(Left, MidY), new Point(Right - FontSize * .06, MidY));
                    break;
                case DecorationType.DoubleArrow:
                    DrawDoubleArrow(dc, forceBlackBrush);
                    break;
                case DecorationType.Parenthesis:
                    DrawParentheses(dc, forceBlackBrush);
                    break;
                case DecorationType.RightHarpoonUpBarb:
                    DrawRightHarpoonUpBarb(dc, forceBlackBrush);
                    break;
                case DecorationType.RightHarpoonDownBarb:
                    DrawRightHarpoonDownBarb(dc, forceBlackBrush);
                    break;
                case DecorationType.LeftHarpoonUpBarb:
                    DrawLeftHarpoonUpBarb(dc, forceBlackBrush);
                    break;
                case DecorationType.LeftHarpoonDownBarb:
                    DrawLeftHarpoonDownBarb(dc, forceBlackBrush);
                    break;
                case DecorationType.Tilde:
                    _firstSign.DrawTextTopLeftAligned(dc, Location, forceBlackBrush);
                    break;
                case DecorationType.Tortoise:
                    DrawTortoise(dc, forceBlackBrush);
                    break;
                case DecorationType.Cross:
                    dc.DrawLine(pen, ParentEquation.Location, new Point(Right, ParentEquation.Bottom));
                    dc.DrawLine(pen, new Point(Left, ParentEquation.Bottom), new Point(Right, ParentEquation.Top));
                    break;
                case DecorationType.LeftCross:
                    dc.DrawLine(pen, ParentEquation.Location, new Point(Right, ParentEquation.Bottom));
                    break;
                case DecorationType.RightCross:
                    dc.DrawLine(pen, new Point(Left, ParentEquation.Bottom), new Point(Right, ParentEquation.Top));
                    break;
                case DecorationType.StrikeThrough:
                    dc.DrawLine(pen, new Point(Left, ParentEquation.MidY), new Point(Right, ParentEquation.MidY));
                    break;
            }
        }

        private void DrawDoubleArrow(DrawingContext dc, bool forceBlackBrush)
        {
            var pen = forceBlackBrush ? BlackThinPen : ThinPen;
            if (Width < FontSize * 0.8)
            {
                var text = FontFactory.GetFormattedTextExtended("\u2194", FontType.STIXGeneral, Width * 1.5, forceBlackBrush);
                var factor = .1;
                do
                {
                    text = FontFactory.GetFormattedTextExtended("\u2194", FontType.STIXGeneral, FontSize * factor, forceBlackBrush);
                    factor += .1;
                }
                while (Width > text.GetFullWidth());
                text.DrawTextTopLeftAligned(dc, Location, forceBlackBrush);
            }
            else
            {
                _firstSign.DrawTextTopLeftAligned(dc, Location, forceBlackBrush);
                _secondSign.DrawTextTopRightAligned(dc, new Point(Right, Top), forceBlackBrush);
                dc.DrawLine(pen, new Point(Left + FontSize * .06, MidY), new Point(Right - FontSize * .06, MidY));
            }
        }

        private void DrawParentheses(DrawingContext dc, bool forceBlackBrush)
        {
            if (Width < FontSize * 3)
            {
                _firstSign.DrawTextTopLeftAligned(dc, Location, forceBlackBrush);
            }
            else
            {
                _firstSign.DrawTextTopLeftAligned(dc, Location, forceBlackBrush);
                _secondSign.DrawTextTopLeftAligned(dc, new Point(Right - _secondSign.Width + _secondSign.OverhangLeading, Top), forceBlackBrush);
                //dc.DrawLine(StandardPen, new Point(Left + secondSign.Width + secondSign.OverhangLeading, Top + FontSize * .03), new Point(Right - (secondSign.Width + secondSign.OverhangLeading), Top + FontSize * .03));
                var left = Left + _firstSign.GetFullWidth() * .85;
                var right = Right - _secondSign.GetFullWidth() * .85;
                while (left < right)
                {
                    _bar.DrawTextTopLeftAligned(dc, new Point(left, Top), forceBlackBrush);
                    left += _bar.GetFullWidth() * .8;
                    var shoot = (left + _bar.GetFullWidth() * .8) - right;
                    if (shoot > 0)
                    {
                        left -= shoot;
                        _bar.DrawTextTopLeftAligned(dc, new Point(left, Top), forceBlackBrush);
                        break;
                    }
                }
            }
        }

        private void DrawLeftHarpoonUpBarb(DrawingContext dc, bool forceBlackBrush)
        {
            List<Point> points =
            [
                new Point(Left + FontSize * .3, Top),
                //new Point(Left + FontSize * .31, Top + FontSize * .041),
                new Point(Left + FontSize * .18, Bottom - FontSize * .06),
                new Point(Right, Bottom - FontSize * .06),
                new Point(Right, Bottom- FontSize * .02)
            ];
            dc.FillPolylineGeometry(new Point(Left, Bottom - FontSize * .02), points, forceBlackBrush);
        }

        private void DrawRightHarpoonUpBarb(DrawingContext dc, bool forceBlackBrush)
        {
            List<Point> points =
            [
                new Point(Right - FontSize * .3, Top),
                //new Point(Right - FontSize * .31, Top + FontSize * .041),
                new Point(Right - FontSize * .18, Bottom - FontSize * .06),
                new Point(Left, Bottom - FontSize * .06),
                new Point(Left, Bottom - FontSize * .02)
            ];
            dc.FillPolylineGeometry(new Point(Right, Bottom - FontSize * .02), points, forceBlackBrush);
        }

        private void DrawLeftHarpoonDownBarb(DrawingContext dc, bool forceBlackBrush)
        {
            List<Point> points =
            [
                new Point(Left + FontSize * .3, Bottom),
                //new Point(Left + FontSize * .31, Bottom - FontSize * .041),
                new Point(Left + FontSize * .18, Top + FontSize * .06),
                new Point(Right, Top + FontSize * .06),
                new Point(Right, Top + FontSize * .02)
            ];
            dc.FillPolylineGeometry(new Point(Left, Top + FontSize * .02), points, forceBlackBrush);
        }

        private void DrawRightHarpoonDownBarb(DrawingContext dc, bool forceBlackBrush)
        {
            List<Point> points =
            [
                new Point(Right - FontSize * .3, Bottom),
                //new Point(Right - FontSize * .31, Bottom - FontSize * .041),
                new Point(Right - FontSize * .18, Top + FontSize * .06),
                new Point(Left, Top + FontSize * .06),
                new Point(Left, Top + FontSize * .02)
            ];
            dc.FillPolylineGeometry(new Point(Right, Top + FontSize * .02), points, forceBlackBrush);
        }

        private void DrawTortoise(DrawingContext dc, bool forceBlackBrush)
        {
            List<Point> points =
            [
                new Point(Left + Height * .5, Top),
                new Point(Right - Height * .5, Top),
                new Point(Right, Bottom),
                new Point(Right - Height * .2, Bottom),
                new Point(Right - Height * .7, Top + Height * .3),
                new Point(Left + Height * .7, Top + Height * .3),
                new Point(Left + Height * .2, Bottom)
            ];
            dc.FillPolylineGeometry(new Point(Left, Bottom), points, forceBlackBrush);
        }
    }
}
