using System.Text;
using System.Xml.Linq;

namespace Editor
{
    public sealed class Sub : SubSuperBase
    {
        private readonly RowContainer rowContainer;

        public Sub(EquationRow parent, Position position)
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
            return LatexConverter.ToSubOrSuper(SubSuperType.Sub, Position, null, rowContainer.ToLatex());
        }

        protected override void CalculateWidth()
        {
            /*
            double width = rowContainer.Width + Padding * 2;
            TextEquation te = Buddy as TextEquation;
            if (te != null)
            {
                width += te.OverHangTrailing;
            }
            */
            Width = rowContainer.Width + Padding * 2;
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                rowContainer.Left = this.Left + Padding;
            }
        }

        public override System.Windows.Thickness Margin
        {
            get
            {
                double left = 0;
                if (Buddy is TextEquation te)
                {
                    left += te.OverhangTrailing;
                }
                return new System.Windows.Thickness(left, 0, 0, 0);
            }
        }

        protected override void CalculateHeight()
        {
            Height = rowContainer.Height - SubOverlap;
        }

        public override double Top
        {
            get => base.Top;
            set
            {
                base.Top = value;
                rowContainer.Bottom = Bottom;
            }
        }

        public override double RefY => 0;
    }
}
