using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace Editor
{
    public sealed class RowContainer : EquationContainer, ISupportsUndo
    {
        private readonly double lineSpaceFactor;
        private double LineSpace => lineSpaceFactor * FontSize;

        public EquationBase FirstRow => childEquations.First();
        public EquationBase LastRow => childEquations.Last();

        public override void Paste(XElement xe)
        {
            if (((EquationRow)ActiveChild).ActiveChild.GetType() == typeof(TextEquation) && xe.Name.LocalName == GetType().Name)
            {
                var children = xe.Element("ChildRows");
                List<EquationRow> newRows = [];
                foreach (var xElement in children.Elements())
                {
                    var row = new EquationRow(Owner, this);
                    row.DeSerialize(xElement);
                    newRows.Add(row);
                    row.FontSize = FontSize;
                }
                if (newRows.Count > 0)
                {
                    var activeText = (TextEquation)((EquationRow)ActiveChild).ActiveChild;
                    var action = new RowContainerPasteAction(this)
                    {
                        ActiveEquation = ActiveChild,
                        ActiveEquationSelectedItems = ActiveChild.SelectedItems,
                        ActiveEquationSelectionIndex = ActiveChild.SelectionStartIndex,
                        ActiveTextInChildRow = activeText,
                        TextEquationDecorations = activeText.GetDecorations(),
                        CaretIndexOfActiveText = activeText.CaretIndex,
                        TextEquationContents = activeText.Text,
                        TextEquationFormats = activeText.GetFormats(),
                        TextEquationModes = activeText.GetModes(),
                        SelectedItems = SelectedItems,
                        SelectionStartIndex = SelectionStartIndex,
                        SelectedItemsOfTextEquation = activeText.SelectedItems,
                        SelectionStartIndexOfTextEquation = activeText.SelectionStartIndex,
                        HeadTextOfPastedRows = newRows[0].GetFirstTextEquation().Text,
                        TailTextOfPastedRows = newRows.Last().GetLastTextEquation().Text,
                        HeadFormatsOfPastedRows = newRows[0].GetFirstTextEquation().GetFormats(),
                        TailFormatsOfPastedRows = newRows.Last().GetLastTextEquation().GetFormats(),
                        HeadModeOfPastedRows = newRows[0].GetFirstTextEquation().GetModes(),
                        TailModesOfPastedRows = newRows.Last().GetLastTextEquation().GetModes(),
                        HeadDecorationsOfPastedRows = newRows[0].GetFirstTextEquation().GetDecorations(),
                        TailDecorationsOfPastedRows = newRows.Last().GetLastTextEquation().GetDecorations(),
                        Equations = newRows
                    };
                    var newRow = (EquationRow)ActiveChild.Split(this);
                    ((EquationRow)ActiveChild).Merge(newRows[0]);
                    var index = childEquations.IndexOf(ActiveChild) + 1;
                    childEquations.InsertRange(index, newRows.Skip(1));
                    newRows.Last().Merge(newRow);
                    newRows.Add(newRow);
                    ActiveChild = childEquations[index + newRows.Count - 3];
                    UndoManager.AddUndoAction(action);
                }
                CalculateSize();
            }
            else
            {
                base.Paste(xe);
            }
        }

        public override void ConsumeText(string text)
        {
            if (((EquationRow)ActiveChild).ActiveChild.GetType() == typeof(TextEquation))
            {
                List<string> lines = [];
                using (var reader = new StringReader(text))
                {
                    string s;
                    while ((s = reader.ReadLine()) != null)
                    {
                        lines.Add(s);
                    }
                }
                if (lines.Count == 1)
                {
                    ActiveChild.ConsumeText(lines[0]);
                }
                else if (lines.Count > 1)
                {
                    List<EquationRow> newEquations = [];
                    var activeText = (TextEquation)((EquationRow)ActiveChild).ActiveChild;
                    var action = new RowContainerTextAction(this)
                    {
                        ActiveEquation = ActiveChild,
                        SelectedItems = SelectedItems,
                        SelectionStartIndex = SelectionStartIndex,
                        ActiveEquationSelectedItems = ActiveChild.SelectedItems,
                        ActiveEquationSelectionIndex = ActiveChild.SelectionStartIndex,
                        ActiveTextInRow = activeText,
                        CaretIndexOfActiveText = activeText.CaretIndex,
                        SelectedItemsOfTextEquation = activeText.SelectedItems,
                        SelectionStartIndexOfTextEquation = activeText.SelectionStartIndex,
                        TextEquationContents = activeText.Text,
                        TextEquationFormats = activeText.GetFormats(),
                        FirstLineOfInsertedText = lines[0],
                        Equations = newEquations
                    };
                    UndoManager.DisableAddingActions = true;
                    ActiveChild.ConsumeText(lines[0]);
                    action.FirstFormatsOfInsertedText = activeText.GetFormats();
                    var splitRow = (EquationRow)ActiveChild.Split(this);
                    if (!splitRow.IsEmpty)
                    {
                        childEquations.Add(splitRow);
                    }
                    var activeIndex = childEquations.IndexOf(ActiveChild);
                    var i = 1;
                    for (; i < lines.Count; i++)
                    {
                        var row = new EquationRow(Owner, this);
                        row.ConsumeText(lines[i]);
                        childEquations.Insert(activeIndex + i, row);
                        newEquations.Add(row);
                    }
                    UndoManager.DisableAddingActions = false;
                    newEquations.Add(splitRow);
                    ActiveChild = childEquations[activeIndex + lines.Count - 1];
                    ((TextEquation)((EquationRow)ActiveChild).ActiveChild).MoveToEnd();
                    SelectedItems = 0;
                    action.ActiveEquationAfterChange = ActiveChild;
                    UndoManager.AddUndoAction(action);
                }
                CalculateSize();
            }
            else
            {
                base.ConsumeText(text);
            }
        }

        public void DrawVisibleRows(DrawingContext dc, double top, double bottom)
        {
            if (Owner.ViewModel.IsSelecting)
            {
                try { DrawSelectionRegion(dc); }
                catch { }
            }
            foreach (var eb in childEquations)
            {
                if (eb.Bottom >= top)
                {
                    eb.DrawEquation(dc);
                }
                if (eb.Bottom >= bottom)
                {
                    break;
                }
            }
        }

        public override void DrawEquation(DrawingContext dc)
        {
            if (Owner.ViewModel.IsSelecting)
            {
                DrawSelectionRegion(dc);
            }
            base.DrawEquation(dc);
        }

        private void DrawSelectionRegion(DrawingContext dc)
        {
            var topSelectedRowIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
            var topEquation = childEquations[topSelectedRowIndex];
            var rect = topEquation.GetSelectionBounds();
            dc.DrawRectangle(Brushes.LightGray, null, rect);

            var count = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex) - topSelectedRowIndex;
            if (count > 0)
            {
                rect.Union(new Point(topEquation.Right, rect.Bottom + LineSpace + 1));
                dc.DrawRectangle(Brushes.LightGray, null, rect);
                var bottomEquation = childEquations[topSelectedRowIndex + count];
                rect = bottomEquation.GetSelectionBounds();
                rect.Union(new Point(bottomEquation.Left, bottomEquation.Top));
                dc.DrawRectangle(Brushes.LightGray, null, rect);
                for (var i = topSelectedRowIndex + 1; i < topSelectedRowIndex + count; i++)
                {
                    var equation = childEquations[i];
                    rect = equation.Bounds;
                    rect.Union(new Point(rect.Left, rect.Bottom + LineSpace + 1));
                    dc.DrawRectangle(Brushes.LightGray, null, rect);
                }
            }
        }

        public override void RemoveSelection(bool registerUndo)
        {
            if (SelectedItems != 0)
            {
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var endIndex = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex);
                var firstRow = (EquationRow)childEquations[startIndex];
                var lastRow = (EquationRow)childEquations[endIndex];
                var firstText = firstRow.GetFirstSelectionText();
                var lastText = lastRow.GetLastSelectionText();
                List<EquationBase> equations = [];
                var action = new RowContainerRemoveAction(this)
                {
                    ActiveEquation = ActiveChild,
                    HeadEquationRow = firstRow,
                    TailEquationRow = lastRow,
                    HeadTextEquation = firstText,
                    TailTextEquation = lastText,
                    SelectionStartIndex = SelectionStartIndex,
                    SelectedItems = SelectedItems,
                    FirstRowActiveIndex = firstRow.ActiveChildIndex,
                    FirstRowSelectionIndex = firstRow.SelectionStartIndex,
                    FirstRowSelectedItems = firstRow.SelectedItems,
                    LastRowActiveIndex = lastRow.ActiveChildIndex,
                    LastRowSelectionIndex = lastRow.SelectionStartIndex,
                    LastRowSelectedItems = lastRow.SelectedItems,
                    FirstTextCaretIndex = firstText.CaretIndex,
                    LastTextCaretIndex = lastText.CaretIndex,
                    FirstTextSelectionIndex = firstText.SelectionStartIndex,
                    LastTextSelectionIndex = lastText.SelectionStartIndex,
                    FirstTextSelectedItems = firstText.SelectedItems,
                    LastTextSelectedItems = lastText.SelectedItems,
                    FirstText = firstText.Text,
                    LastText = lastText.Text,
                    FirstFormats = firstText.GetFormats(),
                    LastFormats = lastText.GetFormats(),
                    FirstModes = firstText.GetModes(),
                    LastModes = lastText.GetModes(),
                    FirstRowDeletedContent = firstRow.DeleteTail(),
                    LastRowDeletedContent = lastRow.DeleteHead(),
                    FirstDecorations = firstRow.GetFirstTextEquation().GetDecorations(),
                    LastDecorations = lastRow.GetLastTextEquation().GetDecorations(),
                    Equations = equations,
                    FirstRowActiveIndexAfterRemoval = firstRow.ActiveChildIndex
                };
                firstText.RemoveSelection(false); //.DeleteSelectedText();
                lastText.RemoveSelection(false); //.DeleteSelectedText();                
                firstRow.Merge(lastRow);
                for (var i = endIndex; i > startIndex; i--)
                {
                    equations.Add(childEquations[i]);
                    childEquations.RemoveAt(i);
                }
                SelectedItems = 0;
                equations.Reverse();
                ActiveChild = firstRow;
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

        public override CopyDataObject? Copy(bool removeSelection)
        {
            if (SelectedItems != 0)
            {
                // Prepare information for copy
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var count = (SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex) - startIndex;

                var firstRow = (EquationRow)childEquations[startIndex];
                var lastRow = (EquationRow)childEquations[startIndex + count];
                var firstRowSelectedItems = firstRow.GetSelectedEquations();
                var lastRowSelectedItems = lastRow.GetSelectedEquations();

                var newFirstRow = new EquationRow(Owner, this);
                var newLastRow = new EquationRow(Owner, this);
                newFirstRow.GetFirstTextEquation().ConsumeFormattedText(firstRowSelectedItems.First().GetSelectedText(),
                                                                        ((TextEquation)firstRowSelectedItems.First()).GetSelectedFormats(),
                                                                        ((TextEquation)firstRowSelectedItems.First()).GetSelectedModes(),
                                                                        ((TextEquation)firstRowSelectedItems.First()).GetSelectedDecorations(), false);
                newLastRow.GetFirstTextEquation().ConsumeFormattedText(lastRowSelectedItems.Last().GetSelectedText(),
                                                                       ((TextEquation)lastRowSelectedItems.Last()).GetSelectedFormats(),
                                                                       ((TextEquation)lastRowSelectedItems.Last()).GetSelectedModes(),
                                                                       ((TextEquation)lastRowSelectedItems.First()).GetSelectedDecorations(),
                                                                       false);

                firstRowSelectedItems.RemoveAt(0);
                lastRowSelectedItems.RemoveAt(lastRowSelectedItems.Count - 1);
                newFirstRow.AddChildren(firstRowSelectedItems, false);
                newLastRow.AddChildren(lastRowSelectedItems, true);
                var equations = new List<EquationBase>();
                for (var i = startIndex + 1; i < startIndex + count; i++)
                {
                    equations.Add(childEquations[i]);
                }
                equations.Add(newLastRow);
                foreach (var eb in equations)
                {
                    eb.Left = 1;
                }
                var left = firstRow.GetFirstSelectionText().Right - Left;
                var firstTextRect = firstRow.GetFirstSelectionText().GetSelectionBounds();
                if (!firstTextRect.IsEmpty)
                {
                    left = firstTextRect.Left - Left;
                }
                newFirstRow.Left = left + 1;
                equations.Insert(0, newFirstRow);

                // Create image if needed
                RenderTargetBitmap? bitmap = null;
                if (App.Settings.CopyType == CopyType.Image)
                {
                    double nextY = 1;
                    var width = firstRow.Width;
                    double height = 0;
                    foreach (var eb in equations)
                    {
                        eb.Top = nextY;
                        nextY += eb.Height + LineSpace;
                        width = eb.Width > width ? eb.Width : width;
                        height += eb.Height + LineSpace;
                    }
                    height -= LineSpace;

                    bitmap = new RenderTargetBitmap((int)(Math.Ceiling(width + 2)), (int)(Math.Ceiling(height + 2)), 96, 96, PixelFormats.Default);
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
                    copyText = LatexConverter.EscapeRows(latexList)?.ToString();
                }

                // Create XML element
                var thisElement = new XElement(GetType().Name);
                var children = new XElement("ChildRows");
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

        public override void SelectAll()
        {
            base.SelectAll();
            ((EquationRow)childEquations.Last()).MoveToEnd();
        }

        public override string GetSelectedText()
        {
            var stringBulider = new StringBuilder("");
            foreach (var eb in childEquations)
            {
                stringBulider.Append(eb.GetSelectedText() + Environment.NewLine);
            }
            return stringBulider.ToString();
        }

        public override bool Select(Key key)
        {
            if (key == Key.Left)
            {
                if (ActiveChild.Select(key))
                {
                    return true;
                }
                else if (ActiveChild != childEquations.First())
                {
                    ActiveChild = childEquations[childEquations.IndexOf(ActiveChild) - 1];
                    SelectedItems--;
                    if (SelectedItems < 0)
                    {
                        ((EquationRow)ActiveChild).MoveToEnd();
                        ActiveChild.StartSelection();
                    }
                    return true;
                }
            }
            else if (key == Key.Right)
            {
                if (ActiveChild.Select(key))
                {
                    return true;
                }
                else if (ActiveChild != childEquations.Last())
                {
                    ActiveChild = childEquations[childEquations.IndexOf(ActiveChild) + 1];
                    SelectedItems++;
                    if (SelectedItems > 0)
                    {
                        ((EquationRow)ActiveChild).MoveToStart();
                        ActiveChild.StartSelection();
                    }
                    return true;
                }
            }
            else if (key == Key.Home)
            {
                if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && SelectionStartIndex > 0)
                {
                    ((EquationRow)childEquations[SelectionStartIndex]).SelectToStart();
                    for (var i = SelectionStartIndex - 1; i >= 0; i--)
                    {
                        ((EquationRow)childEquations[i]).MoveToEnd();
                        childEquations[i].StartSelection();
                        ((EquationRow)childEquations[i]).SelectToStart();
                    }
                    SelectedItems = -SelectionStartIndex;
                    ActiveChild = childEquations.First();
                }
                else
                {
                    ((EquationRow)ActiveChild).SelectToStart();
                }
                return true;
            }
            else if (key == Key.End)
            {
                if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && SelectionStartIndex < childEquations.Count - 1)
                {
                    ((EquationRow)childEquations[SelectionStartIndex]).SelectToEnd();
                    for (var i = SelectionStartIndex + 1; i < childEquations.Count; i++)
                    {
                        ((EquationRow)childEquations[i]).MoveToStart();
                        childEquations[i].StartSelection();
                        ((EquationRow)childEquations[i]).SelectToEnd();
                    }
                    SelectedItems = childEquations.Count - SelectionStartIndex - 1;
                    ActiveChild = childEquations.Last();
                }
                else
                {
                    ((EquationRow)ActiveChild).SelectToEnd();
                }
                return true;
            }
            else if (key == Key.Up && SelectionStartIndex >= 0 && childEquations.IndexOf(ActiveChild) > 0)
            {
                var point = childEquations[SelectionStartIndex].GetVerticalCaretLocation();
                ActiveChild = childEquations[childEquations.IndexOf(ActiveChild) - 1];
                ((EquationRow)childEquations[SelectionStartIndex]).SelectToStart();
                for (var i = SelectionStartIndex - 1; i > childEquations.IndexOf(ActiveChild); i--)
                {
                    ((EquationRow)childEquations[i]).MoveToEnd();
                    childEquations[i].StartSelection();
                    ((EquationRow)childEquations[i]).SelectToStart();
                }
                point.Y = ActiveChild.MidY;
                ((EquationRow)ActiveChild).MoveToEnd();
                ActiveChild.StartSelection();
                ActiveChild.HandleMouseDrag(point);
                SelectedItems = childEquations.IndexOf(ActiveChild) - SelectionStartIndex;
                return true;
            }
            else if (key == Key.Down && SelectionStartIndex < childEquations.Count && childEquations.IndexOf(ActiveChild) < childEquations.Count - 1)
            {
                var point = childEquations[SelectionStartIndex].GetVerticalCaretLocation();
                ActiveChild = childEquations[childEquations.IndexOf(ActiveChild) + 1];
                ((EquationRow)childEquations[SelectionStartIndex]).SelectToEnd();
                for (var i = SelectionStartIndex + 1; i < childEquations.Count; i++)
                {
                    ((EquationRow)childEquations[i]).MoveToStart();
                    childEquations[i].StartSelection();
                    ((EquationRow)childEquations[i]).SelectToEnd();
                }
                point.Y = ActiveChild.MidY;
                ((EquationRow)ActiveChild).MoveToStart();
                ActiveChild.StartSelection();
                ActiveChild.HandleMouseDrag(point);
                SelectedItems = childEquations.IndexOf(ActiveChild) - SelectionStartIndex;
                return true;
            }
            return false;
        }

        public RowContainer(MainWindow owner, EquationContainer parent, double lineSpaceFactor = 0)
            : base(owner, parent)
        {
            var newLine = new EquationRow(owner, this);
            AddLine(newLine);
            Height = newLine.Height;
            Width = newLine.Width;
            this.lineSpaceFactor = lineSpaceFactor;
        }

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var children = new XElement("ChildRows");
            foreach (var childRow in childEquations)
            {
                children.Add(childRow.Serialize());
            }
            thisElement.Add(children);
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            var children = xElement.Element("ChildRows") ?? throw new Exception("Invalid XML format");
            childEquations.Clear();
            foreach (var xe in children.Elements())
            {
                var row = new EquationRow(Owner, this);
                row.DeSerialize(xe);
                childEquations.Add(row);
            }
            if (childEquations.Count == 0)
            {
                childEquations.Add(new EquationRow(Owner, this));
            }
            ActiveChild = childEquations.First();
            CalculateSize();
        }

        public override StringBuilder? ToLatex()
        {
            if (childEquations.Count == 0) return null;
            var sb = new StringBuilder();
            foreach (var childRow in childEquations)
            {
                sb.Append(childRow.ToLatex());
            }
            return sb;
        }

        private void AddLine(EquationRow newRow)
        {
            var index = 0;
            if (childEquations.Count > 0)
            {
                index = childEquations.IndexOf((EquationRow)ActiveChild) + 1;
            }
            childEquations.Insert(index, newRow);
            ActiveChild = newRow;
            CalculateSize();
        }

        public override EquationBase Split(EquationContainer newParent)
        {
            var newRow = (EquationRow)ActiveChild.Split(this);
            if (newRow != null)
            {
                var activeRow = ActiveChild as EquationRow;
                var rca = new RowContainerAction(this, childEquations.IndexOf(activeRow), activeRow.ActiveChildIndex, activeRow.TextLength, newRow) { UndoFlag = false };
                UndoManager.AddUndoAction(rca);
                AddLine(newRow);
            }
            CalculateSize();
            return newRow;
        }

        public override bool ConsumeMouseClick(Point mousePoint)
        {
            foreach (var eb in childEquations)
            {
                var rect = new Rect(0, eb.Top, double.MaxValue, eb.Height);
                if (rect.Contains(mousePoint))
                {
                    ActiveChild = eb;
                    return eb.ConsumeMouseClick(mousePoint);
                }
            }
            return false;
        }

        public override void HandleMouseDrag(Point mousePoint)
        {
            if (mousePoint.Y > ActiveChild.Top - LineSpace && mousePoint.Y < ActiveChild.Bottom + LineSpace)
            {
                ActiveChild.HandleMouseDrag(mousePoint);
            }
            else
            {
                if (mousePoint.Y > ActiveChild.Bottom + LineSpace)
                {
                    for (var i = childEquations.IndexOf(ActiveChild) + 1; i < childEquations.Count; i++)
                    {
                        ActiveChild = childEquations[i];
                        if (ActiveChild.Top <= mousePoint.Y && ActiveChild.Bottom + LineSpace >= mousePoint.Y)
                        {
                            break;
                        }
                    }
                    for (var i = SelectionStartIndex + 1; i < childEquations.IndexOf(ActiveChild); i++)
                    {
                        ((EquationRow)childEquations[i]).MoveToStart();
                        childEquations[i].StartSelection();
                        ((EquationRow)childEquations[i]).SelectToEnd();
                    }
                    if (childEquations.IndexOf(ActiveChild) > SelectionStartIndex)
                    {
                        ((EquationRow)childEquations[SelectionStartIndex]).SelectToEnd();
                        var row = ActiveChild as EquationRow;
                        row.MoveToStart();
                        row.StartSelection();
                    }

                }
                else if (mousePoint.Y < ActiveChild.Top - LineSpace)
                {
                    for (var i = childEquations.IndexOf(ActiveChild) - 1; i >= 0; i--)
                    {
                        ActiveChild = childEquations[i];
                        if (ActiveChild.Top - LineSpace <= mousePoint.Y && ActiveChild.Bottom >= mousePoint.Y)
                        {
                            break;
                        }
                    }
                    for (var i = SelectionStartIndex - 1; i > childEquations.IndexOf(ActiveChild); i--)
                    {
                        ((EquationRow)childEquations[i]).MoveToEnd();
                        childEquations[i].StartSelection();
                        ((EquationRow)childEquations[i]).SelectToStart();
                    }
                    if (childEquations.IndexOf(ActiveChild) < SelectionStartIndex)
                    {
                        ((EquationRow)childEquations[SelectionStartIndex]).SelectToStart();
                        var row = ActiveChild as EquationRow;
                        row.MoveToEnd();
                        row.StartSelection();
                    }
                }
                ActiveChild.HandleMouseDrag(mousePoint);
                SelectedItems = childEquations.IndexOf(ActiveChild) - SelectionStartIndex;
            }
            Owner.SetStatusBarMessage("ActiveStart " + ActiveChild.SelectionStartIndex + ", ActiveItems" + ActiveChild.SelectedItems);
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                if (HAlignment == HAlignment.Left)
                {
                    foreach (var eb in childEquations)
                    {
                        eb.Left = value;
                    }
                }
                else if (HAlignment == HAlignment.Right)
                {
                    foreach (var eb in childEquations)
                    {
                        eb.Right = Right;
                    }
                }
                else if (HAlignment == HAlignment.Center)
                {
                    foreach (var eb in childEquations)
                    {
                        eb.MidX = MidX;
                    }
                }
            }
        }

        public override double Top
        {
            get => base.Top;
            set
            {
                base.Top = value;
                var nextY = value;
                foreach (var eb in childEquations)
                {
                    eb.Top = nextY;
                    nextY += eb.Height + LineSpace;
                }
            }
        }

        public override bool ConsumeKey(Key key)
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                if (key == Key.Home)
                {
                    ActiveChild = childEquations.First();
                    ((EquationRow)ActiveChild).MoveToStart();
                }
                else if (key == Key.End)
                {
                    ActiveChild = childEquations.Last();
                    ((EquationRow)ActiveChild).MoveToEnd();
                }
                return true;
            }
            var result = false;
            if (ActiveChild.ConsumeKey(key))
            {
                result = true;
            }
            else if (key == Key.Enter)
            {
                Split(this);
                ((EquationRow)ActiveChild).MoveToStart();
                result = true;
            }
            else if (key == Key.Delete)
            {
                if (ActiveChild != childEquations.Last())
                {
                    var activeRow = ActiveChild as EquationRow;
                    var rowToRemove = (EquationRow)childEquations[childEquations.IndexOf(activeRow) + 1];
                    UndoManager.AddUndoAction(new RowContainerAction(this, childEquations.IndexOf(activeRow), activeRow.ActiveChildIndex, activeRow.TextLength, rowToRemove));
                    activeRow.Merge(rowToRemove);
                    childEquations.RemoveAt(childEquations.IndexOf(activeRow) + 1);
                }
                result = true;
            }
            else if (!result)
            {
                if (key == Key.Up && ActiveChild != childEquations.First())
                {
                    var point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = childEquations[childEquations.IndexOf(ActiveChild) - 1];
                    point.Y = ActiveChild.Bottom - 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    result = true;
                }
                else if (key == Key.Down && ActiveChild != childEquations.Last())
                {
                    var point = ActiveChild.GetVerticalCaretLocation();
                    ActiveChild = childEquations[childEquations.IndexOf(ActiveChild) + 1];
                    point.Y = ActiveChild.Top + 1;
                    ActiveChild.SetCursorOnKeyUpDown(key, point);
                    result = true;
                }
                else if (key == Key.Left)
                {
                    if (ActiveChild != childEquations.First())
                    {
                        ActiveChild = childEquations[childEquations.IndexOf((EquationRow)ActiveChild) - 1];
                        result = true;
                    }
                }
                else if (key == Key.Right)
                {
                    if (ActiveChild != childEquations.Last())
                    {
                        ActiveChild = childEquations[childEquations.IndexOf((EquationRow)ActiveChild) + 1];
                        result = true;
                    }
                }
                else if (key == Key.Back)
                {
                    if (ActiveChild != childEquations.First())
                    {
                        var activeRow = ActiveChild as EquationRow;
                        var previousRow = (EquationRow)childEquations[childEquations.IndexOf(activeRow) - 1];
                        var index = previousRow.ActiveChildIndex;
                        previousRow.MoveToEnd();
                        UndoManager.AddUndoAction(new RowContainerAction(this, childEquations.IndexOf(previousRow), previousRow.ActiveChildIndex, previousRow.TextLength, activeRow));
                        previousRow.Merge(activeRow);
                        childEquations.Remove(activeRow);
                        ActiveChild = previousRow;
                        result = true;
                    }
                }
            }
            CalculateSize();
            return result;
        }

        protected override void CalculateWidth()
        {
            double maxLeftHalf = 0;
            double maxRightHalf = 0;
            foreach (var eb in childEquations)
            {
                if (eb.RefX > maxLeftHalf)
                {
                    maxLeftHalf = eb.RefX;
                }
                if (eb.Width - eb.RefX > maxRightHalf)
                {
                    maxRightHalf = eb.Width - eb.RefX;
                }
                eb.Left = Left;
            }
            Width = maxLeftHalf + maxRightHalf;
        }

        protected override void CalculateHeight()
        {
            double height = 0;
            foreach (var eb in childEquations)
            {
                height += eb.Height + LineSpace;
            }
            Height = height;
            var nextY = Top;
            foreach (var eb in childEquations)
            {
                eb.Top = nextY;
                nextY += eb.Height + LineSpace;
            }
        }

        public override double RefY
        {
            get
            {
                var count = childEquations.Count;
                if (count == 1)
                {
                    return childEquations.First().RefY;
                }
                else if (count % 2 == 0)
                {
                    return childEquations[(count + 1) / 2].Top - Top - LineSpace / 2;
                }
                else
                {
                    return childEquations[count / 2].MidY - Top;
                    //base.RefY;
                }
            }
        }

        public void ProcessUndo(EquationAction action)
        {
            var type = action.GetType();
            if (type == typeof(RowContainerAction))
            {
                ProcessRowContainerAction(action);
                Owner.ViewModel.IsSelecting = false;
            }
            else if (type == typeof(RowContainerTextAction))
            {
                ProcessRowContainerTextAction(action);
            }
            else if (type == typeof(RowContainerPasteAction))
            {
                ProcessRowContainerPasteAction(action);
            }
            else if (type == typeof(RowContainerFormatAction))
            {
                ProcessRowContainerFormatAction(action);
            }
            else if (type == typeof(RowContainerRemoveAction))
            {
                ProcessRowContainerRemoveAction(action);
            }
            CalculateSize();
            ParentEquation.ChildCompletedUndo(this);
        }

        private void ProcessRowContainerPasteAction(EquationAction action)
        {
            var pasteAction = action as RowContainerPasteAction;
            var activeRow = (EquationRow)pasteAction.ActiveEquation;
            if (pasteAction.UndoFlag)
            {
                SelectedItems = pasteAction.SelectedItems;
                SelectionStartIndex = pasteAction.SelectionStartIndex;
                pasteAction.ActiveTextInChildRow.ResetTextEquation(pasteAction.CaretIndexOfActiveText, pasteAction.SelectionStartIndexOfTextEquation,
                                                                   pasteAction.SelectedItemsOfTextEquation, pasteAction.TextEquationContents,
                                                                   pasteAction.TextEquationFormats, pasteAction.TextEquationModes,
                                                                   pasteAction.TextEquationDecorations);
                activeRow.ResetRowEquation(pasteAction.ActiveTextInChildRow, pasteAction.ActiveEquationSelectionIndex, pasteAction.ActiveEquationSelectedItems);
                activeRow.Truncate();
                activeRow.Merge(pasteAction.Equations.Last());
                foreach (EquationBase eb in pasteAction.Equations)
                {
                    childEquations.Remove(eb);
                }
                activeRow.CalculateSize();
                ActiveChild = activeRow;
            }
            else
            {
                activeRow.ResetRowEquation(pasteAction.ActiveTextInChildRow, pasteAction.ActiveEquationSelectionIndex, pasteAction.ActiveEquationSelectedItems);
                var newRow = (EquationRow)activeRow.Split(this);
                pasteAction.Equations[^2].GetLastTextEquation().SetFormattedText(pasteAction.TailTextOfPastedRows, pasteAction.TailFormatsOfPastedRows, pasteAction.TailModesOfPastedRows);
                activeRow.Merge(pasteAction.Equations.First());
                var index = childEquations.IndexOf(ActiveChild) + 1;
                childEquations.InsertRange(index, pasteAction.Equations.Skip(1));
                childEquations.RemoveAt(childEquations.Count - 1);
                pasteAction.Equations[^2].Merge(newRow);
                ActiveChild = childEquations[index + pasteAction.Equations.Count - 3];
                ((EquationRow)ActiveChild).MoveToEnd();
                FontSize = FontSize;
                SelectedItems = 0;
            }
        }

        private void ProcessRowContainerTextAction(EquationAction action)
        {
            var textAction = action as RowContainerTextAction;
            ActiveChild = textAction.ActiveEquation;
            var activeRow = (EquationRow)ActiveChild;
            activeRow.ResetRowEquation(textAction.ActiveTextInRow, textAction.ActiveEquationSelectionIndex, textAction.ActiveEquationSelectedItems);
            if (textAction.UndoFlag)
            {
                textAction.ActiveTextInRow.ResetTextEquation(textAction.CaretIndexOfActiveText, textAction.SelectionStartIndexOfTextEquation, textAction.SelectedItemsOfTextEquation, textAction.TextEquationContents, textAction.TextEquationFormats, textAction.TextEquationModes, textAction.TextEquationDecoration);
                UndoManager.DisableAddingActions = true;
                ActiveChild.ConsumeFormattedText(textAction.FirstLineOfInsertedText, textAction.FirstFormatsOfInsertedText, textAction.FirstModesOfInsertedText, textAction.FirstDecorationsOfInsertedText, false);
                UndoManager.DisableAddingActions = false;
                var splitRow = (EquationRow)ActiveChild.Split(this);
                childEquations.InsertRange(childEquations.IndexOf(ActiveChild) + 1, textAction.Equations);
                if (splitRow.IsEmpty)
                {
                    childEquations.Remove(textAction.Equations.Last());
                }
                ActiveChild = textAction.ActiveEquationAfterChange!;
                textAction.ActiveTextInRow.MoveToEnd();
                SelectedItems = 0;
            }
            else
            {
                SelectedItems = textAction.SelectedItems;
                SelectionStartIndex = textAction.SelectionStartIndex;
                activeRow.Merge(textAction.Equations.Last());
                textAction.ActiveTextInRow.ResetTextEquation(textAction.CaretIndexOfActiveText, textAction.SelectionStartIndexOfTextEquation,
                                                             textAction.SelectedItemsOfTextEquation, textAction.TextEquationContents,
                                                             textAction.TextEquationFormats, textAction.FirstModesOfInsertedText,
                                                             textAction.FirstDecorationsOfInsertedText);
                foreach (EquationBase eb in textAction.Equations)
                {
                    childEquations.Remove(eb);
                }
            }
            activeRow.CalculateSize();
        }

        private void ProcessRowContainerRemoveAction(EquationAction action)
        {
            var rowAction = action as RowContainerRemoveAction;
            if (rowAction.UndoFlag)
            {
                childEquations.InsertRange(childEquations.IndexOf(rowAction.HeadEquationRow) + 1, rowAction.Equations);
                rowAction.HeadEquationRow.ActiveChildIndex = rowAction.FirstRowActiveIndexAfterRemoval;
                rowAction.HeadEquationRow.Truncate();
                rowAction.HeadEquationRow.ResetRowEquation(rowAction.FirstRowActiveIndex, rowAction.FirstRowSelectionIndex, rowAction.FirstRowSelectedItems, rowAction.FirstRowDeletedContent, true);
                rowAction.TailEquationRow.ResetRowEquation(rowAction.LastRowActiveIndex, rowAction.LastRowSelectionIndex, rowAction.LastRowSelectedItems, rowAction.LastRowDeletedContent, false);
                rowAction.HeadTextEquation.ResetTextEquation(rowAction.FirstTextCaretIndex, rowAction.FirstTextSelectionIndex, rowAction.FirstTextSelectedItems, rowAction.FirstText, rowAction.FirstFormats, rowAction.FirstModes, rowAction.FirstDecorations);
                rowAction.TailTextEquation.ResetTextEquation(rowAction.LastTextCaretIndex, rowAction.LastTextSelectionIndex, rowAction.LastTextSelectedItems, rowAction.LastText, rowAction.LastFormats, rowAction.LastModes, rowAction.LastDecorations);
                foreach (var eb in rowAction.Equations)
                {
                    eb.FontSize = FontSize;
                }
                rowAction.HeadEquationRow.FontSize = FontSize;
                rowAction.TailEquationRow.FontSize = FontSize;
                SelectedItems = rowAction.SelectedItems;
                SelectionStartIndex = rowAction.SelectionStartIndex;
                ActiveChild = rowAction.ActiveEquation;
                Owner.ViewModel.IsSelecting = true;
            }
            else
            {
                rowAction.HeadEquationRow.ResetRowEquation(rowAction.FirstRowActiveIndex, rowAction.FirstRowSelectionIndex, rowAction.FirstRowSelectedItems);
                rowAction.TailEquationRow.ResetRowEquation(rowAction.LastRowActiveIndex, rowAction.LastRowSelectionIndex, rowAction.LastRowSelectedItems);
                rowAction.HeadTextEquation.ResetTextEquation(rowAction.FirstTextCaretIndex, rowAction.FirstTextSelectionIndex, rowAction.FirstTextSelectedItems, rowAction.FirstText, rowAction.FirstFormats, rowAction.FirstModes, rowAction.FirstDecorations);
                rowAction.TailTextEquation.ResetTextEquation(rowAction.LastTextCaretIndex, rowAction.LastTextSelectionIndex, rowAction.LastTextSelectedItems, rowAction.LastText, rowAction.LastFormats, rowAction.LastModes, rowAction.LastDecorations);
                rowAction.HeadTextEquation.RemoveSelection(false); //.DeleteSelectedText();
                rowAction.TailTextEquation.RemoveSelection(false); //.DeleteSelectedText();
                rowAction.HeadEquationRow.DeleteTail();
                rowAction.TailEquationRow.DeleteHead();
                rowAction.HeadEquationRow.Merge(rowAction.TailEquationRow);
                var index = childEquations.IndexOf(rowAction.HeadEquationRow);
                for (var i = index + rowAction.Equations.Count; i > index; i--)
                {
                    childEquations.RemoveAt(i);
                }
                ActiveChild = rowAction.HeadEquationRow;
                SelectedItems = 0;
                Owner.ViewModel.IsSelecting = false;
            }
        }

        private void ProcessRowContainerAction(EquationAction action)
        {
            var containerAction = action as RowContainerAction;
            if (containerAction.UndoFlag)
            {
                var activeRow = (EquationRow)childEquations[containerAction.Index];
                activeRow.SetCurrentChild(containerAction.ChildIndexInRow, containerAction.CaretIndex);
                activeRow.Truncate(containerAction.ChildIndexInRow + 1, containerAction.CaretIndex);
                childEquations.Insert(containerAction.Index + 1, containerAction.Equation);
                ActiveChild = containerAction.Equation;
                ActiveChild.FontSize = this.FontSize;
            }
            else
            {
                ((EquationRow)childEquations[containerAction.Index]).Merge((EquationRow)childEquations[containerAction.Index + 1]);
                childEquations.Remove(childEquations[containerAction.Index + 1]);
                ActiveChild = childEquations[containerAction.Index];
                ((EquationRow)ActiveChild).SetCurrentChild(containerAction.ChildIndexInRow, containerAction.CaretIndex);
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
                        var ecfa = new RowContainerFormatAction(this)
                        {
                            Operation = operation,
                            Argument = argument,
                            Applied = applied,
                            SelectionStartIndex = SelectionStartIndex,
                            SelectedItems = SelectedItems,
                            ActiveChild = ActiveChild,
                            FirstRowActiveChildIndex = ((EquationRow)childEquations[startIndex]).ActiveChildIndex,
                            FirstRowSelectionStartIndex = childEquations[startIndex].SelectionStartIndex,
                            FirstRowSelectedItems = childEquations[startIndex].SelectedItems,
                            LastRowActiveChildIndex = ((EquationRow)childEquations[endIndex]).ActiveChildIndex,
                            LastRowSelectionStartIndex = childEquations[endIndex].SelectionStartIndex,
                            LastRowSelectedItems = childEquations[endIndex].SelectedItems,
                            FirstTextCaretIndex = ((EquationRow)childEquations[startIndex]).GetFirstSelectionText().CaretIndex,
                            FirstTextSelectionStartIndex = ((EquationRow)childEquations[startIndex]).GetFirstSelectionText().SelectionStartIndex,
                            FirstTextSelectedItems = ((EquationRow)childEquations[startIndex]).GetFirstSelectionText().SelectedItems,
                            LastTextCaretIndex = ((EquationRow)childEquations[endIndex]).GetLastSelectionText().CaretIndex,
                            LastTextSelectionStartIndex = ((EquationRow)childEquations[endIndex]).GetLastSelectionText().SelectionStartIndex,
                            LastTextSelectedItems = ((EquationRow)childEquations[endIndex]).GetLastSelectionText().SelectedItems,
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

        private void ProcessRowContainerFormatAction(EquationAction action)
        {
            if (action is RowContainerFormatAction rcfa)
            {
                Owner.ViewModel.IsSelecting = true;
                ActiveChild = rcfa.ActiveChild;
                this.SelectedItems = rcfa.SelectedItems;
                this.SelectionStartIndex = rcfa.SelectionStartIndex;
                var startIndex = SelectedItems > 0 ? SelectionStartIndex : SelectionStartIndex + SelectedItems;
                var endIndex = SelectedItems > 0 ? SelectionStartIndex + SelectedItems : SelectionStartIndex;
                ((EquationRow)childEquations[startIndex]).ActiveChildIndex = rcfa.FirstRowActiveChildIndex;
                childEquations[startIndex].SelectionStartIndex = rcfa.FirstRowSelectionStartIndex;
                childEquations[startIndex].SelectedItems = rcfa.FirstRowSelectedItems;
                ((EquationRow)childEquations[endIndex]).ActiveChildIndex = rcfa.LastRowActiveChildIndex;
                childEquations[endIndex].SelectionStartIndex = rcfa.LastRowSelectionStartIndex;
                childEquations[endIndex].SelectedItems = rcfa.LastRowSelectedItems;
                ((EquationRow)childEquations[startIndex]).GetFirstTextEquation().CaretIndex = rcfa.FirstTextCaretIndex;
                ((EquationRow)childEquations[startIndex]).GetFirstTextEquation().SelectionStartIndex = rcfa.FirstTextSelectionStartIndex;
                ((EquationRow)childEquations[startIndex]).GetFirstTextEquation().SelectedItems = rcfa.FirstTextSelectedItems;
                ((EquationRow)childEquations[endIndex]).GetLastTextEquation().CaretIndex = rcfa.LastTextCaretIndex;
                ((EquationRow)childEquations[endIndex]).GetLastTextEquation().SelectionStartIndex = rcfa.LastTextSelectionStartIndex;
                ((EquationRow)childEquations[endIndex]).GetLastTextEquation().SelectedItems = rcfa.LastTextSelectedItems;
                for (var i = startIndex; i <= endIndex; i++)
                {
                    if (i > startIndex && i < endIndex)
                    {
                        childEquations[i].SelectAll();
                    }
                    childEquations[i].ModifySelection(rcfa.Operation, rcfa.Argument, rcfa.UndoFlag ? !rcfa.Applied : rcfa.Applied, false);
                }
                CalculateSize();
                ParentEquation.ChildCompletedUndo(this);
            }
        }
    }
}
