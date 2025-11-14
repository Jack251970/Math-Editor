using System;
using System.Text;
using Avalonia;
using Avalonia.Media;

namespace Editor
{
    public class DivSlanted : DivBase
    {
        private double ExtraWidth => FontSize * .1;
        private double ExtraHeight => FontSize * .1;

        private double centerX;
        private double slantXTop;
        private double slantXBottom;

        public DivSlanted(IMainWindow owner, EquationContainer parent)
            : base(owner, parent, false)
        {
        }

        public DivSlanted(IMainWindow owner, EquationContainer parent, bool isSmall)
            : base(owner, parent, isSmall)
        {
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToDivision(DivisionType.DivSlanted, _topEquation.ToLatex(), _bottomEquation.ToLatex());
        }

        public override void DrawEquation(DrawingContext dc, bool forceBlackBrush)
        {
            base.DrawEquation(dc, forceBlackBrush);
            var pen = forceBlackBrush ? BlackStandardPen : StandardPen;
            dc.DrawLine(pen, new Point(Left + centerX + slantXTop, Top), new Point(Left + centerX - slantXBottom, Bottom));
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                _topEquation.Right = Left + centerX - ExtraWidth / 2;
                _bottomEquation.Left = Left + centerX + ExtraWidth / 2;
            }
        }
        public override double Top
        {
            get => base.Top;
            set
            {
                base.Top = value;
                _topEquation.Top = Top;
                _bottomEquation.Bottom = Bottom;
            }
        }

        public override void CalculateSize()
        {
            CalculateHeight();
            CalculateWidth();
        }

        protected override void CalculateWidth()
        {
            var width = _topEquation.Width + _bottomEquation.Width + ExtraWidth;
            var rect = new Rect(0, 0, width, Height);
            slantXTop = Math.Sin(Math.PI / 5) * (_topEquation.Height + ExtraHeight / 2);
            slantXBottom = Math.Sin(Math.PI / 5) * (_bottomEquation.Height + ExtraHeight / 2);
            rect = rect.Union(new Point(_topEquation.Width + slantXTop + ExtraWidth / 2, Top));
            rect = rect.Union(new Point(_bottomEquation.Width + slantXBottom + ExtraWidth / 2, Bottom));
            Width = rect.Width;
            centerX = rect.Width - Math.Max(slantXTop, _bottomEquation.Width) - ExtraWidth / 2;
        }

        protected override void CalculateHeight()
        {
            Height = _topEquation.Height + _bottomEquation.Height + ExtraHeight;
        }

        //public override double RefY
        //{
        //    get
        //    {
        //        return topEquation.Height + ExtraHeight / 2;
        //    }
        //}
    }

    public sealed class DivSlantedSmall : DivSlanted
    {
        public DivSlantedSmall(IMainWindow owner, EquationContainer parent)
            : base(owner, parent, true)
        {
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToDivision(DivisionType.DivSlantedSmall, _topEquation.ToLatex(), _bottomEquation.ToLatex());
        }
    }
}
