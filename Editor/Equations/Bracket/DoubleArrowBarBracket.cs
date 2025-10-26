using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace Editor
{
    public sealed class DoubleArrowBarBracket : EquationContainer
    {
        private readonly RowContainer leftEquation;
        private readonly RowContainer rightEquation;
        private readonly BracketSign leftArrowSign;
        private readonly BracketSign rightArrowSign;
        private double ExtraHeight { get; set; }
        private double MidSpace { get; set; }

        public DoubleArrowBarBracket(MainWindow owner, EquationContainer parent)
            : base(owner, parent)
        {
            ExtraHeight = FontSize * 0.2;
            MidSpace = FontSize * 0.5;
            leftArrowSign = new BracketSign(owner, this, BracketSignType.LeftAngle);
            rightArrowSign = new BracketSign(owner, this, BracketSignType.RightAngle);
            ActiveChild = leftEquation = new RowContainer(owner, this);
            rightEquation = new RowContainer(owner, this);
            childEquations.AddRange([leftEquation, leftArrowSign, rightArrowSign, rightEquation]);
        }

        public override void DrawEquation(DrawingContext dc, bool forceBlackBrush)
        {
            base.DrawEquation(dc, forceBlackBrush);
            var pen = forceBlackBrush ? BlackThinPen : ThinPen;
            dc.DrawLine(pen, new Point(leftEquation.Right + MidSpace / 2, Top + ExtraHeight / 2), new Point(leftEquation.Right + MidSpace / 2, Bottom - ExtraHeight / 2));
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            thisElement.Add(leftEquation.Serialize());
            thisElement.Add(rightEquation.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            leftEquation.DeSerialize(xElement.Elements().First());
            rightEquation.DeSerialize(xElement.Elements().Last());
            CalculateSize();
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToDoubleArrowBarBracket(leftEquation.ToLatex(), rightEquation.ToLatex());
        }

        public override double Top
        {
            get => base.Top;
            set
            {
                base.Top = value;
                AdjustVertical();
            }
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                leftArrowSign.Left = value;
                leftEquation.Left = leftArrowSign.Right;
                rightEquation.Left = leftEquation.Right + MidSpace;
                rightArrowSign.Left = rightEquation.Right;
            }
        }

        public override double FontSize
        {
            get => base.FontSize;
            set
            {
                MidSpace = value * 0.5;
                base.FontSize = value;
            }
        }

        private void AdjustVertical()
        {
            leftEquation.MidY = MidY;
            rightEquation.MidY = MidY;
            leftArrowSign.Top = Top;// +ExtraHeight * .25;
            rightArrowSign.Top = Top;// leftArrowSign.Top;
        }

        public override void CalculateSize()
        {
            CalculateHeight();
            CalculateWidth();
        }

        protected override void CalculateWidth()
        {
            Width = leftEquation.Width + leftArrowSign.Width + rightArrowSign.Width + rightEquation.Width + MidSpace;
        }

        protected override void CalculateHeight()
        {
            Height = Math.Max(leftEquation.Height, rightEquation.Height) + ExtraHeight;
            leftArrowSign.Height = Height - ExtraHeight * 0.5;
            rightArrowSign.Height = leftArrowSign.Height;
        }

        public override double RefY => Math.Max(leftEquation.RefY, rightEquation.RefY);
    }
}
