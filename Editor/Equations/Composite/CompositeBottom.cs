using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Input;

namespace Editor
{
    public sealed class CompositeBottom : CompositeBase
    {
        private readonly RowContainer bottomRowContainer;

        public CompositeBottom(IMainWindow owner, EquationContainer parent, bool isCompositeBig)
            : base(owner, parent, isCompositeBig)
        {
            SubLevel++;
            bottomRowContainer = new RowContainer(owner, this)
            {
                FontFactor = SubFontFactor,
                ApplySymbolGap = false
            };
            childEquations.AddRange([mainRowContainer, bottomRowContainer]);
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            thisElement.Add(mainRowContainer.Serialize());
            thisElement.Add(bottomRowContainer.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            mainRowContainer.DeSerialize(xElement.Elements().First());
            bottomRowContainer.DeSerialize(xElement.Elements().Last());
            CalculateSize();
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToComposite(IsCompositeBig, Position.Bottom, mainRowContainer.ToLatex(),
                null, bottomRowContainer.ToLatex());
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                bottomRowContainer.MidX = MidX;
                mainRowContainer.MidX = MidX;
            }
        }

        protected override void CalculateWidth()
        {
            Width = Math.Max(mainRowContainer.Width, bottomRowContainer.Width);
        }

        protected override void CalculateHeight()
        {
            Height = mainRowContainer.Height + bottomRowContainer.Height + bottomGap;
        }


        public override double RefY => mainRowContainer.RefY;


        public override double Top
        {
            get => base.Top;
            set
            {
                base.Top = value;
                mainRowContainer.Top = value;
                bottomRowContainer.Top = mainRowContainer.Bottom + bottomGap;
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
            else if (bottomRowContainer.Bounds.Contains(mousePoint))
            {
                ActiveChild = bottomRowContainer;
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
                if (ActiveChild == mainRowContainer)
                {
                    var point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = bottomRowContainer;
                    point = point.WithY(ActiveChild.Top + 1);
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    return true;
                }
            }
            else if (key == Key.Up)
            {
                if (ActiveChild == bottomRowContainer)
                {
                    var point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = mainRowContainer;
                    point = point.WithY(ActiveChild.Bottom - 1);
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    return true;
                }
            }
            return false;
        }
    }
}
