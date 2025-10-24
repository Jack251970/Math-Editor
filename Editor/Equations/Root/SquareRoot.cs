using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Editor
{
    public sealed class SquareRoot : EquationContainer
    {
        private readonly RowContainer _insideEquation;
        private readonly RadicalSign _radicalSign;

        private double ExtraHeight => FontSize * .15;
        private double LeftGap => FontSize * .1;

        public SquareRoot(MainWindow owner, EquationContainer parent)
            : base(owner, parent)
        {
            _radicalSign = new RadicalSign(owner, this);
            ActiveChild = _insideEquation = new RowContainer(owner, this);
            childEquations.Add(_insideEquation);
            childEquations.Add(_radicalSign);
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
            return LatexConverter.ToSquareRoot(_insideEquation.ToLatex());
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

        private void AdjustVertical()
        {
            _insideEquation.Bottom = Bottom;
            _radicalSign.Top = Top;
        }

        protected override void CalculateWidth()
        {
            Width = _insideEquation.Width + _radicalSign.Width + LeftGap;
        }

        protected override void CalculateHeight()
        {
            Height = _insideEquation.Height + ExtraHeight;
            _radicalSign.Height = Height;
            AdjustVertical();
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                _radicalSign.Left = value + LeftGap;
                _insideEquation.Left = _radicalSign.Right;
            }
        }

        public override double RefY => _insideEquation.RefY + ExtraHeight;
    }
}
