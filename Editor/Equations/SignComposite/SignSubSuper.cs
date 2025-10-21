using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace Editor
{
    public sealed class SignSubSuper : EquationContainer
    {
        private readonly RowContainer mainEquation;
        private readonly StaticSign sign;
        private readonly RowContainer superEquation;
        private readonly RowContainer subEquation;
        private double HGap => FontSize * .06;
        private double SubMinus = 0;
        private double SuperMinus = 0;

        public SignSubSuper(EquationContainer parent, SignCompositeSymbol symbol, bool useUpright)
            : base(parent)
        {
            ActiveChild = mainEquation = new RowContainer(this);
            SubLevel++;
            subEquation = new RowContainer(this);
            superEquation = new RowContainer(this);
            subEquation.ApplySymbolGap = false;
            superEquation.ApplySymbolGap = false;
            sign = new StaticSign(this, symbol, useUpright);
            subEquation.FontFactor = SubFontFactor;
            superEquation.FontFactor = SubFontFactor;
            childEquations.AddRange([mainEquation, sign, superEquation, subEquation]);
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
            thisElement.Add(superEquation.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            XElement[] elements = [.. xElement.Elements(typeof(RowContainer).Name)];
            mainEquation.DeSerialize(elements[0]);
            subEquation.DeSerialize(elements[1]);
            superEquation.DeSerialize(elements[2]);
            CalculateSize();
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToSign(SignType.SubSuper, sign.ToLatex(), mainEquation.ToLatex(),
                superEquation.ToLatex(), subEquation.ToLatex());
        }

        protected override void CalculateWidth()
        {
            if (sign.Symbol.ToString().Contains("integral", StringComparison.CurrentCultureIgnoreCase))
            {
                SubMinus = sign.OverhangTrailing;
                SuperMinus = sign.OverhangLeading + (sign.UseItalicIntegralSign ? FontSize * .1 : 0);
            }
            Width = sign.Width + Math.Max(subEquation.Width + SubMinus, superEquation.Width + SuperMinus) + mainEquation.Width + HGap;
        }

        protected override void CalculateHeight()
        {
            Height = Math.Max(sign.Height * .5 + subEquation.Height + superEquation.Height, mainEquation.Height);
        }

        public override double RefY => Math.Max(superEquation.Height + sign.Height * .3, mainEquation.RefY);

        public override double Top
        {
            get => base.Top;
            set
            {
                base.Top = value;
                sign.MidY = MidY;
                mainEquation.MidY = MidY;
                subEquation.Top = sign.Bottom - sign.Height * .3;
                superEquation.Bottom = sign.Top + sign.Height * .2;
            }
        }

        public override bool ConsumeMouseClick(Point mousePoint)
        {
            if (mainEquation.Bounds.Contains(mousePoint))
            {
                ActiveChild = mainEquation;
            }
            else if (superEquation.Bounds.Contains(mousePoint))
            {
                ActiveChild = superEquation;
            }
            else
            {
                ActiveChild = subEquation;
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
                subEquation.Left = sign.Right + SubMinus;
                superEquation.Left = sign.Right + SuperMinus;
                mainEquation.Left = Math.Max(subEquation.Right, superEquation.Right) + HGap;
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
                if (ActiveChild == superEquation)
                {
                    ActiveChild = mainEquation;
                    return true;
                }
                else if (ActiveChild == mainEquation)
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
                else if (ActiveChild == mainEquation)
                {
                    ActiveChild = superEquation;
                    return true;
                }
            }
            return false;
        }
    }
}
