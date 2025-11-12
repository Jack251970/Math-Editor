using System;
using System.Text;
using Avalonia;
using Avalonia.Media;

namespace Editor
{
    public class DivRegular : DivBase
    {
        private double ExtraWidth => FontSize * .2;
        private double ExtraHeight => FontSize * .2;

        protected int barCount = 1;

        public DivRegular(IMainWindow owner, EquationContainer parent)
            : base(owner, parent, false)
        {
        }

        public DivRegular(IMainWindow owner, EquationContainer parent, bool isSmall)
            : base(owner, parent, isSmall)
        {
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToDivision(DivisionType.DivRegular, _topEquation.ToLatex(), _bottomEquation.ToLatex());
        }

        public override void DrawEquation(DrawingContext dc, bool forceBlackBrush)
        {
            base.DrawEquation(dc, forceBlackBrush);
            var pen = forceBlackBrush ? BlackStandardPen : StandardPen;
            if (barCount == 1)
            {
                dc.DrawLine(pen, new Point(Left + ExtraWidth * .5, MidY), new Point(Right - ExtraWidth * .5, MidY));
            }
            else if (barCount == 2)
            {
                dc.DrawLine(pen, new Point(Left + ExtraWidth * .5, MidY - ExtraHeight / 2), new Point(Right - ExtraWidth * .5, MidY - ExtraHeight / 2));
                dc.DrawLine(pen, new Point(Left + ExtraWidth * .5, MidY + ExtraHeight / 2), new Point(Right - ExtraWidth * .5, MidY + ExtraHeight / 2));
            }
            if (barCount == 3)
            {
                dc.DrawLine(pen, new Point(Left + ExtraWidth * .5, MidY), new Point(Right - ExtraWidth * .5, MidY));
                dc.DrawLine(pen, new Point(Left + ExtraWidth * .5, MidY - ExtraHeight), new Point(Right - ExtraWidth * .5, MidY - ExtraHeight));
                dc.DrawLine(pen, new Point(Left + ExtraWidth * .5, MidY + ExtraHeight), new Point(Right - ExtraWidth * .5, MidY + ExtraHeight));
            }
            //dc.DrawLine(new Pen(Brushes.Purple, 1), new Point(Left, MidY), new Point(Right, MidY));            
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                _topEquation.MidX = MidX;
                _bottomEquation.MidX = MidX;
            }
        }

        public override double Top
        {
            get => base.Top;
            set
            {
                base.Top = value;
                _topEquation.Top = value;
                _bottomEquation.Bottom = Bottom;
            }
        }

        protected override void CalculateWidth()
        {
            Width = Math.Max(_topEquation.Width, _bottomEquation.Width) + ExtraWidth;
        }

        protected override void CalculateHeight()
        {
            var height = _topEquation.Height + _bottomEquation.Height + ExtraHeight * 1.6;
            height += (barCount - 1) * ExtraHeight;
            Height = height;
        }

        public override double RefY => _topEquation.Height + ExtraHeight * ((barCount + 1.0) / 2);
    }
}
