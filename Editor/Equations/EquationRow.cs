using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace Editor
{
    public sealed class EquationRow : EquationContainer, ISupportsUndo
    {
        private EquationContainer? _deleteable = null;

        public EquationRow(MainWindow owner, EquationContainer parent)
            : base(owner, parent)
        {
            var textEq = new TextEquation(owner, this);
            ActiveChild = textEq;
            AddChild(textEq);
            CalculateSize();
        }

        public sealed override void CalculateSize()
        {
            base.CalculateSize();
        }

        public TextEquation GetFirstSelectionText()
        {
            return (TextEquation)childEquations[SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems];
        }

        public TextEquation GetLastSelectionText()
        {
            var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
            var otherOffset = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex);
            return (TextEquation)childEquations[otherOffset];
        }

        public void AddChildren(List<EquationBase> equations, bool insertAtStart)
        {
            if (insertAtStart)
            {
                childEquations.InsertRange(0, equations);
            }
            else
            {
                childEquations.AddRange(equations);
            }
            CalculateSize();
        }

        public TextEquation? GetFirstTextEquation()
        {
            return childEquations.First() as TextEquation;
        }

        public TextEquation? GetLastTextEquation()
        {
            return childEquations.Last() as TextEquation;
        }

        public List<EquationBase> GetSelectedEquations()
        {
            List<EquationBase> list = [];
            var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
            var endIndex = SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex;
            for (var i = startIndex; i <= endIndex; i++)
            {
                list.Add(childEquations[i]);
            }
            return list;
        }

        public List<EquationBase> DeleteTail()
        {
            List<EquationBase> removedList = [];
            var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
            if (SelectedItems != 0)
            {
                var endIndex = SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex;
                for (var i = endIndex; i > startIndex; i--)
                {
                    removedList.Add(childEquations[i]);
                    childEquations.RemoveAt(i);
                }
                removedList.Reverse();
            }
            ActiveChild = childEquations[startIndex];
            return removedList;
        }

        public List<EquationBase> DeleteHead()
        {
            List<EquationBase> removedList = [];
            var startIndex = (SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems);
            if (SelectedItems != 0)
            {
                var endIndex = SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex;
                for (var i = endIndex - 1; i >= startIndex; i--)
                {
                    removedList.Add(childEquations[i]);
                    childEquations.RemoveAt(i);
                }
                removedList.Reverse();
            }
            ActiveChild = childEquations[startIndex];
            return removedList;
        }

        public override void RemoveSelection(bool registerUndo)
        {
            if (SelectedItems != 0)
            {
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var otherIndex = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex);
                var firstEquation = (TextEquation)childEquations[startIndex];
                var lastEquation = (TextEquation)childEquations[otherIndex];
                List<EquationBase> equations = [];
                var action = new RowRemoveAction(this)
                {
                    ActiveEquation = ActiveChild,
                    HeadTextEquation = firstEquation,
                    TailTextEquation = lastEquation,
                    SelectionStartIndex = SelectionStartIndex,
                    SelectedItems = SelectedItems,
                    FirstTextCaretIndex = firstEquation.CaretIndex,
                    LastTextCaretIndex = lastEquation.CaretIndex,
                    FirstTextSelectionIndex = firstEquation.SelectionStartIndex,
                    LastTextSelectionIndex = lastEquation.SelectionStartIndex,
                    FirstTextSelectedItems = firstEquation.SelectedItems,
                    LastTextSelectedItems = lastEquation.SelectedItems,
                    FirstText = firstEquation.Text,
                    LastText = lastEquation.Text,
                    FirstFormats = firstEquation.GetFormats(),
                    LastFormats = lastEquation.GetFormats(),
                    FirstModes = firstEquation.GetModes(),
                    LastModes = lastEquation.GetModes(),
                    FirstDecorations = firstEquation.GetDecorations(),
                    LastDecorations = lastEquation.GetDecorations(),
                    Equations = equations
                };
                firstEquation.RemoveSelection(false);
                lastEquation.RemoveSelection(false);
                firstEquation.Merge(lastEquation);
                for (var i = otherIndex; i > startIndex; i--)
                {
                    equations.Add(childEquations[i]);
                    childEquations.RemoveAt(i);
                }
                SelectedItems = 0;
                equations.Reverse();
                ActiveChild = firstEquation;
                if (registerUndo)
                {
                    UndoManager.AddUndoAction(action);
                }
            }
            else
            {
                ActiveChild.RemoveSelection(registerUndo);
            }
            CalculateSize();
        }

        public override bool Select(Key key)
        {
            if (key == Key.Left)
            {
                return HandleLeftSelect(key);
            }
            else if (key == Key.Right)
            {
                return HandleRightSelect(key);
            }
            return false;
        }
        private bool HandleRightSelect(Key key)
        {
            if (ActiveChild.GetType() == typeof(TextEquation))
            {
                if (ActiveChild.Select(key))
                {
                    return true;
                }
                else if (ActiveChild == childEquations.Last())
                {
                    return false;
                }
                else
                {
                    SelectedItems += 2;
                    ActiveChild = childEquations[childEquations.IndexOf(ActiveChild) + 2];
                    childEquations[childEquations.IndexOf(ActiveChild) - 1].DeSelect();
                    if (SelectedItems > 0)
                    {
                        ((TextEquation)ActiveChild).MoveToStart();
                        ActiveChild.StartSelection();
                    }
                    return true;
                }
            }
            else
            {
                if (!ActiveChild.Select(key))
                {
                    var previsouText = (TextEquation)childEquations[SelectionStartIndex - 1];
                    var nextText = (TextEquation)childEquations[SelectionStartIndex + 1];
                    previsouText.MoveToEnd();
                    previsouText.StartSelection();
                    nextText.MoveToStart();
                    nextText.StartSelection();
                    SelectionStartIndex--;
                    SelectedItems += 2;
                    ActiveChild = nextText;
                }
                return true;
            }
        }
        private bool HandleLeftSelect(Key key)
        {
            if (ActiveChild.GetType() == typeof(TextEquation))
            {
                if (ActiveChild.Select(key))
                {
                    return true;
                }
                else if (ActiveChild == childEquations.First())
                {
                    return false;
                }
                else
                {
                    SelectedItems -= 2;
                    ActiveChild = childEquations[childEquations.IndexOf(ActiveChild) - 2];
                    childEquations[childEquations.IndexOf(ActiveChild) + 1].DeSelect();
                    if (SelectedItems < 0)
                    {
                        ((TextEquation)ActiveChild).MoveToEnd();
                        ActiveChild.StartSelection();
                    }
                    return true;
                }
            }
            else
            {
                if (!ActiveChild.Select(key))
                {
                    var previsouText = (TextEquation)childEquations[SelectionStartIndex - 1];
                    var nextText = (TextEquation)childEquations[SelectionStartIndex + 1];
                    previsouText.MoveToEnd();
                    previsouText.StartSelection();
                    nextText.MoveToStart();
                    nextText.StartSelection();
                    SelectionStartIndex++;
                    SelectedItems -= 2;
                    ActiveChild = previsouText;
                }
                return true;
            }
        }

        public override Rect GetSelectionBounds()
        {
            try
            {
                if (Owner.ViewModel.IsSelecting)
                {
                    var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                    var count = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex) - startIndex;
                    var firstRect = childEquations[startIndex].GetSelectionBounds();
                    if (firstRect == Rect.Empty)
                    {
                        firstRect = new Rect(childEquations[startIndex].Right, childEquations[startIndex].Top, 0, 0);
                    }
                    if (count > 0)
                    {
                        var lastRect = childEquations[count + startIndex].GetSelectionBounds();
                        if (lastRect == Rect.Empty)
                        {
                            lastRect = new Rect(childEquations[count + startIndex].Left, childEquations[count + startIndex].Top, 0, childEquations[count + startIndex].Height);
                        }
                        for (var i = startIndex + 1; i < startIndex + count; i++)
                        {
                            var equation = childEquations[i];
                            lastRect.Union(equation.Bounds);
                        }
                        firstRect.Union(lastRect);
                    }
                    return new Rect(firstRect.TopLeft, firstRect.BottomRight);
                }
            }
            catch
            {

            }
            return Rect.Empty;
        }

        public override void Paste(XElement xe)
        {
            if (ActiveChild.GetType() == typeof(TextEquation) && xe.Name.LocalName == GetType().Name)
            {
                var children = xe.Element("ChildEquations") ?? throw new Exception("Missing required 'ChildEquations' element in XML");
                List<EquationBase> newChildren = [];
                foreach (var xElement in children.Elements())
                {
                    newChildren.Add(CreateChild(xElement));
                }
                if (newChildren.Count > 0)
                {
                    var action = new EquationRowPasteAction(this)
                    {
                        ActiveTextEquation = (TextEquation)ActiveChild,
                        ActiveChildCaretIndex = ((TextEquation)ActiveChild).CaretIndex,
                        SelectedItems = SelectedItems,
                        SelectionStartIndex = SelectionStartIndex,
                        ActiveChildSelectedItems = ActiveChild.SelectedItems,
                        ActiveChildSelectionStartIndex = ActiveChild.SelectionStartIndex,
                        ActiveChildText = ((TextEquation)ActiveChild).Text,
                        ActiveChildFormats = ((TextEquation)ActiveChild).GetFormats(),
                        ActiveChildModes = ((TextEquation)ActiveChild).GetModes(),
                        ActiveChildDecorations = ((TextEquation)ActiveChild).GetDecorations(),
                        FirstNewText = ((TextEquation)newChildren.First()).Text,
                        LastNewText = ((TextEquation)newChildren.Last()).Text,
                        FirstNewFormats = ((TextEquation)newChildren.First()).GetFormats(),
                        LastNewFormats = ((TextEquation)newChildren.Last()).GetFormats(),
                        FirstNewModes = ((TextEquation)newChildren.First()).GetModes(),
                        LastNewModes = ((TextEquation)newChildren.Last()).GetModes(),
                        FirstNewDecorations = ((TextEquation)newChildren.First()).GetDecorations(),
                        LastNewDecorations = ((TextEquation)newChildren.Last()).GetDecorations(),
                        Equations = newChildren
                    };
                    var newChild = ActiveChild.Split(this);
                    var index = childEquations.IndexOf(ActiveChild) + 1;
                    newChildren.RemoveAt(0);
                    childEquations.InsertRange(index, newChildren);
                    ((TextEquation)ActiveChild).ConsumeFormattedText(action.FirstNewText, action.FirstNewFormats, action.FirstNewModes, action.FirstNewDecorations, false);
                    ((TextEquation)newChildren.Last()).Merge((TextEquation)newChild!);
                    ActiveChild = newChildren.Last();
                    UndoManager.AddUndoAction(action);
                }
                CalculateSize();
            }
            else
            {
                base.Paste(xe);
            }
        }

        public override void DeSelect()
        {
            base.DeSelect();
            _deleteable = null;
        }

        public override CopyDataObject? Copy(bool removeSelection)
        {
            if (SelectedItems != 0)
            {
                // Prepare information for copy
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var count = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex) - startIndex;

                var firstText = ((TextEquation)childEquations[startIndex]).GetSelectedText();
                var lastText = ((TextEquation)childEquations[startIndex + count]).GetSelectedText();
                var firstFormats = ((TextEquation)childEquations[startIndex]).GetSelectedFormats();
                var firstModes = ((TextEquation)childEquations[startIndex]).GetSelectedModes();
                var firstDecorations = ((TextEquation)childEquations[startIndex]).GetSelectedDecorations();
                var lastFormats = ((TextEquation)childEquations[startIndex + count]).GetSelectedFormats();
                var lastModes = ((TextEquation)childEquations[startIndex + count]).GetSelectedModes();
                var lastDecorations = ((TextEquation)childEquations[startIndex + count]).GetSelectedDecorations();

                var firstEquation = new TextEquation(Owner, this);
                var lastEquation = new TextEquation(Owner, this);
                firstEquation.ConsumeFormattedText(firstText, firstFormats, firstModes, firstDecorations, false);
                lastEquation.ConsumeFormattedText(lastText, lastFormats, lastModes, lastDecorations, false);

                var equations = new List<EquationBase>() { firstEquation };
                for (var i = startIndex + 1; i < startIndex + count; i++)
                {
                    equations.Add(childEquations[i]);
                }
                equations.Add(lastEquation);

                // Create bitmap if needed
                RenderTargetBitmap? bitmap = null;
                if (App.Settings.CopyType == CopyType.Image)
                {
                    double left = 0;
                    foreach (var eb in equations)
                    {
                        eb.Left = 1 + left;
                        left += eb.Width;
                    }
                    double maxUpperHalf = 0;
                    double maxBottomHalf = 0;
                    foreach (var eb in childEquations)
                    {
                        if (eb.RefY > maxUpperHalf) { maxUpperHalf = eb.RefY; }
                        if (eb.Height - eb.RefY > maxBottomHalf) { maxBottomHalf = eb.Height - eb.RefY; }
                    }
                    double width = 0;
                    foreach (var eb in equations)
                    {
                        eb.Top = 1 + maxUpperHalf - eb.RefY;
                        width += eb.Width;
                    }

                    bitmap = new RenderTargetBitmap((int)(Math.Ceiling(width + 2)),
                        (int)(Math.Ceiling(maxUpperHalf + maxBottomHalf + 2)), 96, 96, PixelFormats.Default);
                    var dv = new DrawingVisual();
                    Owner.ViewModel.IsSelecting = false;
                    using (var dc = dv.RenderOpen())
                    {
                        dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, bitmap.Width, bitmap.Height));
                        foreach (var eb in equations)
                        {
                            eb.DrawEquation(dc);
                        }
                    }
                    Owner.ViewModel.IsSelecting = true;
                    bitmap.Render(dv);
                }

                // Create text if needed
                string? copyText = null;
                if (App.Settings.CopyType == CopyType.Latex)
                {
                    var latexList = new List<StringBuilder>();
                    foreach (var eb in equations)
                    {
                        if (eb.ToLatex() is StringBuilder sb)
                        {
                            latexList.Add(sb);
                        }
                    }
                    copyText = LatexConverter.ConvertToLatexSymbol(latexList, false)?.ToString();
                }

                // Create XML element
                var thisElement = new XElement(GetType().Name);
                var children = new XElement("ChildEquations");
                foreach (var eb in equations)
                {
                    eb.SelectAll();
                    children.Add(eb.Serialize());
                }
                thisElement.Add(children);
                foreach (var eb in equations)
                {
                    eb.DeSelect();
                }

                // Remove selection if needed
                if (removeSelection)
                {
                    RemoveSelection(true);
                }

                return new CopyDataObject { Image = bitmap, Text = copyText, XElement = thisElement };
            }

            return base.Copy(removeSelection);
        }

        public override void DrawEquation(DrawingContext dc)
        {
            base.DrawEquation(dc);
            if (_deleteable != null)
            {
                // TODO: Use AppResources for all brushes?
                Brush brush = new SolidColorBrush(Colors.Gray)
                {
                    Opacity = 0.5
                };
                dc.DrawRectangle(brush, null, new Rect(_deleteable.Location, _deleteable.Size));
            }
            if (childEquations.Count == 1)
            {
                var firstEquation = (TextEquation)childEquations.First();
                if (firstEquation.TextLength == 0)
                {
                    if (Owner.ViewModel.IsSelecting)
                    {
                        //dc.DrawRectangle(Brushes.LightGray, null, new Rect(new Point(Left - 1, Top), new Size(FontSize / 2.5, Height)));
                    }
                    dc.DrawRectangle(null, PenManager.RowBoxPen, new Rect(Left, Top, Width, Height + ThinLineThickness));//new Rect(new Point(Left - 1, Top), new Size(FontSize / 2.5, Height)));
                }
            }
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var children = new XElement("ChildEquations");
            foreach (var childEquation in childEquations)
            {
                children.Add(childEquation.Serialize());
            }
            thisElement.Add(children);
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            var children = xElement.Element("ChildEquations") ?? throw new Exception("Invalid XML format");
            childEquations.Clear();
            foreach (var xe in children.Elements())
            {
                childEquations.Add(CreateChild(xe));
            }
            if (childEquations.Count == 0)
            {
                childEquations.Add(new TextEquation(Owner, this));
            }
            ActiveChild = childEquations.First();
            CalculateSize();
        }

        public override StringBuilder? ToLatex()
        {
            if (childEquations.Count == 0) return null;
            var sb = new StringBuilder();
            foreach (var childEquation in childEquations)
            {
                sb.Append(childEquation.ToLatex());
            }
            return sb;
        }

        private EquationBase CreateChild(XElement xElement)
        {
            var type = Type.GetType(GetType().Namespace + "." + xElement.Name);
            List<object> paramz = [Owner, this];
            var parameters = xElement.Element("parameters");
            if (parameters != null)
            {
                foreach (var xe in parameters.Elements())
                {
                    var paramType = Type.GetType(GetType().Namespace + "." + xe.Name) ?? Type.GetType(xe.Name.ToString());
                    if (paramType != null && paramType.IsEnum)
                    {
                        paramz.Add((Enum.Parse(paramType, xe.Value)));
                    }
                    else if (paramType == typeof(bool))
                    {
                        paramz.Add(bool.Parse(xe.Value));
                    }
                    else if (paramType == typeof(int))
                    {
                        paramz.Add(int.Parse(xe.Value));
                    }
                    else
                    {
                        paramz.Add(xe.Value);
                    }
                }
            }
            var child = Activator.CreateInstance(
                type ?? throw new Exception($"Unknown type for XML element '{xElement.Name}'. Expected type: '{GetType().Namespace}.{xElement.Name}'. XML: {xElement}"),
                [.. paramz]) as EquationBase ?? throw new Exception($"Failed to create EquationBase instance from type '{type?.FullName}' with provided parameters: [{string.Join(", ", paramz.Select(p => p?.ToString() ?? "null"))}]");
            child.DeSerialize(xElement);
            child.FontSize = FontSize;
            return child;
        }

        public override void ExecuteCommand(CommandType commandType, object? data)
        {
            _deleteable = null;
            if (ActiveChild.GetType() == typeof(TextEquation))
            {
                EquationBase? newEquation = null;
                switch (commandType)
                {
                    case CommandType.Composite:
                        newEquation = CompositeFactory.CreateEquation(Owner, this, (Position)data!);
                        break;
                    case CommandType.CompositeBig:
                        newEquation = BigCompositeFactory.CreateEquation(Owner, this, (Position)data!);
                        break;
                    case CommandType.Division:
                        newEquation = DivisionFactory.CreateEquation(Owner, this, (DivisionType)data!);
                        break;
                    case CommandType.SquareRoot:
                        newEquation = new SquareRoot(Owner, this);
                        break;
                    case CommandType.nRoot:
                        newEquation = new NRoot(Owner, this);
                        break;
                    case CommandType.LeftBracket:
                        newEquation = new LeftBracket(Owner, this, (BracketSignType)data!);
                        break;
                    case CommandType.RightBracket:
                        newEquation = new RightBracket(Owner, this, (BracketSignType)data!);
                        break;
                    case CommandType.LeftRightBracket:
                        newEquation = new LeftRightBracket(Owner, this, ((BracketSignType[])data!)[0], ((BracketSignType[])data)[1]);
                        break;
                    case CommandType.Sub:
                        newEquation = new Sub(Owner, this, (Position)data!);
                        break;
                    case CommandType.Super:
                        newEquation = new Super(Owner, this, (Position)data!);
                        break;
                    case CommandType.SubAndSuper:
                        newEquation = new SubAndSuper(Owner, this, (Position)data!);
                        break;
                    case CommandType.TopBracket:
                        newEquation = new TopBracket(Owner, this, (HorizontalBracketSignType)data!);
                        break;
                    case CommandType.BottomBracket:
                        newEquation = new BottomBracket(Owner, this, (HorizontalBracketSignType)data!);
                        break;
                    case CommandType.DoubleArrowBarBracket:
                        newEquation = new DoubleArrowBarBracket(Owner, this);
                        break;
                    case CommandType.SignComposite:
                        newEquation = SignCompositeFactory.CreateEquation(Owner, this, (Position)(((object[])data!)[0]),
                            (SignCompositeSymbol)(((object[])data)[1]), Owner.ViewModel.UseItalicIntergalOnNew);
                        break;
                    case CommandType.Decorated:
                        newEquation = new Decorated(Owner, this, (DecorationType)(((object[])data!)[0]), (Position)(((object[])data)[1]));
                        break;
                    case CommandType.Arrow:
                        newEquation = new Arrow(Owner, this, (ArrowType)(((object[])data!)[0]), (Position)(((object[])data)[1]));
                        break;
                    case CommandType.Box:
                        newEquation = new Box(Owner, this, (BoxType)data!);
                        break;
                    case CommandType.Matrix:
                        newEquation = new MatrixEquation(Owner, this, ((int[])data!)[0], ((int[])data)[1]);
                        break;
                    case CommandType.DecoratedCharacter:
                        if (((TextEquation)ActiveChild).CaretIndex > 0)
                        {
                            //newEquation = new DecoratedCharacter(Owner, this,
                            //    (TextEquation)ActiveChild,
                            //    (CharacterDecorationType)((object[])data)[0],
                            //    (Position)((object[])data)[1],
                            //    (string)((object[])data)[2]);
                            ((TextEquation)ActiveChild).AddDecoration((CharacterDecorationType)((object[])data!)[0],
                                (Position)((object[])data)[1],
                                (string)((object[])data)[2]);
                            CalculateSize();
                        }
                        break;
                    default:
                        throw new InvalidOperationException("Invalid command type for EquationRow");
                }
                if (newEquation != null)
                {
                    var newText = ActiveChild.Split(this);
                    var caretIndex = ((TextEquation)ActiveChild).TextLength;
                    AddChild(newEquation);
                    AddChild(newText!);
                    newEquation.CalculateSize();
                    ActiveChild = newEquation;
                    CalculateSize();
                    UndoManager.AddUndoAction(new RowAction(this, ActiveChild, (TextEquation)newText!, childEquations.IndexOf(ActiveChild), caretIndex));
                }
            }
            else if (ActiveChild != null)
            {
                ((EquationContainer)ActiveChild).ExecuteCommand(commandType, data);
                CalculateSize();
            }
        }

        private void AddChild(EquationBase newChild)
        {
            var index = 0;
            if (childEquations.Count > 0)
            {
                index = childEquations.IndexOf(ActiveChild) + 1;
            }
            childEquations.Insert(index, newChild);
            newChild.ParentEquation = this;
            ActiveChild = newChild;
        }

        private void RemoveChild(EquationBase child)
        {
            childEquations.Remove(child);
            CalculateSize();
        }

        public override void HandleMouseDrag(Point mousePoint)
        {
            if (mousePoint.X < ActiveChild.Left)
            {
                HandleLeftDrag(mousePoint);
            }
            else if (mousePoint.X > ActiveChild.Right)
            {
                HandleRightDrag(mousePoint);
            }
            else
            {
                ActiveChild.HandleMouseDrag(mousePoint);
            }
            SelectedItems = childEquations.IndexOf(ActiveChild) - SelectionStartIndex;
        }

        private void HandleRightDrag(Point mousePoint)
        {
            if (ActiveChild.GetType() == typeof(TextEquation))
            {
                ((TextEquation)ActiveChild).SelectToEnd();
                if (ActiveChild != childEquations.Last())
                {
                    if (mousePoint.X > childEquations[childEquations.IndexOf(ActiveChild) + 1].MidX)
                    {
                        childEquations[childEquations.IndexOf(ActiveChild) + 1].SelectAll();
                        ActiveChild = childEquations[childEquations.IndexOf(ActiveChild) + 2];
                        //childEquations[childEquations.IndexOf(ActiveChild) - 1].DeSelect();
                        if (childEquations.IndexOf(ActiveChild) > SelectionStartIndex) // old-> (SelectedItems > 0)
                        {
                            ((TextEquation)ActiveChild).MoveToStart();
                            ActiveChild.StartSelection();
                            ActiveChild.HandleMouseDrag(mousePoint);
                        }
                    }
                }
            }
            else
            {
                var previsouText = (TextEquation)childEquations[SelectionStartIndex - 1];
                var nextText = (TextEquation)childEquations[SelectionStartIndex + 1];
                previsouText.MoveToEnd();
                previsouText.StartSelection();
                nextText.MoveToStart();
                nextText.StartSelection();
                SelectionStartIndex--;
                ActiveChild = nextText;
                ActiveChild.HandleMouseDrag(mousePoint);
            }
        }

        private void HandleLeftDrag(Point mousePoint)
        {
            if (ActiveChild.GetType() == typeof(TextEquation))
            {
                ((TextEquation)ActiveChild).SelectToStart();
                if (ActiveChild != childEquations.First())
                {
                    if (mousePoint.X < childEquations[childEquations.IndexOf(ActiveChild) - 1].MidX)
                    {
                        childEquations[childEquations.IndexOf(ActiveChild) - 1].SelectAll();
                        ActiveChild = childEquations[childEquations.IndexOf(ActiveChild) - 2];
                        //childEquations[childEquations.IndexOf(ActiveChild) + 1].DeSelect();
                        if (childEquations.IndexOf(ActiveChild) < SelectionStartIndex)      // old -> (SelectedItems < 0)
                        {
                            ((TextEquation)ActiveChild).MoveToEnd();
                            ActiveChild.StartSelection();
                            ActiveChild.HandleMouseDrag(mousePoint);
                        }
                    }
                }
            }
            else
            {
                var previsouText = (TextEquation)childEquations[SelectionStartIndex - 1];
                var nextText = (TextEquation)childEquations[SelectionStartIndex + 1];
                previsouText.MoveToEnd();
                previsouText.StartSelection();
                nextText.MoveToStart();
                nextText.StartSelection();
                SelectionStartIndex++;
                ActiveChild = previsouText;
                ActiveChild.HandleMouseDrag(mousePoint);
            }
        }

        public override void SetCursorOnKeyUpDown(Key key, Point point)
        {
            foreach (var eb in childEquations)
            {
                if (eb.Right >= point.X)
                {
                    eb.SetCursorOnKeyUpDown(key, point);
                    ActiveChild = eb;
                    break;
                }
            }
        }

        public override bool ConsumeMouseClick(Point mousePoint)
        {
            _deleteable = null;
            ActiveChild = null!;
            foreach (var eb in childEquations)
            {
                if (eb.Right >= mousePoint.X && eb.Left <= mousePoint.X)
                {
                    ActiveChild = eb;
                    break;
                }
            }
            if (ActiveChild == null)
            {
                if (mousePoint.X <= MidX)
                    ActiveChild = childEquations.First();
                else
                    ActiveChild = childEquations.Last();
            }
            if (!ActiveChild.ConsumeMouseClick(mousePoint))
            {
                var moveToStart = true;
                if (childEquations.Count == 1)
                {
                    if (ActiveChild.MidX < mousePoint.X)
                    {
                        moveToStart = false;
                    }
                }
                else if (mousePoint.X < ActiveChild.MidX)
                {
                    if (ActiveChild != childEquations.First())
                    {
                        ActiveChild = childEquations[childEquations.IndexOf(ActiveChild) - 1];
                        moveToStart = false;
                    }
                }
                else if (ActiveChild != childEquations.Last())
                {
                    ActiveChild = childEquations[childEquations.IndexOf(ActiveChild) + 1];
                }
                else
                {
                    moveToStart = false;
                }
                if (ActiveChild is TextEquation equation)
                {
                    if (moveToStart)
                    {
                        equation.MoveToStart();
                    }
                    else
                    {
                        equation.MoveToEnd();
                    }
                }
            }
            return true;
        }

        public override bool ConsumeKey(Key key)
        {
            var result = false;
            if (key == Key.Home)
            {
                ActiveChild = childEquations.First();
            }
            else if (key == Key.End)
            {
                ActiveChild = childEquations.Last();
            }
            if (ActiveChild.ConsumeKey(key))
            {
                _deleteable = null;
                result = true;
            }
            else if (key == Key.Delete)
            {
                if (ActiveChild.GetType() == typeof(TextEquation) && ActiveChild != childEquations.Last())
                {
                    if (childEquations[childEquations.IndexOf(ActiveChild) + 1] == _deleteable)
                    {
                        UndoManager.AddUndoAction(new RowAction(this, _deleteable, (TextEquation)childEquations[childEquations.IndexOf(_deleteable) + 1],
                                                                childEquations.IndexOf(_deleteable), TextLength)
                        { UndoFlag = false });
                        childEquations.Remove(_deleteable);
                        _deleteable = null;
                        ((TextEquation)ActiveChild).Merge((TextEquation)childEquations[childEquations.IndexOf(ActiveChild) + 1]);
                        childEquations.Remove(childEquations[childEquations.IndexOf(ActiveChild) + 1]);
                    }
                    else
                    {
                        _deleteable = (EquationContainer)childEquations[childEquations.IndexOf(ActiveChild) + 1];
                    }
                    result = true;
                }
            }
            else if (key == Key.Back)
            {
                if (ActiveChild.GetType() == typeof(TextEquation))
                {
                    if (ActiveChild != childEquations.First())
                    {
                        if ((EquationContainer)childEquations[childEquations.IndexOf(ActiveChild) - 1] == _deleteable)
                        {
                            var equationAfter = (TextEquation)ActiveChild;
                            ActiveChild = childEquations[childEquations.IndexOf(ActiveChild) - 2];
                            UndoManager.AddUndoAction(new RowAction(this, _deleteable, equationAfter, childEquations.IndexOf(_deleteable), TextLength) { UndoFlag = false });
                            childEquations.Remove(_deleteable);
                            ((TextEquation)ActiveChild).Merge(equationAfter);
                            childEquations.Remove(equationAfter);
                            _deleteable = null;
                        }
                        else
                        {
                            _deleteable = (EquationContainer)childEquations[childEquations.IndexOf(ActiveChild) - 1];
                        }
                        result = true;
                    }
                }
                else
                {
                    if (_deleteable == ActiveChild)
                    {
                        var equationAfter = (TextEquation)childEquations[childEquations.IndexOf(ActiveChild) + 1];
                        ActiveChild = childEquations[childEquations.IndexOf(ActiveChild) - 1];
                        UndoManager.AddUndoAction(new RowAction(this, _deleteable, equationAfter, childEquations.IndexOf(_deleteable), TextLength) { UndoFlag = false });
                        childEquations.Remove(_deleteable);
                        ((TextEquation)ActiveChild).Merge(equationAfter);
                        childEquations.Remove(equationAfter);
                        _deleteable = null;
                    }
                    else
                    {
                        _deleteable = (EquationContainer)ActiveChild;
                    }
                    result = true;
                }
            }
            if (!result)
            {
                _deleteable = null;
                if (key == Key.Right)
                {
                    if (ActiveChild != childEquations.Last())
                    {
                        ActiveChild = childEquations[childEquations.IndexOf(ActiveChild) + 1];
                        result = true;
                    }
                }
                else if (key == Key.Left)
                {
                    if (ActiveChild != childEquations.First())
                    {
                        ActiveChild = childEquations[childEquations.IndexOf(ActiveChild) - 1];
                        result = true;
                    }
                }
            }
            CalculateSize();
            return result;
        }

        public void Merge(EquationRow secondLine)
        {
            ((TextEquation)childEquations.Last()).Merge((TextEquation)secondLine.childEquations.First()); //first and last are always of tyep TextEquation
            for (var i = 1; i < secondLine.childEquations.Count; i++)
            {
                AddChild(secondLine.childEquations[i]);
            }
            CalculateSize();
        }

        private void SplitRow(EquationRow newRow)
        {
            var index = childEquations.IndexOf(ActiveChild) + 1;
            var newChild = ActiveChild.Split(newRow);

            if (newChild != null)
            {
                newRow.RemoveChild(newRow.ActiveChild);
                newRow.AddChild(newChild);
                var i = index;
                for (; i < childEquations.Count; i++)
                {
                    newRow.AddChild(childEquations[i]);
                }
                for (i = childEquations.Count - 1; i >= index; i--)
                {
                    RemoveChild(childEquations[i]);
                }
            }
        }

        public override EquationBase? Split(EquationContainer newParent)
        {
            _deleteable = null;
            EquationRow? newRow = null;
            if (ActiveChild.GetType() == typeof(TextEquation))
            {
                newRow = new EquationRow(Owner, newParent);
                SplitRow(newRow);
                newRow.CalculateSize();
            }
            else
            {
                ActiveChild.Split(this);
            }
            CalculateSize();
            return newRow;
        }

        public void Truncate()
        {
            if (ActiveChild.GetType() == typeof(TextEquation))
            {
                _deleteable = null;
                ((TextEquation)ActiveChild).Truncate();
                var index = childEquations.IndexOf(ActiveChild) + 1;
                for (var i = childEquations.Count - 1; i >= index; i--)
                {
                    RemoveChild(childEquations[i]);
                }
            }
            CalculateSize();
        }

        protected override void CalculateWidth()
        {
            double width = 0;
            foreach (var eb in childEquations)
            {
                //eb.Left = Left + left;
                width += eb.Width + eb.Margin.Left + eb.Margin.Right;
            }
            if (childEquations.Count > 1)
            {
                width -= childEquations.Last().Width == 0 ? childEquations[^2].Margin.Right : 0;
                width -= childEquations.First().Width == 0 ? childEquations[1].Margin.Left : 0;
            }
            Width = width;
        }

        protected override void CalculateHeight()
        {
            double maxUpperHalf = 0;
            double maxBottomHalf = 0;
            foreach (var eb in childEquations)
            {
                if (eb.GetType() == typeof(Super) || eb.GetType() == typeof(Sub) || eb.GetType() == typeof(SubAndSuper))
                {
                    var subSuperBase = (SubSuperBase)eb;
                    if (subSuperBase.Position == Position.Right)
                    {
                        subSuperBase.SetBuddy(PreviousNonEmptyChild(subSuperBase));
                    }
                    else
                    {
                        subSuperBase.SetBuddy(NextNonEmptyChild(subSuperBase));
                    }
                }
                var childRefY = eb.RefY;
                var childHeight = eb.Height;
                if (childRefY > maxUpperHalf)
                {
                    maxUpperHalf = childRefY;
                }
                if (childHeight - childRefY > maxBottomHalf)
                {
                    maxBottomHalf = childHeight - childRefY;
                }
            }
            Height = maxUpperHalf + maxBottomHalf;
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                double left = 0;
                for (var i = 0; i < childEquations.Count; i++)
                {
                    childEquations[i].Left = left + value + (left == 0 && i == 1 ? 0 : childEquations[i].Margin.Left);
                    left += childEquations[i].Width + childEquations[i].Margin.Right + (left == 0 && i == 1 ? 0 : childEquations[i].Margin.Left);
                }
            }
        }

        public override double RefY => childEquations.First().MidY - Top;

        public override double Top
        {
            get => base.Top;
            set
            {
                base.Top = value;
                double maxUpperHalf = 0;
                foreach (var eb in childEquations)
                {
                    maxUpperHalf = Math.Max(maxUpperHalf, eb.RefY);
                }
                foreach (var eb in childEquations)
                {
                    eb.Top = (Top + maxUpperHalf) - eb.RefY;
                }
            }
        }

        private void AdjustChildrenVertical(double maxUpperHalf)
        {
            foreach (var eb in childEquations)
            {
                eb.Top = (Top + maxUpperHalf) - eb.RefY;
            }
        }

        public override double Width
        {
            get => base.Width;
            set
            {
                if (value > 0)
                {
                    base.Width = value;
                }
                else
                {
                    base.Width = FontSize / 2;
                }
            }
        }

        public void ProcessUndo(EquationAction action)
        {
            _deleteable = null;
            if (action.GetType() == typeof(RowAction))
            {
                ProcessRowAction(action);
                Owner.ViewModel.IsSelecting = false;
            }
            else if (action.GetType() == typeof(EquationRowPasteAction))
            {
                ProcessRowPasteAction(action);
            }
            else if (action.GetType() == typeof(EquationRowFormatAction))
            {
                ProcessEquationRowFormatAction(action);
            }
            else
            {
                ProcessRowRemoveAction(action);
            }
            CalculateSize();
            ParentEquation?.ChildCompletedUndo(this);
        }

        public void ResetRowEquation(int activeChildIndex, int selectionStartIndex, int selectedItems, List<EquationBase> items, bool appendAtEnd)
        {
            this.SelectionStartIndex = selectionStartIndex;
            this.SelectedItems = selectedItems;
            var index = 0;
            if (appendAtEnd)
            {
                index = childEquations.Count;
            }
            for (var i = 0; i < items.Count; i++)
            {
                childEquations.Insert(i + index, items[i]);
            }
            this.ActiveChild = childEquations[activeChildIndex];
        }

        public void ResetRowEquation(int activeChildIndex, int selectionStartIndex, int selectedItems)
        {
            SelectionStartIndex = selectionStartIndex;
            SelectedItems = selectedItems;
            ActiveChild = childEquations[activeChildIndex];
        }

        public void ResetRowEquation(EquationBase activeChild, int selectionStartIndex, int selectedItems)
        {
            SelectionStartIndex = selectionStartIndex;
            SelectedItems = selectedItems;
            ActiveChild = activeChild;
        }

        private void ProcessRowRemoveAction(EquationAction action)
        {
            var rowAction = action as RowRemoveAction;
            rowAction.HeadTextEquation.ResetTextEquation(rowAction.FirstTextCaretIndex, rowAction.FirstTextSelectionIndex,
                                                         rowAction.FirstTextSelectedItems, rowAction.FirstText, rowAction.FirstFormats,
                                                         rowAction.FirstModes, rowAction.FirstDecorations);
            rowAction.TailTextEquation.ResetTextEquation(rowAction.LastTextCaretIndex, rowAction.LastTextSelectionIndex,
                                                         rowAction.LastTextSelectedItems, rowAction.LastText,
                                                         rowAction.LastFormats, rowAction.LastModes, rowAction.LastDecorations);
            if (rowAction.UndoFlag)
            {
                childEquations.InsertRange(childEquations.IndexOf(rowAction.HeadTextEquation) + 1, rowAction.Equations);
                ActiveChild = rowAction.ActiveEquation;
                foreach (var eb in rowAction.Equations)
                {
                    eb.FontSize = FontSize;
                }
                SelectedItems = rowAction.SelectedItems;
                SelectionStartIndex = rowAction.SelectionStartIndex;
                Owner.ViewModel.IsSelecting = true;
            }
            else
            {
                rowAction.HeadTextEquation.RemoveSelection(false); //.DeleteSelectedText();
                rowAction.TailTextEquation.RemoveSelection(false); //.DeleteSelectedText();
                rowAction.HeadTextEquation.Merge(rowAction.TailTextEquation);
                var index = childEquations.IndexOf(rowAction.HeadTextEquation);
                for (var i = index + rowAction.Equations.Count; i > index; i--)
                {
                    childEquations.RemoveAt(i);
                }
                ActiveChild = rowAction.HeadTextEquation;
                SelectedItems = 0;
                Owner.ViewModel.IsSelecting = false;
            }
        }

        private void ProcessRowPasteAction(EquationAction action)
        {
            var pasteAction = action as EquationRowPasteAction;
            var activeText = pasteAction.ActiveTextEquation;
            activeText.ResetTextEquation(pasteAction.ActiveChildCaretIndex, pasteAction.ActiveChildSelectionStartIndex, pasteAction.ActiveChildSelectedItems,
                                         pasteAction.ActiveChildText, pasteAction.ActiveChildFormats, pasteAction.ActiveChildModes, pasteAction.ActiveChildDecorations);
            ActiveChild = activeText;
            if (pasteAction.UndoFlag)
            {
                SelectedItems = pasteAction.SelectedItems;
                SelectionStartIndex = pasteAction.SelectionStartIndex;
                foreach (var eb in pasteAction.Equations)
                {
                    childEquations.Remove(eb);
                }
            }
            else
            {
                ((TextEquation)pasteAction.Equations.Last()).ResetTextEquation(0, 0, 0, pasteAction.LastNewText, pasteAction.LastNewFormats, pasteAction.LastNewModes, pasteAction.LastNewDecorations);
                var newChild = ActiveChild.Split(this);
                var index = childEquations.IndexOf(ActiveChild) + 1;
                childEquations.InsertRange(index, pasteAction.Equations);
                ((TextEquation)ActiveChild).ConsumeFormattedText(pasteAction.FirstNewText, pasteAction.FirstNewFormats, pasteAction.FirstNewModes, pasteAction.FirstNewDecorations, false);
                ((TextEquation)pasteAction.Equations.Last()).Merge((TextEquation)newChild);
                ActiveChild = childEquations[index + pasteAction.Equations.Count - 1];
                foreach (var eb in pasteAction.Equations)
                {
                    eb.FontSize = FontSize;
                }
                SelectedItems = 0;
            }
        }

        private void ProcessRowAction(EquationAction action)
        {
            var rowAction = action as RowAction;
            if (rowAction.UndoFlag)
            {
                childEquations.Remove(rowAction.Equation);
                ActiveChild = childEquations.ElementAt(rowAction.Index - 1);
                ((TextEquation)ActiveChild).Merge(rowAction.EquationAfter);
                childEquations.RemoveAt(rowAction.Index);
            }
            else
            {
                ActiveChild = childEquations[rowAction.Index - 1];
                ((TextEquation)ActiveChild).Truncate(rowAction.CaretIndex);
                childEquations.Insert(rowAction.Index, rowAction.Equation);
                childEquations.Insert(rowAction.Index + 1, rowAction.EquationAfter);
                ActiveChild = rowAction.Equation;
                rowAction.Equation.FontSize = FontSize;
                rowAction.EquationAfter.FontSize = FontSize;
            }
        }

        private void ProcessEquationRowFormatAction(EquationAction action)
        {
            if (action is EquationRowFormatAction ecfa)
            {
                Owner.ViewModel.IsSelecting = true;
                SelectedItems = ecfa.SelectedItems;
                SelectionStartIndex = ecfa.SelectionStartIndex;
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var endIndex = SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex;
                childEquations[startIndex].SelectionStartIndex = ecfa.FirstChildSelectionStartIndex;
                childEquations[startIndex].SelectedItems = ecfa.FirstChildSelectedItems;
                childEquations[endIndex].SelectionStartIndex = ecfa.LastChildSelectionStartIndex;
                childEquations[endIndex].SelectedItems = ecfa.LastChildSelectedItems;
                for (var i = startIndex; i <= endIndex; i++)
                {
                    if (i > startIndex && i < endIndex)
                    {
                        childEquations[i].SelectAll();
                    }
                    childEquations[i].ModifySelection(ecfa.Operation, ecfa.Argument, ecfa.UndoFlag ? !ecfa.Applied : ecfa.Applied, false);
                }
                CalculateSize();
                ParentEquation.ChildCompletedUndo(this);
            }
        }

        public void Truncate(int indexFrom, int keepCount)
        {
            childEquations.RemoveRange(indexFrom, childEquations.Count - indexFrom);
            ((TextEquation)childEquations[indexFrom - 1]).Truncate(keepCount);
            CalculateSize();
        }

        public void SetCurrentChild(int childIndex, int caretIndex)
        {
            var textEquation = childEquations[childIndex] as TextEquation;
            textEquation.CaretIndex = caretIndex;
            ActiveChild = textEquation;
        }

        public bool IsEmpty
        {
            get
            {
                if (childEquations.Count == 1)
                {
                    if (string.IsNullOrEmpty(((TextEquation)ActiveChild).Text))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public int ActiveChildIndex
        {
            get => childEquations.IndexOf(ActiveChild); set => ActiveChild = childEquations[value];
        }

        public int TextLength => ((TextEquation)ActiveChild).TextLength;

        public void SelectToStart()
        {
            if (childEquations[SelectionStartIndex].GetType() == typeof(TextEquation))
            {
                ((TextEquation)childEquations[SelectionStartIndex]).SelectToStart();
            }
            else
            {
                SelectionStartIndex++;
                ((TextEquation)childEquations[SelectionStartIndex]).MoveToStart();
                childEquations[SelectionStartIndex].StartSelection();
            }
            for (var i = SelectionStartIndex - 2; i >= 0; i -= 2)
            {
                ((TextEquation)childEquations[i]).MoveToEnd();
                childEquations[i].StartSelection();
                ((TextEquation)childEquations[i]).SelectToStart();
                childEquations[i + 1].SelectAll();
            }
            SelectedItems = -SelectionStartIndex;
            ActiveChild = childEquations[0];
        }

        public void SelectToEnd()
        {
            if (childEquations[SelectionStartIndex].GetType() == typeof(TextEquation))
            {
                ((TextEquation)childEquations[SelectionStartIndex]).SelectToEnd();
            }
            else
            {
                SelectionStartIndex--;
                ((TextEquation)childEquations[SelectionStartIndex]).MoveToEnd();
                childEquations[SelectionStartIndex].StartSelection();
            }
            for (var i = SelectionStartIndex + 2; i < childEquations.Count; i += 2)
            {
                ((TextEquation)childEquations[i]).MoveToStart();
                childEquations[i].StartSelection();
                ((TextEquation)childEquations[i]).SelectToEnd();
                childEquations[i - 1].SelectAll();
            }
            SelectedItems = childEquations.Count - SelectionStartIndex - 1;
            ActiveChild = childEquations.Last();
        }

        public EquationBase? PreviousNonEmptyChild(EquationContainer equation)
        {
            var index = childEquations.IndexOf(equation) - 1;
            if (index >= 0)
            {
                if (index >= 1 && ((TextEquation)childEquations[index]).TextLength == 0)
                {
                    index--;
                }
                return childEquations[index];
            }
            else
            {
                return null;
            }
        }

        public EquationBase? NextNonEmptyChild(EquationContainer equation)
        {
            var index = childEquations.IndexOf(equation) + 1;
            if (index < childEquations.Count)
            {
                if (index < childEquations.Count - 1 && ((TextEquation)childEquations[index]).TextLength == 0)
                {
                    index++;
                }
                return childEquations[index];
            }
            else
            {
                return null;
            }
        }

        public override void SelectAll()
        {
            base.SelectAll();
            ((TextEquation)childEquations.Last()).MoveToEnd();
        }

        public void MoveToStart()
        {
            ActiveChild = childEquations[0];
            ((TextEquation)ActiveChild).MoveToStart();
        }

        public void MoveToEnd()
        {
            ActiveChild = childEquations.Last();
            ((TextEquation)ActiveChild).MoveToEnd();
        }

        public override double GetVerticalCaretLength()
        {
            if (ActiveChild.GetType() == typeof(TextEquation))
            {
                return Height;
            }
            else
            {
                return ActiveChild.GetVerticalCaretLength();
            }
        }

        public override Point GetVerticalCaretLocation()
        {
            if (ActiveChild.GetType() == typeof(TextEquation))
            {
                return new Point(ActiveChild.GetVerticalCaretLocation().X, Top);
            }
            else
            {
                return ActiveChild.GetVerticalCaretLocation();
            }
        }

        public override void ModifySelection(string operation, object argument, bool applied, bool addUndo)
        {
            if (Owner.ViewModel.IsSelecting)
            {
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var endIndex = SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex;
                if (endIndex - startIndex > 0)
                {
                    for (var i = startIndex; i <= endIndex; i++)
                    {
                        childEquations[i].ModifySelection(operation, argument, applied, false);
                    }
                    if (addUndo)
                    {
                        var ecfa = new EquationRowFormatAction(this)
                        {
                            Operation = operation,
                            Argument = argument,
                            Applied = applied,
                            SelectionStartIndex = SelectionStartIndex,
                            SelectedItems = SelectedItems,
                            FirstChildSelectionStartIndex = childEquations[startIndex].SelectionStartIndex,
                            FirstChildSelectedItems = childEquations[startIndex].SelectedItems,
                            LastChildSelectionStartIndex = childEquations[endIndex].SelectionStartIndex,
                            LastChildSelectedItems = childEquations[endIndex].SelectedItems,
                        };
                        UndoManager.AddUndoAction(ecfa);
                    }
                }
                else
                {
                    ActiveChild.ModifySelection(operation, argument, applied, addUndo);
                }
                CalculateSize();
            }
        }
    }
}
