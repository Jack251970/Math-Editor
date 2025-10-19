using System;
using System.Text;
using System.Xml.Linq;

namespace Editor
{
    public sealed class Super : SubSuperBase
    {
        private readonly RowContainer rowContainer;

        public Super(EquationRow parent, Position position)
            : base(parent, position)
        {
            ActiveChild = rowContainer = new RowContainer(this);
            childEquations.Add(rowContainer);
            if (SubLevel == 1)
            {
                rowContainer.FontFactor = SubFontFactor;
            }
            else if (SubLevel == 2)
            {
                rowContainer.FontFactor = SubSubFontFactor;
            }
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var parameters = new XElement("parameters");
            parameters.Add(new XElement(Position.GetType().Name, Position));
            thisElement.Add(parameters);
            thisElement.Add(rowContainer.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            rowContainer.DeSerialize(xElement.Element(rowContainer.GetType().Name)!);
            CalculateSize();
        }

        public override StringBuilder? ToLatex()
        {
            if (Position == Position.Left)
            {
                return LatexConverter.ToLeftSuper(rowContainer.ToLatex());
            }
            else if (Position == Position.Right)
            {
                return LatexConverter.ToRightSuper(rowContainer.ToLatex());
            }
            else
            {
                throw new InvalidOperationException($"Invalid position for Super: {Position}");
            }
        }

        protected override void CalculateHeight()
        {
            Height = rowContainer.Height + Buddy.RefY - SuperOverlap;
        }

        public override double Top
        {
            get => base.Top;
            set
            {
                base.Top = value;
                rowContainer.Top = value;
            }
        }

        protected override void CalculateWidth()
        {
            Width = rowContainer.Width + Padding * 2;
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                rowContainer.Left = Left + Padding;
            }
        }

        public override double RefY => Height;
    }
}
