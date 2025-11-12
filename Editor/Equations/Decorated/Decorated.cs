using System.Text;
using System.Xml.Linq;
using Avalonia.Media;

namespace Editor
{
    public sealed class Decorated : EquationContainer
    {
        private readonly RowContainer _rowContainer;
        private readonly DecorationDrawing _decoration;
        private readonly DecorationType _decorationType;
        private readonly Position _decorationPosition;

        public Decorated(IMainWindow owner, EquationContainer parent, DecorationType decorationType, Position decorationPosition)
            : base(owner, parent)
        {
            ActiveChild = _rowContainer = new RowContainer(owner, this);
            _decorationType = decorationType;
            _decorationPosition = decorationPosition;
            _decoration = new DecorationDrawing(owner, this, decorationType);
            childEquations.Add(_rowContainer);
            childEquations.Add(_decoration);
        }

        public override void DrawEquation(DrawingContext dc, bool forceBlackBrush)
        {
            _rowContainer.DrawEquation(dc, forceBlackBrush);
            _decoration.DrawEquation(dc, forceBlackBrush);
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var parameters = new XElement("parameters");
            parameters.Add(new XElement(_decorationType.GetType().Name, _decorationType));
            parameters.Add(new XElement(_decorationPosition.GetType().Name, _decorationPosition));
            thisElement.Add(parameters);
            thisElement.Add(_rowContainer.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            _rowContainer.DeSerialize(xElement.Element(_rowContainer.GetType().Name)!);
            CalculateSize();
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToDecorated(_decorationType, _decorationPosition, _rowContainer.ToLatex());
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

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                _rowContainer.Left = value;
                _decoration.Left = value;
            }
        }

        private void AdjustVertical()
        {
            if (_decorationPosition == Position.Top)
            {
                _rowContainer.Bottom = Bottom;
                _decoration.Top = Top;
            }
            else
            {
                _rowContainer.Top = Top;
                _decoration.Bottom = Bottom;
            }
        }

        public override double RefY
        {
            get
            {
                if (_decorationPosition == Position.Top)
                {
                    return _rowContainer.RefY + _decoration.Height;
                }
                else
                {
                    return _rowContainer.RefY;
                }
            }
        }

        protected override void CalculateHeight()
        {
            if (_decorationPosition == Position.Bottom)
            {
                Height = _rowContainer.Height + _decoration.Height + FontSize * .1;
            }
            else
            {
                Height = _rowContainer.Height + _decoration.Height;
            }
            AdjustVertical();
        }

        protected override void CalculateWidth()
        {
            Width = _rowContainer.Width;
            _decoration.Width = Width;
        }
    }
}
