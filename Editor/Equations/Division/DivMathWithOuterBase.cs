using System;
using System.Linq;
using System.Xml.Linq;

namespace Editor
{
    public abstract class DivMathWithOuterBase : DivMath
    {
        protected RowContainer outerEquation;

        public DivMathWithOuterBase(EquationContainer parent)
            : base(parent)
        {
            outerEquation = new RowContainer(this)
            {
                HAlignment = HAlignment.Right
            };
            //insideEquation.HAlignment = Editor.HAlignment.Right;
            childEquations.Add(outerEquation);
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            thisElement.Add(_insideEquation.Serialize());
            thisElement.Add(outerEquation.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            var elements = xElement.Elements().ToArray();
            _insideEquation.DeSerialize(elements[0]);
            outerEquation.DeSerialize(elements[1]);
            CalculateSize();
        }

        public override void CalculateSize()
        {
            CalculateHeight();
            CalculateWidth();
        }

        protected override void CalculateWidth()
        {
            Width = Math.Max(_insideEquation.Width, outerEquation.Width) + _divMathSign.Width + LeftGap;
        }

        protected override void CalculateHeight()
        {
            Height = outerEquation.Height + _insideEquation.Height + ExtraHeight;
            _divMathSign.Height = _insideEquation.FirstRow.Height + ExtraHeight;
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                _divMathSign.Left = value + LeftGap;
                _insideEquation.Right = Right;
                outerEquation.Right = Right;
            }
        }
    }
}
