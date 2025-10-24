using System;
using System.Text;
using System.Xml.Linq;

namespace Editor
{
    public sealed class SignSimple : EquationContainer
    {
        private readonly RowContainer _mainEquation;
        private readonly StaticSign _sign;

        public SignSimple(MainWindow owner, EquationContainer parent, SignCompositeSymbol symbol, bool useUpright)
            : base(owner, parent)
        {
            ActiveChild = _mainEquation = new RowContainer(owner, this);
            _sign = new StaticSign(owner, this, symbol, useUpright);
            childEquations.AddRange([_mainEquation, _sign]);
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var parameters = new XElement("parameters");
            parameters.Add(new XElement(_sign.Symbol.GetType().Name, _sign.Symbol));
            parameters.Add(new XElement(typeof(bool).FullName!, _sign.UseItalicIntegralSign));
            thisElement.Add(parameters);
            thisElement.Add(_mainEquation.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            _mainEquation.DeSerialize(xElement.Element(_mainEquation.GetType().Name)!);
            CalculateSize();
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToSign(SignType.Simple, _sign.ToLatex(), _mainEquation.ToLatex(), null, null);
        }

        protected override void CalculateWidth()
        {
            Width = _sign.Width + _mainEquation.Width;
        }

        protected override void CalculateHeight()
        {
            Height = Math.Max(_sign.RefY, _mainEquation.RefY) + Math.Max(_sign.RefY, _mainEquation.Height - _mainEquation.RefY);
        }

        public override double Top
        {
            get => base.Top;
            set
            {
                base.Top = value;
                _sign.MidY = MidY;
                _mainEquation.MidY = MidY;
            }
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                _sign.Left = value;
                _mainEquation.Left = _sign.Right;
            }
        }

        public override double RefY => Math.Max(_sign.RefY, _mainEquation.RefY);
    }
}
