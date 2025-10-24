using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace Editor
{
    public sealed class Box : EquationContainer
    {
        private readonly BoxType _boxType;
        private readonly RowContainer _insideEq;
        private readonly double paddingFactor = 0.2;
        private double Padding => FontSize * paddingFactor;
        private double TopPadding
        {
            get
            {
                if (_boxType is BoxType.All or BoxType.LeftTop or BoxType.RightTop)
                {
                    return 0; // paddingFactor* FontSize;
                }
                else
                {
                    return 0;
                }
            }
        }
        private double BottomPadding
        {
            get
            {
                if (_boxType is BoxType.All or BoxType.LeftBottom or BoxType.RightBottom)
                {
                    return paddingFactor * FontSize;
                }
                else
                {
                    return 0;
                }
            }
        }
        private double LeftPadding
        {
            get
            {
                if (_boxType is BoxType.All or BoxType.LeftTop or BoxType.LeftBottom)
                {
                    return paddingFactor * FontSize;
                }
                else
                {
                    return 0;
                }
            }
        }
        private double RightPadding
        {
            get
            {
                if (_boxType is BoxType.All or BoxType.RightTop or BoxType.RightBottom)
                {
                    return paddingFactor * FontSize;
                }
                else
                {
                    return 0;
                }
            }
        }

        private Point LeftTop => new(Left + LeftPadding / 2, Top + TopPadding / 2);
        private Point RightTop => new(Right - RightPadding / 2, Top + TopPadding / 2);
        private Point LeftBottom => new(Left + LeftPadding / 2, Bottom - BottomPadding / 2);
        private Point RightBottom => new(Right - RightPadding / 2, Bottom - BottomPadding / 2);

        public Box(MainWindow owner, EquationContainer parent, BoxType boxType)
            : base(owner, parent)
        {
            _boxType = boxType;
            ActiveChild = _insideEq = new RowContainer(owner, this);
            childEquations.Add(_insideEq);
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var parameters = new XElement("parameters");
            parameters.Add(new XElement(_boxType.GetType().Name, _boxType));
            thisElement.Add(parameters);
            thisElement.Add(_insideEq.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            _insideEq.DeSerialize(xElement.Element(_insideEq.GetType().Name)!);
            CalculateSize();
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToBox(_boxType, _insideEq.ToLatex());
        }

        public override void DrawEquation(DrawingContext dc)
        {
            base.DrawEquation(dc);
            switch (_boxType)
            {
                case BoxType.All:
                    dc.DrawPolyline(LeftTop, [RightTop, RightBottom, LeftBottom, LeftTop, RightTop], StandardMiterPen);
                    break;
                case BoxType.LeftBottom:
                    dc.DrawPolyline(LeftTop, [LeftBottom, RightBottom], StandardMiterPen);
                    break;
                case BoxType.LeftTop:
                    dc.DrawPolyline(RightTop, [LeftTop, LeftBottom], StandardMiterPen);
                    break;
                case BoxType.RightBottom:
                    dc.DrawPolyline(RightTop, [RightBottom, LeftBottom], StandardMiterPen);
                    break;
                case BoxType.RightTop:
                    dc.DrawPolyline(LeftTop, [RightTop, RightBottom], StandardMiterPen);
                    break;
            }
        }

        public override double Top
        {
            get => base.Top;
            set
            {
                base.Top = value;
                _insideEq.Top = value + TopPadding;
            }
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                _insideEq.Left = value + LeftPadding;
            }
        }

        protected override void CalculateWidth()
        {
            Width = _insideEq.Width + LeftPadding + RightPadding;
        }

        protected override void CalculateHeight()
        {
            Height = _insideEq.Height + TopPadding + BottomPadding;
        }

        public override double RefY => _insideEq.RefY + TopPadding;
    }
}
