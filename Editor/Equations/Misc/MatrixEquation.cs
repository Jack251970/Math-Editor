using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace Editor
{
    public sealed class MatrixEquation : EquationContainer
    {
        private readonly int _columns = 1;
        private readonly int _rows = 1;
        private double CellSpace => FontSize * .7;

        public override Thickness Margin => new(FontSize * .15, 0, FontSize * .15, 0);

        public MatrixEquation(EquationContainer parent, int rows, int columns)
            : base(parent)
        {
            _rows = rows;
            _columns = columns;
            // 0 1 2
            // 3 4 5
            // 6 7 8
            for (var i = 0; i < columns * rows; i++)
            {
                childEquations.Add(new RowContainer(this));
            }
            ActiveChild = childEquations.First();
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var parameters = new XElement("parameters");
            parameters.Add(new XElement(typeof(int).FullName!, _rows));
            parameters.Add(new XElement(typeof(int).FullName!, _columns));
            thisElement.Add(parameters);
            foreach (var eb in childEquations)
            {
                thisElement.Add(eb.Serialize());
            }
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            XElement[] elements = [.. xElement.Elements(typeof(RowContainer).Name)];
            for (var i = 0; i < childEquations.Count; i++)
            {
                childEquations[i].DeSerialize(elements[i]);
            }
            CalculateSize();
        }

        public override StringBuilder? ToLatex()
        {
            if (childEquations.Count == 0) return null;
            var matrix = new List<StringBuilder>();
            foreach (var childEquation in childEquations)
            {
                if (childEquation.ToLatex() is StringBuilder childLatex)
                {
                    matrix.Add(childLatex);
                }
            }
            return LatexConverter.ToMatrix(_rows, _columns, matrix);
        }

        public override double Top
        {
            get => base.Top;
            set
            {
                base.Top = value;
                var rowRefYs = new double[_rows];
                var topOffsets = new double[_rows + 1];

                for (var i = 0; i < _rows; i++)
                {
                    rowRefYs[i] = childEquations.Skip(i * _columns).Take(_columns).Max(x => x.RefY);
                    topOffsets[i + 1] = childEquations.Skip(i * _columns).Take(_columns).Max(x => x.Height) + topOffsets[i];
                }

                for (var i = 0; i < _rows; i++)
                {
                    for (var j = 0; j < _columns; j++)
                    {
                        childEquations[i * _columns + j].MidY = Top + rowRefYs[i] + topOffsets[i] + CellSpace * i;
                    }
                }
            }
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                var columnRefXs = new double[_columns];
                var leftOffsets = new double[_columns + 1];
                for (var i = 0; i < _columns; i++)
                {
                    for (var j = 0; j < _rows; j++)
                    {
                        columnRefXs[i] = Math.Max(childEquations[j * _columns + i].RefX, columnRefXs[i]);
                        leftOffsets[i + 1] = Math.Max(childEquations[j * _columns + i].Width, leftOffsets[i + 1]);
                    }
                    leftOffsets[i + 1] += leftOffsets[i];
                }
                for (var i = 0; i < _columns; i++)
                {
                    for (var j = 0; j < _rows; j++)
                    {
                        childEquations[j * _columns + i].MidX = value + columnRefXs[i] + leftOffsets[i] + CellSpace * i;
                    }
                }
            }
        }

        protected override void CalculateWidth()
        {
            var columnWidths = new double[_columns];
            for (var i = 0; i < _columns; i++)
            {
                for (var j = 0; j < _rows; j++)
                {
                    columnWidths[i] = Math.Max(childEquations[j * _columns + i].Width, columnWidths[i]);
                }
            }
            Width = columnWidths.Sum() + CellSpace * (_columns - 1);
        }

        protected override void CalculateHeight()
        {
            var rowHeights = new double[_rows];
            for (var i = 0; i < _rows; i++)
            {
                rowHeights[i] = childEquations.Skip(i * _columns).Take(_columns).Max(x => x.Height);
            }
            Height = rowHeights.Sum() + CellSpace * (_rows - 1);
        }

        public override double RefY
        {
            get
            {
                if (_rows == 1)
                {
                    return childEquations.Max(x => x.RefY);
                }
                else if (_rows % 2 == 0)
                {
                    //return childEquations.Take(rows / 2 * columns).Sum(x => x.Height) - CellSpace / 2 + FontSize * .3;
                    var rowHeights = new double[_rows / 2];
                    for (var i = 0; i < _rows / 2; i++)
                    {
                        rowHeights[i] = childEquations.Skip(i * _columns).Take(_columns).Max(x => x.Height);
                    }
                    return rowHeights.Sum() + CellSpace * _rows / 2 - CellSpace / 2 + FontSize * .1;
                }
                else
                {
                    //return childEquations.Skip(rows / 2 * columns).Take(columns).Max(x => x.MidY) - Top;
                    var rowHeights = new double[_rows / 2 + 1];
                    for (var i = 0; i < _rows / 2; i++)
                    {
                        rowHeights[i] = childEquations.Skip(i * _columns).Take(_columns).Max(x => x.Height);
                    }
                    rowHeights[_rows / 2] = childEquations.Skip(_rows / 2 * _columns).Take(_columns).Max(x => x.RefY);
                    return rowHeights.Sum() + CellSpace * (_rows / 2);// -FontSize * .1;
                }
            }
        }

        public override bool ConsumeKey(Key key)
        {
            if (ActiveChild.ConsumeKey(key))
            {
                CalculateSize();
                return true;
            }
            var currentIndex = childEquations.IndexOf(ActiveChild);
            if (key == Key.Right)
            {
                if (currentIndex % _columns < _columns - 1)//not last column?
                {
                    ActiveChild = childEquations[currentIndex + 1];
                    return true;
                }
            }
            else if (key == Key.Left)
            {
                if (currentIndex % _columns > 0)//not last column?
                {
                    ActiveChild = childEquations[currentIndex - 1];
                    return true;
                }
            }
            else if (key == Key.Up)
            {
                if (currentIndex / _columns > 0)//not in first row?
                {
                    var point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = childEquations[currentIndex - _columns]; ;
                    point.Y = ActiveChild.Top + 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    return true;
                }
            }
            else if (key == Key.Down)
            {
                if (currentIndex / _columns < _rows - 1)//not in last row?
                {
                    var point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = childEquations[currentIndex + _columns]; ;
                    point.Y = ActiveChild.Top + 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    return true;
                }
            }
            return false;
        }
    }
}
