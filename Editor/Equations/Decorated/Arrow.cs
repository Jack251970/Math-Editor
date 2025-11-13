using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Avalonia.Input;

namespace Editor
{
    public sealed class Arrow : EquationContainer
    {
        private readonly RowContainer _rowContainer1;
        private readonly RowContainer? _rowContainer2;
        private DecorationDrawing _arrow1 = null!;
        private DecorationDrawing? _arrow2;
        private readonly ArrowType _arrowType;
        private readonly Position _equationPosition;

        private double ArrowGap
        {
            get
            {
                if (_arrowType is ArrowType.SmallRightLeftHarpoon or ArrowType.RightSmallLeftHarpoon or
                    ArrowType.RightLeftHarpoon)
                {
                    return FontSize * .2;
                }
                else
                {
                    return 0; //FontSize * .02;
                }
            }
        }

        public Arrow(IMainWindow owner, EquationContainer parent, ArrowType arrowType, Position equationPosition)
            : base(owner, parent)
        {
            _arrowType = arrowType;
            _equationPosition = equationPosition;
            SubLevel++;
            ApplySymbolGap = false;
            ActiveChild = _rowContainer1 = new RowContainer(owner, this);
            _rowContainer1.FontFactor = SubFontFactor;
            childEquations.Add(_rowContainer1);
            CreateDecorations();
            if (equationPosition == Position.BottomAndTop)
            {
                _rowContainer2 = new RowContainer(owner, this)
                {
                    FontFactor = SubFontFactor
                };
                childEquations.Add(_rowContainer2);
            }
        }

        private void CreateDecorations()
        {
            switch (_arrowType)
            {
                case ArrowType.LeftArrow:
                    _arrow1 = new DecorationDrawing(Owner, this, DecorationType.LeftArrow);
                    childEquations.Add(_arrow1);
                    break;
                case ArrowType.RightArrow:
                    _arrow1 = new DecorationDrawing(Owner, this, DecorationType.RightArrow);
                    childEquations.Add(_arrow1);
                    break;
                case ArrowType.DoubleArrow:
                    _arrow1 = new DecorationDrawing(Owner, this, DecorationType.DoubleArrow);
                    childEquations.Add(_arrow1);
                    break;
                case ArrowType.RightLeftArrow:
                case ArrowType.RightSmallLeftArrow:
                case ArrowType.SmallRightLeftArrow:
                    _arrow1 = new DecorationDrawing(Owner, this, DecorationType.RightArrow);
                    _arrow2 = new DecorationDrawing(Owner, this, DecorationType.LeftArrow);
                    childEquations.Add(_arrow1);
                    childEquations.Add(_arrow2);
                    break;
                case ArrowType.RightLeftHarpoon:
                case ArrowType.RightSmallLeftHarpoon:
                case ArrowType.SmallRightLeftHarpoon:
                    _arrow1 = new DecorationDrawing(Owner, this, DecorationType.RightHarpoonUpBarb);
                    _arrow2 = new DecorationDrawing(Owner, this, DecorationType.LeftHarpoonDownBarb);
                    childEquations.Add(_arrow1);
                    childEquations.Add(_arrow2);
                    break;
            }
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var parameters = new XElement("parameters");
            parameters.Add(new XElement(_arrowType.GetType().Name, _arrowType));
            parameters.Add(new XElement(_equationPosition.GetType().Name, _equationPosition));
            thisElement.Add(parameters);
            thisElement.Add(_rowContainer1.Serialize());
            if (_rowContainer2 != null)
            {
                thisElement.Add(_rowContainer2.Serialize());
            }
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            XElement[] elements = [.. xElement.Elements(_rowContainer1.GetType().Name)];
            _rowContainer1.DeSerialize(elements[0]);
            _rowContainer2?.DeSerialize(elements[1]);
            CalculateSize();
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToArrow(_arrowType, _equationPosition, _rowContainer1.ToLatex(), _rowContainer2?.ToLatex());
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

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                foreach (var eb in childEquations)
                {
                    eb.MidX = MidX;
                }
            }
        }

        private void AdjustVertical()
        {
            if (_equationPosition == Position.Top)
            {
                _rowContainer1.Top = Top;
                if (_arrow2 != null)
                {
                    _arrow1.Top = _rowContainer1.Bottom;
                    _arrow2.Bottom = Bottom;
                }
                else
                {
                    _arrow1.Bottom = Bottom;
                }
            }
            else if (_equationPosition == Position.Bottom)
            {
                _arrow1.Top = Top;
                _rowContainer1.Bottom = Bottom;
                _arrow2?.Top = _arrow1.Bottom + ArrowGap;
            }
            else if (_equationPosition == Position.BottomAndTop)
            {
                _rowContainer1.Top = Top;
                _arrow1.Top = _rowContainer1.Bottom;
                _arrow2?.Top = _arrow1.Bottom + ArrowGap;
                _rowContainer2!.Bottom = Bottom;
            }
        }

        public override double RefY
        {
            get
            {
                if (_equationPosition == Position.Top) //only top container
                {
                    if (_arrow2 == null)
                    {
                        return Height - LineThickness;// -arrow1.Height / 2;
                    }
                    else
                    {
                        return Height - LineThickness * 4;
                    }
                }
                else if (_equationPosition == Position.Bottom) //only bottom container
                {
                    if (_arrow2 == null)
                    {
                        return _arrow1.Height / 2;
                    }
                    else
                    {
                        return _arrow1.Height + ArrowGap / 2;
                    }
                }
                else //both top and bottom containers
                {
                    if (_arrow2 == null)
                    {
                        return _rowContainer1.Height + _arrow1.Height / 2;
                    }
                    else
                    {
                        return _rowContainer1.Height + _arrow1.Height + ArrowGap / 2;
                    }
                }
            }
        }

        protected override void CalculateHeight()
        {
            Height = childEquations.Sum(x => x.Height) + ArrowGap;
        }

        protected override void CalculateWidth()
        {
            if (_arrowType.ToString().Contains("small", StringComparison.CurrentCultureIgnoreCase))
            {
                Width = Math.Max(_rowContainer1.Width, (_rowContainer2 != null ? _rowContainer2.Width : 0)) + FontSize * 3;
            }
            else
            {
                Width = Math.Max(_rowContainer1.Width, (_rowContainer2 != null ? _rowContainer2.Width : 0)) + FontSize * 2;
            }
            switch (_arrowType)
            {
                case ArrowType.LeftArrow:
                case ArrowType.RightArrow:
                case ArrowType.DoubleArrow:
                    _arrow1.Width = Width - FontSize * .3;
                    break;

                case ArrowType.RightLeftArrow:
                case ArrowType.RightLeftHarpoon:
                    _arrow1.Width = Width - FontSize * .3;
                    _arrow2!.Width = Width - FontSize * .3;
                    break;

                case ArrowType.RightSmallLeftArrow:
                case ArrowType.RightSmallLeftHarpoon:
                    _arrow1.Width = Width - FontSize * .3;
                    _arrow2!.Width = Width - FontSize * 1.5;
                    break;

                case ArrowType.SmallRightLeftArrow:
                case ArrowType.SmallRightLeftHarpoon:
                    _arrow1.Width = Width - FontSize * 1.5;
                    _arrow2!.Width = Width - FontSize * .3;
                    break;
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
                if (ActiveChild == _rowContainer1)
                {
                    if (_rowContainer2 != null)
                    {
                        var point = ActiveChild.GetVerticalCaretLocation();
                        ActiveChild = _rowContainer2;
                        point = point.WithY(ActiveChild.Top + 1);
                        ActiveChild.SetCursorOnKeyUpDown(key, point);
                        return true;
                    }
                    else
                    {
                        // TODO: Move active child to parent equation container
                        // TODO: Add do the same for Key.Up for parent equation to move active child to _rowContainer1
                    }
                }
            }
            else if (key == Key.Up)
            {
                if (ActiveChild == _rowContainer2)
                {
                    var point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = _rowContainer1;
                    point = point.WithY(ActiveChild.Bottom - 1);
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    return true;
                }
            }
            return false;
        }
    }
}
