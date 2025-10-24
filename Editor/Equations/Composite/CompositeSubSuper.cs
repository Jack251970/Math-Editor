using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace Editor
{
    public sealed class CompositeSubSuper : CompositeBase
    {
        private readonly RowContainer superRow;
        private readonly RowContainer subRow;

        public CompositeSubSuper(MainWindow owner, EquationContainer parent, bool isCompositeBig)
            : base(owner, parent, isCompositeBig)
        {
            SubLevel++;
            subRow = new RowContainer(owner, this);
            superRow = new RowContainer(owner, this)
            {
                FontFactor = subRow.FontFactor = SubFontFactor,
                ApplySymbolGap = subRow.ApplySymbolGap = false
            };
            childEquations.AddRange([mainRowContainer, subRow, superRow]);
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            thisElement.Add(mainRowContainer.Serialize());
            thisElement.Add(subRow.Serialize());
            thisElement.Add(superRow.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            XElement[] elementArray = [.. xElement.Elements()];
            mainRowContainer.DeSerialize(elementArray[0]);
            subRow.DeSerialize(elementArray[1]);
            superRow.DeSerialize(elementArray[2]);
            CalculateSize();
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToComposite(IsCompositeBig, Position.SubAndSuper, mainRowContainer.ToLatex(),
                superRow.ToLatex(), subRow.ToLatex());
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                mainRowContainer.Left = value;
                subRow.Left = mainRowContainer.Right;
                superRow.Left = mainRowContainer.Right;
            }
        }

        protected override void CalculateWidth()
        {
            Width = mainRowContainer.Width + Math.Max(subRow.Width, superRow.Width);
        }

        protected override void CalculateHeight()
        {
            Height = mainRowContainer.Height + subRow.Height - SubOverlap + superRow.Height - SuperOverlap;
        }

        public override double RefY => superRow.Height - SuperOverlap + mainRowContainer.RefY;

        public override double Top
        {
            get => base.Top;
            set
            {
                base.Top = value;
                superRow.Top = value;
                mainRowContainer.Top = superRow.Bottom - SuperOverlap;
                subRow.Bottom = Bottom;
            }
        }

        public override bool ConsumeMouseClick(Point mousePoint)
        {
            var returnValue = false;
            if (mainRowContainer.Bounds.Contains(mousePoint))
            {
                ActiveChild = mainRowContainer;
                returnValue = true;
            }
            else if (subRow.Bounds.Contains(mousePoint))
            {
                ActiveChild = subRow;
                returnValue = true;
            }
            else if (superRow.Bounds.Contains(mousePoint))
            {
                ActiveChild = superRow;
                returnValue = true;
            }
            ActiveChild.ConsumeMouseClick(mousePoint);
            return returnValue;
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
                if (ActiveChild == mainRowContainer)
                {
                    ActiveChild = subRow;
                    return true;
                }
                else if (ActiveChild == superRow)
                {
                    ActiveChild = mainRowContainer;
                    return true;
                }
            }
            else if (key == Key.Up)
            {
                if (ActiveChild == subRow)
                {
                    ActiveChild = mainRowContainer;
                    return true;
                }
                else if (ActiveChild == mainRowContainer)
                {
                    ActiveChild = superRow;
                    return true;
                }
            }
            return false;
        }
    }
}
