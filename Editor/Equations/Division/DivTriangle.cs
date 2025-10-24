using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Editor
{
    internal class DivTriangle : EquationContainer
    {
        private readonly RowContainer _insideEquation;
        private readonly DivTriangleSign _divTriangleSign;
        private readonly bool _isFixed;

        private double ExtraHeight => FontSize * .2;

        private double VerticalGap => FontSize * .1;

        private double LeftGap => FontSize * .1;

        public DivTriangle(MainWindow owner, EquationContainer parent, bool isFixed)
            : base(owner, parent)
        {
            _isFixed = isFixed;
            _divTriangleSign = new DivTriangleSign(owner, this);
            ActiveChild = _insideEquation = new RowContainer(owner, this);
            childEquations.Add(_insideEquation);
            childEquations.Add(_divTriangleSign);
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
            return LatexConverter.ToDivision(_isFixed ? DivisionType.DivTriangleFixed :
                DivisionType.DivTriangleExpanding, _insideEquation.ToLatex(), null);
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
            _divTriangleSign.Bottom = Bottom;
            _insideEquation.Top = Top; //Bottom - VerticalGap;            
        }

        public override void CalculateSize()
        {
            CalculateHeight();
            CalculateWidth();
        }

        protected override void CalculateWidth()
        {
            Width = _insideEquation.Width + _divTriangleSign.Width + LeftGap * 2;
        }

        protected override void CalculateHeight()
        {
            if (_isFixed)
            {
                _divTriangleSign.Height = _insideEquation.LastRow.Height + ExtraHeight;
            }
            else
            {
                _divTriangleSign.Height = _insideEquation.Height + ExtraHeight;
            }
            Height = _insideEquation.Height + ExtraHeight;
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                _divTriangleSign.Left = value + LeftGap;
                _insideEquation.Left = _divTriangleSign.Right + LeftGap;
            }
        }

        public override double RefY => _insideEquation.LastRow.MidY - Top;
    }
}
