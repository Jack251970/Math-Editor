using System.Windows;
using System.Windows.Media;

namespace Editor
{
    public sealed class HorizontalBracketSign : EquationBase
    {
        public HorizontalBracketSignType SignType { get; private set; }
        private FormattedText sign = null!;
        private FormattedText leftCurlyPart = null!;
        private FormattedText rightCurlyPart = null!;

        public HorizontalBracketSign(MainWindow owner, EquationContainer parent, HorizontalBracketSignType signType)
            : base(owner, parent)
        {
            SignType = signType;
            IsStatic = true;
        }

        public override void DrawEquation(DrawingContext dc, bool forceBlackBrush)
        {
            switch (SignType)
            {
                case HorizontalBracketSignType.TopCurly:
                    DrawTopCurly(dc, forceBlackBrush);
                    break;
                case HorizontalBracketSignType.BottomCurly:
                    DrawBottomCurly(dc, forceBlackBrush);
                    break;
                case HorizontalBracketSignType.TopSquare:
                    DrawTopSquare(dc, forceBlackBrush);
                    break;
                case HorizontalBracketSignType.BottomSquare:
                    DrawBottomSquare(dc, forceBlackBrush);
                    break;
            }
        }

        private void DrawTopSquare(DrawingContext dc, bool forceBlackBrush)
        {
            PointCollection points =
            [
                new Point(Left, Top),
                new Point(Right, Top),
                new Point(Right, Bottom),
                new Point(Right - ThinLineThickness, Bottom),
                new Point(Right - ThinLineThickness, Top + LineThickness),
                new Point(Left + ThinLineThickness, Top + LineThickness),
                new Point(Left + ThinLineThickness, Bottom)
            ];
            dc.FillPolylineGeometry(new Point(Left, Bottom), points, forceBlackBrush);
        }

        private void DrawBottomSquare(DrawingContext dc, bool forceBlackBrush)
        {
            PointCollection points =
            [
                new Point(Left, Top),
                new Point(Left + ThinLineThickness, Top),
                new Point(Left + ThinLineThickness, Bottom - LineThickness),
                new Point(Right - ThinLineThickness, Bottom - LineThickness),
                new Point(Right - ThinLineThickness, Top),
                new Point(Right, Top),
                new Point(Right, Bottom)
            ];
            dc.FillPolylineGeometry(new Point(Left, Bottom), points, forceBlackBrush);
        }

        private void DrawBottomCurly(DrawingContext dc, bool forceBlackBrush)
        {
            if (Width < FontSize * 5)
            {
                sign.DrawTextTopLeftAligned(dc, Location, forceBlackBrush);
            }
            else
            {
                var line = FontFactory.GetFormattedText("\uE14B", FontType.STIXNonUnicode, FontSize * .55, forceBlackBrush);
                leftCurlyPart.DrawTextTopLeftAligned(dc, Location, forceBlackBrush);
                sign.DrawTextTopLeftAligned(dc, new Point(MidX - sign.GetFullWidth() * .5, Top + leftCurlyPart.Extent - line.Extent), forceBlackBrush);
                rightCurlyPart.DrawTextTopLeftAligned(dc, new Point(Right - rightCurlyPart.GetFullWidth(), Top), forceBlackBrush);
                var left = Left + leftCurlyPart.GetFullWidth() * .85;
                var right = MidX - sign.GetFullWidth() * .4;
                while (left < right)
                {
                    line.DrawTextTopLeftAligned(dc, new Point(left, Top + leftCurlyPart.Extent - line.Extent), forceBlackBrush);
                    left += line.GetFullWidth() * .8;
                    var shoot = (left + line.GetFullWidth() * .8) - right;
                    if (shoot > 0)
                    {
                        left -= shoot;
                        line.DrawTextTopLeftAligned(dc, new Point(left, Top + leftCurlyPart.Extent - line.Extent), forceBlackBrush);
                        break;
                    }
                }
                left = MidX + sign.GetFullWidth() * .4;
                right = Right - rightCurlyPart.GetFullWidth() * .8;
                while (left < right)
                {
                    line.DrawTextTopLeftAligned(dc, new Point(left, Top + leftCurlyPart.Extent - line.Extent), forceBlackBrush);
                    left += line.GetFullWidth() * .8;
                    var shoot = (left + line.GetFullWidth() * .8) - right;
                    if (shoot > 0)
                    {
                        left -= shoot;
                        line.DrawTextTopLeftAligned(dc, new Point(left, Top + leftCurlyPart.Extent - line.Extent), forceBlackBrush);
                        break;
                    }
                }

                //dc.DrawLine(StandardPen, new Point(Left + leftCurlyPart.GetFullWidth() * .96, Top + leftCurlyPart.Extent - LineThickness * .5),
                //                         new Point(MidX - sign.GetFullWidth() * .48, Top + leftCurlyPart.Extent - LineThickness * .5));
                //dc.DrawLine(StandardPen, new Point(MidX + sign.GetFullWidth() * .48, Top + rightCurlyPart.Extent - LineThickness * .5),
                //                         new Point(Right - rightCurlyPart.GetFullWidth() * .96, Top + rightCurlyPart.Extent - LineThickness * .5));
            }
        }

        private void DrawTopCurly(DrawingContext dc, bool forceBlackBrush)
        {
            if (Width < FontSize * 5)
            {
                sign.DrawTextTopLeftAligned(dc, Location, forceBlackBrush);
            }
            else
            {
                var extension = FontFactory.GetFormattedText("\uE14A", FontType.STIXNonUnicode, FontSize * .55, forceBlackBrush);
                //dc.DrawLine(new Pen(Brushes.Red, 1), Location, new Point(Right, Top));
                leftCurlyPart.DrawTextTopLeftAligned(dc, new Point(Left, Top + sign.Extent - extension.Extent), forceBlackBrush);
                sign.DrawTextTopLeftAligned(dc, new Point(MidX - sign.GetFullWidth() * .5, Top), forceBlackBrush);
                rightCurlyPart.DrawTextTopLeftAligned(dc, new Point(Right - rightCurlyPart.GetFullWidth(), Top + sign.Extent - extension.Extent), forceBlackBrush);
                var left = Left + leftCurlyPart.GetFullWidth() * .9;
                var right = MidX - sign.GetFullWidth() * .4;
                //var geometry = extension.BuildGeometry(new Point(0, Top + sign.Extent - extension.Height - extension.OverhangAfter));
                //PointCollection points = new PointCollection { new Point(right, geometry.Bounds.Top),
                //                                                   new Point(right, geometry.Bounds.Bottom),
                //                                                   new Point(left, geometry.Bounds.Bottom),
                //                                                 };
                //dc.FillPolylineGeometry(new Point(left, geometry.Bounds.Top), points);                
                while (left < right)
                {
                    extension.DrawTextTopLeftAligned(dc, new Point(left, Top + sign.Extent - extension.Extent), forceBlackBrush);
                    left += extension.GetFullWidth() * .8;
                    var shoot = (left + extension.GetFullWidth() * .8) - right;
                    if (shoot > 0)
                    {
                        left -= shoot;
                        extension.DrawTextTopLeftAligned(dc, new Point(left, Top + sign.Extent - extension.Extent), forceBlackBrush);
                        break;
                    }
                }
                left = MidX + sign.GetFullWidth() * .4;
                right = Right - rightCurlyPart.GetFullWidth() * .8;
                //points = new PointCollection { new Point(right, geometry.Bounds.Top),
                //                                                   new Point(right, geometry.Bounds.Bottom),
                //                                                   new Point(left, geometry.Bounds.Bottom),
                //                                                 };
                //dc.FillPolylineGeometry(new Point(left, geometry.Bounds.Top), points);
                while (left < right)
                {
                    extension.DrawTextTopLeftAligned(dc, new Point(left, Top + sign.Extent - extension.Extent), forceBlackBrush);
                    left += extension.GetFullWidth() * .8;
                    var shoot = (left + extension.GetFullWidth() * .8) - right;
                    if (shoot > 0)
                    {
                        left -= shoot;
                        extension.DrawTextTopLeftAligned(dc, new Point(left, Top + sign.Extent - extension.Extent), forceBlackBrush);
                        break;
                    }
                }
            }
        }

        public override double Width
        {
            get => base.Width;
            set
            {
                base.Width = value;
                AdjustHeight();
            }
        }

        private void AdjustHeight()
        {
            if (SignType is HorizontalBracketSignType.BottomSquare or HorizontalBracketSignType.TopSquare)
            {
                Height = FontSize * .3;
            }
            else if (Width < FontSize * 5)
            {
                CreateSingleCharacterCurlySign(false);
            }
            else
            {
                if (SignType == HorizontalBracketSignType.BottomCurly)
                {
                    CreateBrokenCurlyBottom(false);
                }
                else
                {
                    CreateBrokenCurlyTop(false);
                }
                Height = FontSize * .5;
            }
        }

        private void CreateBrokenCurlyTop(bool forceBlackBrush)
        {
            var fontSize = FontSize * .55;
            leftCurlyPart = FontFactory.GetFormattedText("\uE13B", FontType.STIXNonUnicode, fontSize, forceBlackBrush); //Top left of overbrace 
            sign = FontFactory.GetFormattedText("\uE140", FontType.STIXNonUnicode, fontSize, forceBlackBrush); //middle of overbrace
            rightCurlyPart = FontFactory.GetFormattedText("\uE13C", FontType.STIXNonUnicode, fontSize, forceBlackBrush); //Top right of overbrace             
        }

        private void CreateBrokenCurlyBottom(bool forceBlackBrush)
        {
            var fontSize = FontSize * .55;
            leftCurlyPart = FontFactory.GetFormattedText("\uE13D", FontType.STIXNonUnicode, fontSize, forceBlackBrush); //Top left of overbrace 
            sign = FontFactory.GetFormattedText("\uE141", FontType.STIXNonUnicode, fontSize, forceBlackBrush); //middle of overbrace
            rightCurlyPart = FontFactory.GetFormattedText("\uE13E", FontType.STIXNonUnicode, fontSize, forceBlackBrush); //Top right of overbrace
        }

        private void CreateSingleCharacterCurlySign(bool forceBlackBrush)
        {
            var signStr = SignType == HorizontalBracketSignType.TopCurly ? "\u23DE" : "\u23DF";
            FontType fontType;
            if (Width < FontSize)
            {
                fontType = FontType.STIXSizeOneSym;
            }
            else if (Width < FontSize * 2)
            {
                fontType = FontType.STIXSizeTwoSym;
            }
            else if (Width < FontSize * 3)
            {
                fontType = FontType.STIXSizeThreeSym;
            }
            else if (Width < FontSize * 4)
            {
                fontType = FontType.STIXSizeFourSym;
            }
            else
            {
                fontType = FontType.STIXSizeFiveSym;
            }
            double fontSize = 4;
            do
            {
                sign = FontFactory.GetFormattedText(signStr, fontType, fontSize++, forceBlackBrush);
            }
            while (sign.Width < Width);
            Height = sign.Extent * 1.1;
        }
    }
}
