using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace Editor
{
    public sealed class NRoot : EquationContainer
    {
        private readonly RadicalSign _radicalSign;
        private readonly RowContainer _insideEquation;
        private readonly RowContainer _nthRootEquation;
        private double ExtraHeight => FontSize * .15;

        private double HGap => FontSize * .5;
        private double LeftPadding => FontSize * .1;

        public NRoot(EquationContainer parent)
            : base(parent)
        {
            _radicalSign = new RadicalSign(this);
            ActiveChild = _insideEquation = new RowContainer(this);
            _nthRootEquation = new RowContainer(this)
            {
                ApplySymbolGap = false,
                FontFactor = SubFontFactor
            };
            childEquations.AddRange([_insideEquation, _radicalSign, _nthRootEquation]);
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            thisElement.Add(_insideEquation.Serialize());
            thisElement.Add(_nthRootEquation.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            _insideEquation.DeSerialize(xElement.Elements().First());
            _nthRootEquation.DeSerialize(xElement.Elements().Last());
            CalculateSize();
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToNRoot(_insideEquation.ToLatex(), _nthRootEquation.ToLatex());
        }

        public override bool ConsumeMouseClick(Point mousePoint)
        {
            if (_nthRootEquation.Bounds.Contains(mousePoint))
            {
                ActiveChild = _nthRootEquation;
            }
            else if (_insideEquation.Bounds.Contains(mousePoint))
            {
                ActiveChild = _insideEquation;
            }
            return ActiveChild.ConsumeMouseClick(mousePoint);
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
            _radicalSign.Bottom = Bottom;
            _nthRootEquation.Bottom = _radicalSign.MidY - FontSize * .05;
        }

        protected override void CalculateWidth()
        {
            Width = Math.Max(_nthRootEquation.Width + HGap, _radicalSign.Width) +
                _insideEquation.Width + LeftPadding;
        }

        protected override void CalculateHeight()
        {
            Height = _insideEquation.Height + Math.Max(0, _nthRootEquation.Height -
                _insideEquation.Height / 2 + FontSize * .05) + ExtraHeight;
        }

        public override double Height
        {
            get => base.Height;
            set
            {
                base.Height = value;
                _radicalSign.Height = _insideEquation.Height + ExtraHeight;
                AdjustVertical();
            }
        }

        public override double RefY => _insideEquation.RefY +
            Math.Max(0, _nthRootEquation.Height - _insideEquation.Height / 2 + FontSize * .05) + ExtraHeight;

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                if (_nthRootEquation.Width + HGap > _radicalSign.Width)
                {
                    _nthRootEquation.Left = Left + LeftPadding;
                    _radicalSign.Right = _nthRootEquation.Right + HGap;
                }
                else
                {
                    _radicalSign.Left = Left + LeftPadding;
                    _nthRootEquation.Right = _radicalSign.Right - HGap;
                }
                _insideEquation.Left = _radicalSign.Right;
            }
        }

        public override bool ConsumeKey(Key key)
        {
            if (ActiveChild.ConsumeKey(key))
            {
                CalculateSize();
                return true;
            }
            if (key == Key.Left)
            {
                if (ActiveChild == _insideEquation)
                {
                    ActiveChild = _nthRootEquation;
                    return true;
                }
            }
            else if (key == Key.Right)
            {
                if (ActiveChild == _nthRootEquation)
                {
                    ActiveChild = _insideEquation;
                    return true;
                }
            }
            return false;
        }
    }
}
