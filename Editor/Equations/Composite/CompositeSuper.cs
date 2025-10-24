using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace Editor
{
    public sealed class CompositeSuper : CompositeBase
    {
        private readonly RowContainer topRowContainer;

        public CompositeSuper(MainWindow owner, EquationContainer parent, bool isCompositeBig)
            : base(owner, parent, isCompositeBig)
        {
            SubLevel++;
            topRowContainer = new RowContainer(owner, this)
            {
                FontFactor = SubFontFactor,
                ApplySymbolGap = false
            };
            childEquations.AddRange([mainRowContainer, topRowContainer]);
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            thisElement.Add(mainRowContainer.Serialize());
            thisElement.Add(topRowContainer.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            mainRowContainer.DeSerialize(xElement.Elements().First());
            topRowContainer.DeSerialize(xElement.Elements().Last());
            CalculateSize();
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToComposite(IsCompositeBig, Position.Super, mainRowContainer.ToLatex(),
                topRowContainer.ToLatex(), null);
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                mainRowContainer.Left = Left;
                topRowContainer.Left = mainRowContainer.Right;

            }
        }

        protected override void CalculateWidth()
        {
            Width = mainRowContainer.Width + topRowContainer.Width;
        }

        protected override void CalculateHeight()
        {
            Height = mainRowContainer.Height + topRowContainer.Height - SuperOverlap;
        }

        public override double RefY => mainRowContainer.RefY + topRowContainer.Height - SuperOverlap;

        public override double Top
        {
            get => base.Top;
            set
            {
                base.Top = value;
                topRowContainer.Top = value;
                mainRowContainer.Bottom = Bottom;
            }
        }

        public override bool ConsumeMouseClick(Point mousePoint)
        {
            if (mainRowContainer.Bounds.Contains(mousePoint))
            {
                ActiveChild = mainRowContainer;
                ActiveChild.ConsumeMouseClick(mousePoint);
                return true;
            }
            else if (topRowContainer.Bounds.Contains(mousePoint))
            {
                ActiveChild = topRowContainer;
                ActiveChild.ConsumeMouseClick(mousePoint);
                return true;
            }
            return false;
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
                if (ActiveChild == topRowContainer)
                {
                    ActiveChild = mainRowContainer;
                    return true;
                }
            }
            else if (key == Key.Up)
            {
                if (ActiveChild == mainRowContainer)
                {
                    ActiveChild = topRowContainer;
                    return true;
                }
            }
            return false;
        }
    }
}
