using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Editor
{
    public class DivMath : EquationContainer
    {
        protected RowContainer _insideEquation;
        protected DivMathSign _divMathSign;
        protected double ExtraHeight => FontSize * .3;

        protected double VerticalGap => FontSize * .1;

        protected double LeftGap => FontSize * .1;

        public DivMath(EquationContainer parent)
            : base(parent)
        {
            _divMathSign = new DivMathSign(this);
            ActiveChild = _insideEquation = new RowContainer(this);
            childEquations.Add(_insideEquation);
            childEquations.Add(_divMathSign);
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            thisElement.Add(_insideEquation.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            _insideEquation.DeSerialize(xElement.Elements().First());
            CalculateSize();
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToDivision(DivisionType.DivMath, _insideEquation.ToLatex(), null);
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

        protected virtual void AdjustVertical()
        {
            _insideEquation.Top = Top + VerticalGap;
            _divMathSign.Top = Top + VerticalGap;
        }

        public override void CalculateSize()
        {
            CalculateHeight();
            CalculateWidth();
        }

        protected override void CalculateWidth()
        {
            Width = _insideEquation.Width + _divMathSign.Width + LeftGap;
        }

        protected override void CalculateHeight()
        {
            _divMathSign.Height = _insideEquation.FirstRow.Height + ExtraHeight;
            Height = Math.Max(_insideEquation.Height + ExtraHeight, _divMathSign.Height);
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                _divMathSign.Left = value + LeftGap;
                _insideEquation.Left = _divMathSign.Right;
            }
        }

        public override double RefY => _insideEquation.FirstRow.RefY + VerticalGap;
    }
}
