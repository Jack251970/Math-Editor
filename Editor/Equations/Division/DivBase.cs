using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace Editor
{
    public abstract class DivBase : EquationContainer
    {
        protected RowContainer _topEquation;
        protected RowContainer _bottomEquation;

        protected DivBase(EquationContainer parent, bool isSmall = false)
            : base(parent)
        {
            if (isSmall)
            {
                SubLevel++;
            }
            ActiveChild = _topEquation = new RowContainer(this);
            _bottomEquation = new RowContainer(this);
            if (isSmall)
            {
                _topEquation.FontFactor = SubFontFactor;
                _bottomEquation.FontFactor = SubFontFactor;
                _topEquation.ApplySymbolGap = false;
                _bottomEquation.ApplySymbolGap = false;
            }
            childEquations.AddRange([_topEquation, _bottomEquation]);
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            thisElement.Add(_topEquation.Serialize());
            thisElement.Add(_bottomEquation.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            _topEquation.DeSerialize(xElement.Elements().First());
            _bottomEquation.DeSerialize(xElement.Elements().Last());
            CalculateSize();
        }

        public override bool ConsumeMouseClick(Point mousePoint)
        {
            if (_bottomEquation.Bounds.Contains(mousePoint))
            {
                ActiveChild = _bottomEquation;
                ActiveChild.ConsumeMouseClick(mousePoint);
                return true;
            }
            else if (_topEquation.Bounds.Contains(mousePoint))
            {
                ActiveChild = _topEquation;
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
                if (ActiveChild == _topEquation)
                {
                    var point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = _bottomEquation;
                    point.Y = ActiveChild.Top + 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    return true;
                }
            }
            else if (key == Key.Up)
            {
                if (ActiveChild == _bottomEquation)
                {
                    var point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = _topEquation;
                    point.Y = ActiveChild.Bottom - 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    return true;
                }
            }
            return false;
        }
    }
}
