using System;
using System.Text;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Input;

namespace Editor
{
    public sealed class SignBottom : EquationContainer
    {
        private readonly RowContainer mainEquation;
        private readonly RowContainer bottomEquation;
        private readonly StaticSign sign;
        private double HGap => FontSize * .02;
        private double VGap => FontSize * .05;

        public SignBottom(IMainWindow owner, EquationContainer parent, SignCompositeSymbol symbol, bool useUpright)
            : base(owner, parent)
        {
            ActiveChild = mainEquation = new RowContainer(owner, this);
            SubLevel++;
            bottomEquation = new RowContainer(owner, this)
            {
                ApplySymbolGap = false
            };
            sign = new StaticSign(owner, this, symbol, useUpright);
            bottomEquation.FontFactor = SubFontFactor;
            childEquations.AddRange([mainEquation, bottomEquation, sign]);
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var parameters = new XElement("parameters");
            parameters.Add(new XElement(sign.Symbol.GetType().Name, sign.Symbol));
            parameters.Add(new XElement(typeof(bool).FullName!, sign.UseItalicIntegralSign));
            thisElement.Add(parameters);
            thisElement.Add(mainEquation.Serialize());
            thisElement.Add(bottomEquation.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            XElement[] elements = [.. xElement.Elements(typeof(RowContainer).Name)];
            mainEquation.DeSerialize(elements[0]);
            bottomEquation.DeSerialize(elements[1]);
            CalculateSize();
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToSign(SignType.Bottom, sign.ToLatex(), mainEquation.ToLatex(), null,
                bottomEquation.ToLatex());
        }

        protected override void CalculateWidth()
        {
            var maxLeft = Math.Max(sign.Width, bottomEquation.Width);
            Width = maxLeft + mainEquation.Width + HGap;
            sign.MidX = Left + maxLeft / 2;
            bottomEquation.MidX = sign.MidX;
            mainEquation.Left = Math.Max(bottomEquation.Right, sign.Right) + HGap;
        }

        protected override void CalculateHeight()
        {
            var upperHalf = Math.Max(mainEquation.RefY, sign.RefY);
            var lowerHalf = Math.Max(sign.RefY + VGap + bottomEquation.Height, mainEquation.Height - mainEquation.RefY);
            Height = upperHalf + lowerHalf;
            AdjustVertical();
        }

        private void AdjustVertical()
        {
            if (mainEquation.RefY > sign.RefY)
            {
                sign.MidY = MidY;
                mainEquation.MidY = MidY;
                bottomEquation.Top = sign.Bottom + VGap;
            }
            else
            {
                bottomEquation.Bottom = Bottom;
                sign.Bottom = bottomEquation.Top - VGap;
                mainEquation.MidY = sign.MidY;
            }
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

        public override bool ConsumeMouseClick(Point mousePoint)
        {
            if (bottomEquation.Bounds.Contains(mousePoint))
            {
                ActiveChild = bottomEquation;
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
                var maxLeft = Math.Max(sign.Width, bottomEquation.Width);
                sign.MidX = value + maxLeft / 2;
                bottomEquation.MidX = sign.MidX;
                mainEquation.Left = Math.Max(bottomEquation.Right, sign.Right) + HGap;
            }
        }

        public override double Height
        {
            get => base.Height;
            set => base.Height = value;
        }

        public override double RefY => Math.Max(sign.RefY, mainEquation.RefY);

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
                    ActiveChild = bottomEquation;
                    return true;
                }
            }
            else if (key == Key.Up)
            {
                if (ActiveChild == bottomEquation)
                {
                    ActiveChild = mainEquation;
                    return true;
                }
            }
            return false;
        }
    }
}
