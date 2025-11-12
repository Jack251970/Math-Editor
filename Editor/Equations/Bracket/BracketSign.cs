using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;

namespace Editor
{
    public sealed class BracketSign : EquationBase
    {
        public BracketSignType SignType { get; set; }
        private FormattedTextExtended? signText; //used by certain brackets
        private FormattedTextExtended? signText2; //used by certain brackets
        private FormattedTextExtended? midText; //for bigger curly brackets
        private FormattedTextExtended? extension; //for bigger curly brackets

        private double BracketBreakLimit => FontSize * 2.8;

        private readonly double leftPaddingFactor = 0.02;
        private readonly double rightPaddingFactor = 0.02;
        private double SignLeft => Left + LeftPadding;
        private double SignRight => Right - RightPadding;
        private double LeftPadding => FontSize * leftPaddingFactor;
        private double RightPadding => FontSize * rightPaddingFactor;

        public BracketSign(IMainWindow owner, EquationContainer parent, BracketSignType entityType)
            : base(owner, parent)
        {
            SignType = entityType;
            IsStatic = true;
            if (new[] { BracketSignType.LeftRound, BracketSignType.LeftCurly, BracketSignType.LeftAngle,
                BracketSignType.LeftCeiling, BracketSignType.LeftFloor, BracketSignType.LeftSquare,
                BracketSignType.LeftSquareBar }.Contains(entityType))
            {
                leftPaddingFactor = 0.02;
                rightPaddingFactor = 0;
            }
            else if (entityType is BracketSignType.LeftBar or BracketSignType.LeftDoubleBar or
                BracketSignType.RightBar or BracketSignType.RightDoubleBar)
            {
                leftPaddingFactor = 0.06;
                rightPaddingFactor = 0.06;
            }
        }

        private void CreateTextBrackets(bool forceBlackBrush)
        {
            switch (SignType)
            {
                case BracketSignType.LeftRound:
                case BracketSignType.RightRound:
                    CreateRoundTextBracket(forceBlackBrush);
                    break;
                case BracketSignType.LeftCurly:
                case BracketSignType.RightCurly:
                    CreateCurlyTextBracket(forceBlackBrush);
                    break;
            }
        }

        private void CreateRoundTextBracket(bool forceBlackBrush)
        {
            if (Height < FontSize * 1.2)
            {
                var signText = SignType == BracketSignType.LeftRound ? "(" : ")";
                FitSignToHeight(FontType.STIXGeneral, signText, forceBlackBrush);
            }
            if (Height < FontSize * 1.5)
            {
                var signText = SignType == BracketSignType.LeftRound ? "(" : ")";
                FitSignToHeight(FontType.STIXSizeOneSym, signText, forceBlackBrush);
            }
            else if (Height < FontSize * 1.9)
            {
                var signText = SignType == BracketSignType.LeftRound ? "(" : ")";
                FitSignToHeight(FontType.STIXSizeTwoSym, signText, forceBlackBrush);
            }
            else if (Height < FontSize * 2.5)
            {
                var signText = SignType == BracketSignType.LeftRound ? "(" : ")";
                FitSignToHeight(FontType.STIXSizeThreeSym, signText, forceBlackBrush);
            }
            else if (Height < BracketBreakLimit)
            {
                var signText = SignType == BracketSignType.LeftRound ? "(" : ")";
                FitSignToHeight(FontType.STIXSizeFourSym, signText, forceBlackBrush);
            }
            else
            {
                var text1 = SignType == BracketSignType.LeftRound ? "\u239b" : "\u239e";
                var text2 = SignType == BracketSignType.LeftRound ? "\u239d" : "\u23a0";
                var ext = SignType == BracketSignType.LeftRound ? "\u239c" : "\u239f";
                signText = FontFactory.GetFormattedTextExtended(text1, FontType.STIXSizeOneSym, FontSize * .5, forceBlackBrush);
                signText2 = FontFactory.GetFormattedTextExtended(text2, FontType.STIXSizeOneSym, FontSize * .5, forceBlackBrush);
                extension = FontFactory.GetFormattedTextExtended(ext, FontType.STIXSizeOneSym, FontSize * .5, forceBlackBrush);
            }
        }

        private void CreateCurlyTextBracket(bool forceBlackBrush)
        {
            if (Height < FontSize * 1.5)
            {
                var signText = SignType == BracketSignType.LeftCurly ? "{" : "}";
                FitSignToHeight(FontType.STIXSizeOneSym, signText, forceBlackBrush);
            }
            else if (Height < FontSize * 1.9)
            {
                var signText = SignType == BracketSignType.LeftCurly ? "{" : "}";
                FitSignToHeight(FontType.STIXSizeTwoSym, signText, forceBlackBrush);
            }
            else if (Height < FontSize * 2.5)
            {
                var signText = SignType == BracketSignType.LeftCurly ? "{" : "}";
                FitSignToHeight(FontType.STIXSizeThreeSym, signText, forceBlackBrush);
            }
            else if (Height < BracketBreakLimit)
            {
                var signText = SignType == BracketSignType.LeftCurly ? "{" : "}";
                FitSignToHeight(FontType.STIXSizeFourSym, signText, forceBlackBrush);
            }
            else
            {
                var text1 = SignType == BracketSignType.LeftCurly ? "\u23a7" : "\u23ab";
                var midtex = SignType == BracketSignType.LeftCurly ? "\u23a8" : "\u23ac";
                var text2 = SignType == BracketSignType.LeftCurly ? "\u23a9" : "\u23ad";
                signText = FontFactory.GetFormattedTextExtended(text1, FontType.STIXSizeOneSym, FontSize * .5, forceBlackBrush);
                midText = FontFactory.GetFormattedTextExtended(midtex, FontType.STIXSizeOneSym, FontSize * .5, forceBlackBrush);
                extension = FontFactory.GetFormattedTextExtended("\u23AA", FontType.STIXSizeOneSym, FontSize * .5, forceBlackBrush);
                signText2 = FontFactory.GetFormattedTextExtended(text2, FontType.STIXSizeOneSym, FontSize * .5, forceBlackBrush);
            }
        }

        private void FitSignToHeight(FontType fontType, string unicodeCharText, bool forceBlackBrush)
        {
            var factor = .4;
            do
            {
                signText = FontFactory.GetFormattedTextExtended(unicodeCharText, fontType, FontSize * factor, forceBlackBrush);
                factor += .02;
            }
            while (Height > signText.Extent);
        }

        public override double Height
        {
            get => base.Height;
            set
            {
                base.Height = value;
                if (SignType is BracketSignType.LeftRound or BracketSignType.RightRound or
                    BracketSignType.LeftCurly or BracketSignType.RightCurly
                    )
                {
                    CreateTextBrackets(false);
                }
                DetermineWidth();
            }
        }

        private void DetermineWidth()
        {
            var width = FontSize * .2;
            switch (SignType)
            {
                case BracketSignType.LeftRound:
                case BracketSignType.RightRound:
                    width = signText!.GetFullWidth();
                    break;
                case BracketSignType.LeftCurly:
                case BracketSignType.RightCurly:
                    if (Height < BracketBreakLimit)
                    {
                        width = signText!.GetFullWidth();
                    }
                    else
                    {
                        width = FontSize * .3;
                    }
                    break;
                case BracketSignType.LeftBar:
                case BracketSignType.RightBar:
                    width = ThinLineThickness + FontSize * 0.05;
                    break;
                case BracketSignType.LeftDoubleBar:
                case BracketSignType.RightDoubleBar:
                    width = ThinLineThickness * 2 + FontSize * 0.05;
                    break;
                case BracketSignType.LeftAngle:
                case BracketSignType.RightAngle:
                    width = FontSize * .12 + Height * 0.1;
                    break;
                case BracketSignType.LeftSquareBar:
                case BracketSignType.RightSquareBar:
                    width = LineThickness * 2 + FontSize * 0.15;
                    break;
            }
            Width = width + LeftPadding + RightPadding;
        }

        public override void DrawEquation(DrawingContext dc, bool forceBlackBrush)
        {
            //dc.DrawRectangle(Brushes.Yellow, null, Bounds);
            var pen = forceBlackBrush ? BlackThinPen : ThinPen;
            switch (SignType)
            {
                case BracketSignType.LeftAngle:
                    PaintLeftAngle(dc, forceBlackBrush);
                    break;
                case BracketSignType.RightAngle:
                    PaintRightAngle(dc, forceBlackBrush);
                    break;
                case BracketSignType.LeftBar:
                    dc.DrawLine(pen, new Point(SignLeft, Top), new Point(SignLeft, Bottom));
                    break;
                case BracketSignType.RightBar:
                    dc.DrawLine(pen, new Point(SignRight, Top), new Point(SignRight, Bottom));
                    //PaintVerticalBar(dc);
                    break;
                case BracketSignType.LeftCeiling:
                    PaintLeftCeiling(dc, forceBlackBrush);
                    break;
                case BracketSignType.RightCeiling:
                    PaintRightCeiling(dc, forceBlackBrush);
                    break;
                case BracketSignType.LeftCurly:
                case BracketSignType.RightCurly:
                    PaintCurly(dc, forceBlackBrush);
                    break;
                case BracketSignType.LeftDoubleBar:
                case BracketSignType.RightDoubleBar:
                    dc.DrawLine(pen, new Point(SignLeft, Top), new Point(SignLeft, Bottom));
                    dc.DrawLine(pen, new Point(SignRight, Top), new Point(SignRight, Bottom));
                    break;
                case BracketSignType.LeftFloor:
                    PaintLeftFloor(dc, forceBlackBrush);
                    break;
                case BracketSignType.RightFloor:
                    PaintRightFloor(dc, forceBlackBrush);
                    break;
                case BracketSignType.LeftRound:
                case BracketSignType.RightRound:
                    PaintRound(dc, forceBlackBrush);
                    break;
                case BracketSignType.LeftSquare:
                    PaintLeftSquare(dc, forceBlackBrush);
                    break;
                case BracketSignType.RightSquare:
                    PaintRightSquare(dc, forceBlackBrush);
                    break;
                case BracketSignType.LeftSquareBar:
                    PaintLeftSquareBar(dc, forceBlackBrush);
                    break;
                case BracketSignType.RightSquareBar:
                    PaintRightSquareBar(dc, forceBlackBrush);
                    break;
            }
        }

        private void PaintVerticalBar(DrawingContext dc, bool forceBlackBrush)
        {
            List<Point> points = [
                new Point(SignRight, Top),
                new Point(SignRight, Bottom),
                new Point(SignLeft, Bottom),
            ];
            dc.FillPolylineGeometry(new Point(SignLeft, Top), points, forceBlackBrush);
        }

        private void PaintLeftCeiling(DrawingContext dc, bool forceBlackBrush)
        {
            List<Point> points =
            [
                new Point(SignRight, Top),
                new Point(SignRight, Top + ThinLineThickness),
                new Point(SignLeft + ThinLineThickness, Top + ThinLineThickness),
                new Point(SignLeft + ThinLineThickness, Bottom),
                new Point(SignLeft, Bottom),
            ];
            dc.FillPolylineGeometry(new Point(SignLeft, Top), points, forceBlackBrush);
        }

        private void PaintRightCeiling(DrawingContext dc, bool forceBlackBrush)
        {
            List<Point> points =
            [
                new Point(SignRight, Top),
                new Point(SignRight, Bottom),
                new Point(SignRight - ThinLineThickness, Bottom),
                new Point(SignRight - ThinLineThickness, Top + ThinLineThickness),
                new Point(SignLeft, Top + ThinLineThickness),
            ];
            dc.FillPolylineGeometry(new Point(SignLeft, Top), points, forceBlackBrush);
        }

        private void PaintLeftFloor(DrawingContext dc, bool forceBlackBrush)
        {
            List<Point> points =
            [
                new Point(SignLeft + ThinLineThickness, Top),
                new Point(SignLeft + ThinLineThickness, Bottom - ThinLineThickness),
                new Point(SignRight, Bottom - ThinLineThickness),
                new Point(SignRight, Bottom),
                new Point(SignLeft, Bottom),
            ];
            dc.FillPolylineGeometry(new Point(SignLeft, Top), points, forceBlackBrush);
        }

        private void PaintRightFloor(DrawingContext dc, bool forceBlackBrush)
        {
            List<Point> points =
            [
                new Point(SignRight, Bottom),
                new Point(SignLeft, Bottom),
                new Point(SignLeft, Bottom - ThinLineThickness),
                new Point(SignRight - ThinLineThickness, Bottom - ThinLineThickness),
                new Point(SignRight - ThinLineThickness, Top),
            ];
            dc.FillPolylineGeometry(new Point(SignRight, Top), points, forceBlackBrush);
        }

        private void PaintLeftSquareBar(DrawingContext dc, bool forceBlackBrush)
        {
            var pen = forceBlackBrush ? BlackThinPen : ThinPen;
            List<Point> points =
            [
                new Point(SignRight, Top),
                new Point(SignRight, Top + ThinLineThickness),
                new Point(SignLeft + ThinLineThickness, Top + ThinLineThickness),
                new Point(SignLeft + ThinLineThickness, Bottom - ThinLineThickness),
                new Point(SignRight, Bottom - ThinLineThickness),
                new Point(SignRight, Bottom),
                new Point(SignLeft, Bottom),
            ];
            dc.FillPolylineGeometry(new Point(SignLeft, Top), points, forceBlackBrush);
            dc.DrawLine(pen, new Point(SignLeft + FontSize * .12, Top + ThinLineThickness * .5), new Point(SignLeft + FontSize * .12, Bottom - ThinLineThickness * .5));
        }

        private void PaintRightSquareBar(DrawingContext dc, bool forceBlackBrush)
        {
            var pen = forceBlackBrush ? BlackThinPen : ThinPen;
            List<Point> points =
            [
                new Point(SignRight, Top),
                new Point(SignRight, Bottom),
                new Point(SignLeft, Bottom),
                new Point(SignLeft, Bottom - ThinLineThickness),
                new Point(SignRight - ThinLineThickness, Bottom - ThinLineThickness),
                new Point(SignRight - ThinLineThickness, Top + ThinLineThickness),
                new Point(SignLeft, Top + ThinLineThickness),
            ];
            dc.FillPolylineGeometry(new Point(SignLeft, Top), points, forceBlackBrush);
            dc.DrawLine(pen, new Point(SignRight - FontSize * .12, Top + ThinLineThickness * .5), new Point(SignRight - FontSize * .12, Bottom - ThinLineThickness * .5));
        }

        private void PaintLeftSquare(DrawingContext dc, bool forceBlackBrush)
        {
            List<Point> points =
            [
                new Point(SignRight, Top),
                new Point(SignRight, Top + ThinLineThickness),
                new Point(SignLeft + LineThickness, Top + ThinLineThickness),
                new Point(SignLeft + LineThickness, Bottom - ThinLineThickness),
                new Point(SignRight, Bottom - ThinLineThickness),
                new Point(SignRight, Bottom),
                new Point(SignLeft, Bottom),
            ];
            dc.FillPolylineGeometry(new Point(SignLeft, Top), points, forceBlackBrush);
        }

        private void PaintRightSquare(DrawingContext dc, bool forceBlackBrush)
        {
            List<Point> points =
            [
                new Point(SignRight, Top),
                new Point(SignRight, Bottom),
                new Point(SignLeft, Bottom),
                new Point(SignLeft, Bottom - ThinLineThickness),
                new Point(SignRight - LineThickness, Bottom - ThinLineThickness),
                new Point(SignRight - LineThickness, Top + ThinLineThickness),
                new Point(SignLeft, Top + ThinLineThickness),
            ];
            dc.FillPolylineGeometry(new Point(SignLeft, Top), points, forceBlackBrush);
        }

        private void PaintRound(DrawingContext dc, bool forceBlackBrush)
        {
            if (Height < BracketBreakLimit)
            {
                if (SignType == BracketSignType.LeftRound)
                {
                    signText!.DrawTextTopLeftAligned(dc, new Point(SignLeft, Top), forceBlackBrush);
                }
                else
                {
                    signText!.DrawTextTopRightAligned(dc, new Point(SignRight, Top), forceBlackBrush);
                }
            }
            else
            {
                if (SignType == BracketSignType.LeftRound)
                {
                    var left = Math.Floor(SignLeft);
                    signText!.DrawTextTopLeftAligned(dc, new Point(left, Top), forceBlackBrush);
                    signText2!.DrawTextBottomLeftAligned(dc, new Point(left, Bottom), forceBlackBrush);
                    var top = Top + signText!.Extent * .9;
                    var bottom = Bottom - signText2!.Extent * .9;
                    //double topExtra = extension.Height + extension.OverhangAfter - extension.Extent;
                    var padding = extension!.OverhangLeading;
                    var geometry = extension.BuildGeometry(new Point(left - padding, 0));

                    List<Point> points =
                    [
                        new Point(geometry!.Bounds.Right, top),
                        new Point(geometry.Bounds.Right, bottom),
                        new Point(geometry.Bounds.Left, bottom),
                    ];
                    dc.FillPolylineGeometry(new Point(geometry.Bounds.Left, top), points, forceBlackBrush);

                    //Pen pen = PenManager.GetPen(extension.GetFullWidth() * .68);
                    //dc.DrawLine(pen, new Point(SignLeft + pen.Thickness * .68, top),
                    //                 new Point(SignLeft + pen.Thickness * .68, bottom));

                    ////double topExtra = extension.Height + extension.OverhangAfter - extension.Extent;
                    ////double padding = extension.OverhangLeading;
                    ////var geometry = extension.BuildGeometry(new Point(SignLeft - padding, top - topExtra));
                    ////Pen pen = new Pen(Brushes.Black, geometry.Bounds.Width);
                    ////dc.DrawLine(pen, new Point(SignLeft + pen.Thickness - padding * 1.2, top), 
                    ////                 new Point(SignLeft + LineThickness - padding * 1.2, bottom));
                    //var geometry2 = extension.BuildGeometry(new Point(SignLeft - 10, (top - topExtra)/2));
                    //var geometry3 = extension.BuildGeometry(new Point(SignLeft - 20, 10));
                    //double factor = (bottom - top) / (extension.Extent);
                    //ScaleTransform scale = new ScaleTransform(1.0, factor);
                    //scale.CenterY = extension.Extent / 2;
                    //scale.CenterY = geometry2.Bounds.Height;
                    //geometry2.Transform = scale;
                    //ScaleTransform scale2 = new ScaleTransform(1.0, 3);
                    //geometry3.Transform = scale2;                    
                    //dc.DrawGeometry(Brushes.Red, null, geometry);
                    //dc.DrawGeometry(Brushes.Blue, null, geometry2);                    
                    //dc.DrawGeometry(Brushes.Green, null, geometry3);
                    //var geo = Geometry.Combine(geometry, geometry, GeometryCombineMode.Intersect, scale);
                    //dc.DrawGeometry(Brushes.HotPink, null, geo);
                    //dc.PushTransform(scale);
                    //double topExtra = extension.Height + extension.OverhangAfter - extension.Extent;
                    //double padding = extension.OverhangLeading;
                    //dc.DrawText(extension, new Point(SignLeft - padding, top - (topExtra/factor)));
                    //dc.Pop();

                    //while (top < bottom)
                    //{
                    //    extension.DrawTextTopLeftAligned(dc, new Point(SignLeft, top));
                    //    top += extension.Extent *.85;
                    //    double shoot = (top + extension.Extent) - bottom;
                    //    if (shoot > 0)
                    //    {
                    //        top -= shoot;
                    //        extension.DrawTextTopLeftAligned(dc, new Point(SignLeft, top));
                    //        break;
                    //    }
                    //}
                }
                else
                {
                    signText!.DrawTextTopRightAligned(dc, new Point(SignRight, Top), forceBlackBrush);
                    signText2!.DrawTextBottomRightAligned(dc, new Point(SignRight, Bottom), forceBlackBrush);
                    var top = Top + signText!.Extent * .9;
                    var bottom = Bottom - signText2!.Extent * .9;
                    var geometry = extension!.BuildGeometry(new Point(SignRight - extension.GetFullWidth() - extension.OverhangLeading, 0));

                    List<Point> points =
                    [
                        new Point(geometry!.Bounds.Right, top),
                        new Point(geometry.Bounds.Right, bottom),
                        new Point(geometry.Bounds.Left, bottom),
                    ];
                    dc.FillPolylineGeometry(new Point(geometry.Bounds.Left, top), points, forceBlackBrush);
                    //double topExtra = extension.Height + extension.OverhangAfter - extension.Extent;
                    ////double padding = extension.OverhangLeading;
                    ////var geometry = extension.BuildGeometry(new Point(SignLeft, top));
                    ////Pen pen = new Pen(Brushes.Black, geometry.Bounds.Width);
                    ////dc.DrawLine(pen, new Point(SignRight - pen.Thickness * .65, top),
                    ////                 new Point(SignRight - pen.Thickness * .65, bottom));
                    //dc.DrawLine(new Pen(Brushes.Red, 1), new Point(SignLeft, Top), new Point(SignLeft, Bottom));
                    //Pen pen = PenManager.GetPen(extension.GetFullWidth() * .68);
                    //dc.DrawLine(pen, new Point(SignRight - pen.Thickness * .68, top),
                    //                 new Point(SignRight - pen.Thickness * .68, bottom));
                    //while (top < bottom)
                    //{
                    //    extension.DrawTextTopRightAligned(dc, new Point(left + signText.GetFullWidth(), top));
                    //    top += extension.Extent * .85;
                    //    double shoot = (top + extension.Extent) - bottom;
                    //    if (shoot > 0)
                    //    {
                    //        top -= shoot;
                    //        extension.DrawTextTopRightAligned(dc, new Point(left + signText.GetFullWidth(), top));
                    //        break;
                    //    }
                    //}
                }
            }
        }

        private void PaintCurly(DrawingContext dc, bool forceBlackBrush)
        {
            if (Height < BracketBreakLimit)
            {
                signText!.DrawTextTopLeftAligned(dc, new Point(SignLeft, Top), forceBlackBrush);
            }
            else
            {
                if (SignType == BracketSignType.LeftCurly)
                {
                    var left = SignLeft + midText!.GetFullWidth() - extension!.GetFullWidth();
                    signText!.DrawTextTopLeftAligned(dc, new Point(left, Top), forceBlackBrush);
                    //dc.DrawLine(new Pen(Brushes.Red, 1), new Point(left, Top), new Point(left, Bottom));
                    midText!.DrawTextTopLeftAligned(dc, new Point(SignLeft, MidY - midText!.Extent / 2), forceBlackBrush);
                    signText2!.DrawTextBottomLeftAligned(dc, new Point(left, Bottom), forceBlackBrush);
                    var top = Top + signText!.Extent * .9;
                    var bottom = MidY - midText.Extent * .4;

                    var padding = extension!.OverhangLeading;
                    var geometry = extension.BuildGeometry(new Point(left - padding, 0));

                    List<Point> points =
                    [
                        new Point(geometry!.Bounds.Right, top),
                        new Point(geometry.Bounds.Right, bottom),
                        new Point(geometry.Bounds.Left, bottom),
                    ];
                    dc.FillPolylineGeometry(new Point(geometry.Bounds.Left, top), points, forceBlackBrush);

                    //dc.DrawLine(new Pen(Brushes.Red, 1), new Point(Left, top), new Point(Right, top));
                    //dc.DrawLine(new Pen(Brushes.Red, 1), new Point(Left, bottom), new Point(Right, bottom));
                    //double padding = extension.OverhangLeading;
                    //double topExtra = extension.Height + extension.OverhangAfter - extension.Extent;
                    //var geometry = extension.BuildGeometry(new Point(SignLeft + midText.GetFullWidth(), top));
                    //double factor = ((bottom - top) / (extension.Extent)) * .95;
                    //ScaleTransform transform = new ScaleTransform(1, factor);
                    //transform.CenterY = extension.Extent / 2;
                    //geometry.Transform = transform;
                    //dc.DrawGeometry(Brushes.Red, null, geometry);
                    //dc.PushTransform(transform);
                    //Pen pen = new Pen(Brushes.Black, extension.GetFullWidth());
                    //dc.DrawText(extension, new Point(left - padding, (top/factor) - topExtra));
                    //dc.Pop();
                    //dc.DrawLine(pen, new Point(left + pen.Thickness * .65, top), new Point(left + pen.Thickness * .65, bottom));
                    //Pen pen = PenManager.GetPen(extension.GetFullWidth() * .68);
                    //dc.DrawLine(pen, new Point(left + pen.Thickness * .68, top),
                    //                 new Point(left + pen.Thickness * .68, bottom));
                    //while (top < bottom)
                    //{
                    //    extension.DrawTextTopLeftAligned(dc, new Point(left, top));
                    //    top += extension.Extent * .75;
                    //    double shoot = (top + extension.Extent * .8) - bottom;
                    //    if (shoot > 0)
                    //    {
                    //        top -= shoot;
                    //        extension.DrawTextTopLeftAligned(dc, new Point(left, top));
                    //        break;
                    //    }
                    //}
                    top = MidY + midText.Extent * .4;
                    bottom = Bottom - signText2!.Extent * .9;

                    points = [
                        new Point(geometry.Bounds.Right, top),
                        new Point(geometry.Bounds.Right, bottom),
                        new Point(geometry.Bounds.Left, bottom),
                    ];
                    dc.FillPolylineGeometry(new Point(geometry.Bounds.Left, top), points, forceBlackBrush);
                    //dc.DrawLine(pen, new Point(left + pen.Thickness - padding * 1.2, top),
                    //                 new Point(left + pen.Thickness - padding * 1.2, bottom));
                    //dc.DrawLine(pen, new Point(left + pen.Thickness * .68, top),
                    //                 new Point(left + pen.Thickness * .68, bottom));
                    //while (top < bottom)
                    //{
                    //    extension.DrawTextTopLeftAligned(dc, new Point(left, top));
                    //    top += extension.Extent * .75;
                    //    double shoot = (top + extension.Extent * .85) - bottom;
                    //    if (shoot > 0)
                    //    {
                    //        top -= shoot;
                    //        extension.DrawTextTopLeftAligned(dc, new Point(left, top));
                    //        break;
                    //    }
                    //}
                }
                else
                {
                    var left = SignLeft + signText!.GetFullWidth() - extension!.GetFullWidth();
                    signText!.DrawTextTopLeftAligned(dc, new Point(SignLeft, Top), forceBlackBrush);
                    midText!.DrawTextTopLeftAligned(dc, new Point(left, MidY - midText!.Extent / 2), forceBlackBrush);
                    signText2!.DrawTextBottomLeftAligned(dc, new Point(SignLeft, Bottom), forceBlackBrush);
                    var top = Top + signText!.Extent * .9;
                    var bottom = MidY - midText.Extent * .4;

                    var padding = extension!.OverhangLeading;
                    var geometry = extension.BuildGeometry(new Point(left - padding, 0));

                    List<Point> points =
                    [
                        new Point(geometry!.Bounds.Right, top),
                        new Point(geometry.Bounds.Right, bottom),
                        new Point(geometry.Bounds.Left, bottom),
                    ];
                    dc.FillPolylineGeometry(new Point(geometry.Bounds.Left, top), points, forceBlackBrush);

                    //double padding = extension.OverhangLeading;
                    //var geometry = extension.BuildGeometry(new Point(SignLeft, top));
                    //Pen pen = new Pen(Brushes.Black, geometry.Bounds.Width);
                    //dc.DrawLine(pen, new Point(left + pen.Thickness - padding * 1.2, top),
                    //                 new Point(left + pen.Thickness - padding * 1.2, bottom));
                    //Pen pen = PenManager.GetPen(extension.GetFullWidth() * .68);
                    //dc.DrawLine(pen, new Point(left + pen.Thickness * .68, top),
                    //                 new Point(left + pen.Thickness * .68, bottom));
                    //while (top < bottom)
                    //{
                    //    extension.DrawTextTopLeftAligned(dc, new Point(left, top));
                    //    top += extension.Extent * .75;
                    //    double shoot = (top + extension.Extent * .85) - bottom;
                    //    if (shoot > 0)
                    //    {
                    //        top -= shoot;
                    //        extension.DrawTextTopLeftAligned(dc, new Point(left, top));
                    //        break;
                    //    }
                    //}
                    top = MidY + midText.Extent * .4;
                    bottom = Bottom - signText2!.Extent * .9;
                    points = [
                        new Point(geometry.Bounds.Right, top),
                        new Point(geometry.Bounds.Right, bottom),
                        new Point(geometry.Bounds.Left, bottom),
                    ];
                    dc.FillPolylineGeometry(new Point(geometry.Bounds.Left, top), points, forceBlackBrush);
                    //dc.DrawLine(pen, new Point(left + pen.Thickness - padding * 1.2, top),
                    //                 new Point(left + pen.Thickness - padding * 1.2, bottom));
                    //dc.DrawLine(pen, new Point(left + pen.Thickness * .68, top),
                    //                 new Point(left + pen.Thickness * .68, bottom));

                    //while (top < bottom)
                    //{
                    //    extension.DrawTextTopLeftAligned(dc, new Point(left, top));
                    //    top += extension.Extent * .75;
                    //    double shoot = (top + extension.Extent * .85) - bottom;
                    //    if (shoot > 0)
                    //    {
                    //        top -= shoot;
                    //        extension.DrawTextTopLeftAligned(dc, new Point(left, top));
                    //        break;
                    //    }
                    //}
                }
            }
        }

        private void PaintLeftAngle(DrawingContext dc, bool forceBlackBrush)
        {
            var pen = forceBlackBrush ? BlackThinPen : ThinPen;
            List<Point> points =
            [
                new Point(SignLeft, MidY),
                new Point(SignRight, Bottom)
            ];
            dc.DrawPolyline(new Point(SignRight, Top), points, pen);
        }

        private void PaintRightAngle(DrawingContext dc, bool forceBlackBrush)
        {
            var pen = forceBlackBrush ? BlackThinPen : ThinPen;
            List<Point> points =
            [
                new Point(SignRight, MidY),
                new Point(SignLeft, Bottom)
            ];
            dc.DrawPolyline(new Point(SignLeft, Top), points, pen);
        }
    }
}
