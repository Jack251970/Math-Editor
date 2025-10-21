using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace Editor
{
    public sealed class SignSub : EquationContainer
    {
        private readonly RowContainer mainEquation;
        private readonly StaticSign sign;
        private readonly RowContainer subEquation;
        private double SubOverlap => FontSize * .5;
        private double maxUpperHalf = 0;
        private readonly double gapFactor = .06;
        private double Gap => FontSize * gapFactor;
        private double LeftMinus { get; set; }
        private double MainLeft => Left + LeftMinus;

        public SignSub(EquationContainer parent, SignCompositeSymbol symbol, bool useUpright)
            : base(parent)
        {
            ActiveChild = mainEquation = new RowContainer(this);
            this.SubLevel++;
            subEquation = new RowContainer(this)
            {
                ApplySymbolGap = false
            };
            sign = new StaticSign(this, symbol, useUpright);
            subEquation.FontFactor = SubFontFactor;
            childEquations.AddRange([mainEquation, sign, subEquation]);
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var parameters = new XElement("parameters");
            parameters.Add(new XElement(sign.Symbol.GetType().Name, sign.Symbol));
            parameters.Add(new XElement(typeof(bool).FullName!, sign.UseItalicIntegralSign));
            thisElement.Add(parameters);
            thisElement.Add(mainEquation.Serialize());
            thisElement.Add(subEquation.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            XElement[] elements = [.. xElement.Elements(typeof(RowContainer).Name)];
            mainEquation.DeSerialize(elements[0]);
            subEquation.DeSerialize(elements[1]);
            CalculateSize();
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToSign(SignType.Sub, sign.ToLatex(), mainEquation.ToLatex(), null,
                subEquation.ToLatex());
        }

        protected override void CalculateWidth()
        {
            if (sign.Symbol.ToString().Contains("integral", StringComparison.CurrentCultureIgnoreCase))
            {
                LeftMinus = sign.OverhangTrailing;
            }
            Width = sign.Width + subEquation.Width + mainEquation.Width + Gap + LeftMinus;
        }

        protected override void CalculateHeight()
        {
            maxUpperHalf = Math.Max(mainEquation.RefY, sign.RefY);
            var maxLowerHalf = Math.Max(mainEquation.RefYReverse, sign.RefYReverse + subEquation.Height - SubOverlap);
            Height = maxLowerHalf + maxUpperHalf;
            sign.MidY = MidY;
            mainEquation.MidY = MidY;
            subEquation.Top = sign.Bottom - SubOverlap;
        }

        public override double Height
        {
            get => base.Height; set => base.Height = value;
        }

        public override double RefY => maxUpperHalf;

        public override double Top
        {
            get => base.Top;
            set
            {
                base.Top = value;
                sign.MidY = MidY;
                mainEquation.MidY = MidY;
                subEquation.Top = sign.Bottom - SubOverlap;
            }
        }

        public override bool ConsumeMouseClick(Point mousePoint)
        {
            if (subEquation.Bounds.Contains(mousePoint))
            {
                ActiveChild = subEquation;
            }
            else
            {
                ActiveChild = mainEquation;
            }
            return ActiveChild.ConsumeMouseClick(mousePoint);
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                sign.Left = value;
                subEquation.Left = sign.Right + LeftMinus;
                mainEquation.Left = subEquation.Right + Gap;
            }
        }

        public override bool ConsumeKey(Key key)
        {
            if (ActiveChild.ConsumeKey(key))
            {
                CalculateSize();
                return true;
            }
            if (key == Key.Down)
            {
                if (ActiveChild == mainEquation)
                {
                    ActiveChild = subEquation;
                    return true;
                }
            }
            else if (key == Key.Up)
            {
                if (ActiveChild == subEquation)
                {
                    ActiveChild = mainEquation;
                    return true;
                }
            }
            return false;
        }
    }
}
