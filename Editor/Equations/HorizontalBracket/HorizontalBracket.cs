using System;
using System.Text;
using System.Xml.Linq;

namespace Editor
{
    public abstract class HorizontalBracket : EquationContainer
    {
        protected RowContainer _topEquation;
        protected HorizontalBracketSign _bracketSign;
        protected RowContainer _bottomEquation;

        public HorizontalBracket(MainWindow owner, EquationContainer parent, HorizontalBracketSignType signType)
            : base(owner, parent)
        {
            _topEquation = new RowContainer(owner, this);
            _bottomEquation = new RowContainer(owner, this);
            _bracketSign = new HorizontalBracketSign(owner, this, signType);
            childEquations.Add(_topEquation);
            childEquations.Add(_bracketSign);
            childEquations.Add(_bottomEquation);
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var parameters = new XElement("parameters");
            parameters.Add(new XElement(_bracketSign.SignType.GetType().Name, _bracketSign.SignType));
            thisElement.Add(parameters);
            thisElement.Add(_topEquation.Serialize());
            thisElement.Add(_bottomEquation.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            XElement[] elements = [.. xElement.Elements(typeof(RowContainer).Name)];
            _topEquation.DeSerialize(elements[0]);
            _bottomEquation.DeSerialize(elements[1]);
            CalculateSize();
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToHorizontalBracket(_bracketSign.SignType, _topEquation.ToLatex(), _bottomEquation.ToLatex());
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                _bracketSign.MidX = MidX;
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
                AdjustChildrenVertical();
            }
        }

        private void AdjustChildrenVertical()
        {
            _topEquation.Top = Top;
            _bracketSign.Top = _topEquation.Bottom;
            _bottomEquation.Top = _bracketSign.Bottom;
        }

        protected override void CalculateWidth()
        {
            Width = Math.Max(_topEquation.Width, _bottomEquation.Width) + FontSize * .6;
            _bracketSign.Width = Width - FontSize * .2;
        }

        protected override void CalculateHeight()
        {
            Height = _topEquation.Height + _bottomEquation.Height + _bracketSign.Height;
            AdjustChildrenVertical();
        }

        public override double RefY
        {
            get
            {
                if (_bracketSign.SignType is HorizontalBracketSignType.TopCurly or HorizontalBracketSignType.TopSquare)
                {
                    return Height - _bottomEquation.RefY;
                }
                else
                {
                    return _topEquation.RefY;
                }
            }
        }
    }
}
