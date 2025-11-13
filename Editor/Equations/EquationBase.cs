using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Editor
{
    public abstract class EquationBase : EquationBox
    {
        protected TextManager TextManager { get; } = Ioc.Default.GetRequiredService<TextManager>();
        protected LatexConverter LatexConverter { get; } = Ioc.Default.GetRequiredService<LatexConverter>();
        protected UndoManager UndoManager { get; }
        protected const double LineFactor = 0.06;

        public virtual bool ApplySymbolGap { get; set; }

        public virtual HashSet<int>? GetUsedTextFormats() { return null; }
        public virtual void ResetTextFormats(Dictionary<int, int> formatMapping) { }

        protected double LineThickness => fontSize * LineFactor;
        protected double ThinLineThickness => fontSize * LineFactor * 0.7;
        protected Pen BlackStandardPen => PenManager.GetBlackPen(LineThickness);
        protected Pen StandardPen => PenManager.GetPen(LineThickness);

        protected Pen BlackThinPen => PenManager.GetBlackPen(ThinLineThickness);
        protected Pen ThinPen => PenManager.GetPen(ThinLineThickness);

        protected Pen BlackStandardMiterPen => PenManager.GetBlackPen(ThinLineThickness, PenLineJoin.Miter);
        protected Pen StandardMiterPen => PenManager.GetPen(LineThickness, PenLineJoin.Miter);

        protected Pen BlackThinMiterPen => PenManager.GetBlackPen(ThinLineThickness, PenLineJoin.Miter);
        protected Pen ThinMiterPen => PenManager.GetPen(ThinLineThickness, PenLineJoin.Miter);

        protected Pen BlackStandardRoundPen => PenManager.GetBlackPen(LineThickness, PenLineJoin.Round);
        protected Pen StandardRoundPen => PenManager.GetPen(LineThickness, PenLineJoin.Round);

        protected Pen BlackThinRoundPen => PenManager.GetBlackPen(ThinLineThickness, PenLineJoin.Round);
        protected Pen ThinRoundPen => PenManager.GetPen(ThinLineThickness, PenLineJoin.Round);

        public HAlignment HAlignment { get; set; }
        public VAlignment VAlignment { get; set; }
        public bool IsStatic { get; set; }
        public int SubLevel { get; set; }
        protected double SubFontFactor = 0.6;
        protected double SubSubFontFactor = 0.7;

        public IMainWindow Owner { get; set; }
        public EquationContainer ParentEquation { get; set; }
        private readonly Point location = new();
        /*private readonly double width;*/
        private const double height = 0;
        private double fontSize = 20;
        private double fontFactor = 1;
        public int SelectionStartIndex { get; set; }
        public int SelectedItems { get; set; } //this is a directed value (as on a real line!!)

        protected IImmutableBrush debugBrush;
        private readonly byte r = 80;
        private readonly byte g = 80;
        private readonly byte b = 80;

        public EquationBase(IMainWindow owner, EquationContainer parent)
        {
            Owner = owner;
            UndoManager = owner.UndoManager;
            ParentEquation = parent;
            if (parent != null)
            {
                SubLevel = parent.SubLevel;
                fontSize = parent.fontSize;
                ApplySymbolGap = parent.ApplySymbolGap;
                r = (byte)(parent.r + 15);
                g = (byte)(parent.r + 15);
                b = (byte)(parent.r + 15);
            }
            debugBrush = new SolidColorBrush(Color.FromArgb(100, r, g, b)).ToImmutable();
        }

        public virtual bool ConsumeMouseClick(Point mousePoint) { return false; }
        public virtual void HandleMouseDrag(Point mousePoint) { }
        public virtual void HandleMouseDoubleClick(Point mousePoint) { }

        public virtual EquationBase? Split(EquationContainer newParent) { return null; }
        public virtual void ConsumeText(string text) { }
        public virtual void ConsumeFormattedTextExtended(string text, int[] formats, EditorMode[] modes, CharacterDecorationInfo[] decorations, bool addUndo) { }
        public virtual bool ConsumeKey(Key key) { return false; }
        public virtual Point GetVerticalCaretLocation() { return location; }
        public virtual double GetVerticalCaretLength() { return height; }
        protected virtual void CalculateWidth() { }
        protected virtual void CalculateHeight() { }
        public virtual XElement? Serialize() { return null; }
        public virtual void DeSerialize(XElement xElement) { }
        public virtual StringBuilder? ToLatex() { return null; }
        public virtual void StartSelection() { SelectedItems = 0; }
        public virtual bool Select(Key key) { return false; }
        public virtual void DeSelect() { SelectedItems = 0; }
        public virtual void RemoveSelection(bool registerUndo) { }
        public virtual Rect GetSelectionBounds() { return Rect.Empty; }
        public virtual CopyDataObject? Copy(bool removeSelection) { return null; } //copy & cut
        public virtual void Paste(XElement xe) { }
        public virtual void SetCursorOnKeyUpDown(Key key, Point point) { }
        public virtual void ModifySelection(string operation, object argument, bool applied, bool addUndo) { }
        public virtual void ModifySolidBrush() { }

        public virtual void CalculateSize()
        {
            CalculateWidth();
            CalculateHeight();
        }
        public virtual void SelectAll() { }
        public virtual string GetSelectedText() { return string.Empty; }

        public virtual void DrawEquation(DrawingContext dc, bool forceBlackBrush)
        {
            if (App.Settings.ShowNesting)
            {
                dc.DrawRectangle(debugBrush, null, Bounds);
            }
        }

        public virtual double FontFactor
        {
            get => fontFactor;
            set
            {
                fontFactor = value;
                FontSize = fontSize; //fontsize needs adjustement!
            }
        }

        public virtual double FontSize
        {
            get => fontSize; set => fontSize = Math.Min(1000, Math.Max(value * fontFactor, 4));
        }
    }
}
