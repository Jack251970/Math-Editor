using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Editor
{
    public class DivHorizontal : DivBase
    {
        private double ExtraWidth => FontSize * .3;

        public DivHorizontal(MainWindow owner, EquationContainer parent)
            : base(owner, parent, false)
        {
        }

        public DivHorizontal(MainWindow owner, EquationContainer parent, bool isSmall)
            : base(owner, parent, isSmall)
        {
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToDivision(DivisionType.DivHoriz, _topEquation.ToLatex(), _bottomEquation.ToLatex());
        }

        public override void DrawEquation(DrawingContext dc)
        {
            base.DrawEquation(dc);
            dc.DrawLine(StandardPen, new Point(_bottomEquation.Left - ExtraWidth / 10, Top), new Point(_topEquation.Right + ExtraWidth / 10, Bottom));
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                AdjustHorizontal();
            }
        }

        private void AdjustHorizontal()
        {

            _topEquation.Left = this.Left;
            _bottomEquation.Left = _topEquation.Right + ExtraWidth;
        }
        public override double Top
        {
            get => base.Top;
            set
            {
                base.Top = value;
                _topEquation.MidY = MidY;
                _bottomEquation.MidY = MidY;
            }
        }

        public override double RefY => Math.Max(_topEquation.RefY, _bottomEquation.RefY);

        protected override void CalculateWidth()
        {
            Width = _topEquation.Width + _bottomEquation.Width + ExtraWidth;
            AdjustHorizontal();
        }

        protected override void CalculateHeight()
        {
            Height = Math.Max(_topEquation.Height, _bottomEquation.Height);
        }

        public override bool ConsumeKey(Key key)
        {
            if (ActiveChild.ConsumeKey(key))
            {
                CalculateSize();
                return true;
            }
            if (key == Key.Right)
            {
                if (ActiveChild == _topEquation)
                {
                    ActiveChild = _bottomEquation;
                    return true;
                }
            }
            else if (key == Key.Left)
            {
                if (ActiveChild == _bottomEquation)
                {
                    ActiveChild = _topEquation;
                    return true;
                }
            }
            return false;
        }
    }

    public sealed class DivHorizSmall : DivHorizontal
    {
        public DivHorizSmall(MainWindow owner, EquationContainer parent)
            : base(owner, parent, true)
        {
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToDivision(DivisionType.DivHorizSmall, _topEquation.ToLatex(), _bottomEquation.ToLatex());
        }
    }
}
