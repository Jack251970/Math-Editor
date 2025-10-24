using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Editor
{
    public abstract class EquationBase : EquationBox
    {
        protected static TextManager TextManager { get; } = new();
        protected LatexConverter LatexConverter = Ioc.Default.GetRequiredService<LatexConverter>();
        protected const double LineFactor = 0.06;

        public virtual bool ApplySymbolGap { get; set; }

        public virtual HashSet<int>? GetUsedTextFormats() { return null; }
        public virtual void ResetTextFormats(Dictionary<int, int> formatMapping) { }

        protected double LineThickness => fontSize * LineFactor;
        protected double ThinLineThickness => fontSize * LineFactor * 0.7;
        protected Pen StandardPen => PenManager.GetPen(LineThickness);
        protected Pen ThinPen => PenManager.GetPen(ThinLineThickness);

        protected Pen StandardMiterPen => PenManager.GetPen(LineThickness, PenLineJoin.Miter);
        protected Pen ThinMiterPen => PenManager.GetPen(ThinLineThickness, PenLineJoin.Miter);

        protected Pen StandardRoundPen => PenManager.GetPen(LineThickness, PenLineJoin.Round);
        protected Pen ThinRoundPen => PenManager.GetPen(ThinLineThickness, PenLineJoin.Round);

        public HAlignment HAlignment { get; set; }
        public VAlignment VAlignment { get; set; }
        public bool IsStatic { get; set; }
        public int SubLevel { get; set; }
        protected double SubFontFactor = 0.6;
        protected double SubSubFontFactor = 0.7;
        public static event EventHandler<EventArgs>? SelectionAvailable;
        public static event EventHandler<EventArgs>? SelectionUnavailable;

        private static bool isSelecting;
        protected static bool IsSelecting
        {
            get => isSelecting;
            set
            {
                isSelecting = value;
                if (isSelecting)
                {
                    SelectionAvailable?.Invoke(null, EventArgs.Empty); //there MUST always be one handler attached!
                }
                else
                {
                    SelectionUnavailable?.Invoke(null, EventArgs.Empty); //there MUST always be one handler attached!
                }
            }
        }

        public static bool ShowNesting { get; set; }
        public MainWindow Owner { get; set; }
        public EquationContainer ParentEquation { get; set; }
        //protected static Pen BluePen = new Pen(Brushes.Blue, 1);
        private Point location = new();
        //Point refPoint = new Point();
        private readonly double width;
        private readonly double height;
        private double fontSize = 20;
        private double fontFactor = 1;
        public int SelectionStartIndex { get; set; }
        public int SelectedItems { get; set; } //this is a directed value (as on a real line!!)

        protected Brush debugBrush;
        private readonly byte r = 80;
        private readonly byte g = 80;
        private readonly byte b = 80;

        public EquationBase(MainWindow owner, EquationContainer parent)
        {
            Owner = owner;
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
            debugBrush = new SolidColorBrush(Color.FromArgb(100, r, g, b));
            debugBrush.Freeze();
        }

        public virtual bool ConsumeMouseClick(Point mousePoint) { return false; }
        public virtual void HandleMouseDrag(Point mousePoint) { }

        public virtual EquationBase? Split(EquationContainer newParent) { return null; }
        public virtual void ConsumeText(string text) { }
        public virtual void ConsumeFormattedText(string text, int[] formats, EditorMode[] modes, CharacterDecorationInfo[] decorations, bool addUndo) { }
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

        public virtual void CalculateSize()
        {
            CalculateWidth();
            CalculateHeight();
        }
        public virtual void SelectAll() { }
        public virtual string GetSelectedText() { return string.Empty; }

        public virtual void DrawEquation(DrawingContext dc)
        {
            if (ShowNesting)
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
