using Avalonia;
using Avalonia.Media;

namespace Editor
{
    public class StaticText : EquationBase
    {
        protected string Text { get; set; } = string.Empty;
        protected FontType FontType { get; set; }
        protected double FontSizeFactor = 1;
        protected FontWeight FontWeight = FontWeight.Normal;
        protected double TopOffestFactor = 0;
        protected double LeftMarginFactor = 0;
        protected double RightMarginFactor = 0;

        private FormattedTextExtended _formattedTextExtended = null!;

        public StaticText(IMainWindow owner, EquationContainer parent)
            : base(owner, parent)
        {
            IsStatic = true;
        }

        public override void DrawEquation(DrawingContext dc, bool forceBlackBrush)
        {
            //dc.DrawText(formattedTextExtended, new Point(Left + LeftMarginFactor * FontSize, Top + TopOffestFactor * Height));
            _formattedTextExtended.DrawTextTopLeftAligned(dc, new Point(Left + LeftMarginFactor * FontSize, Top + TopOffestFactor * Height), forceBlackBrush);
        }

        public override double FontSize
        {
            get => base.FontSize;
            set
            {
                base.FontSize = value;
                ReformatSign();
            }
        }

        protected void ReformatSign()
        {
            _formattedTextExtended = FontFactory.GetFormattedTextExtended(Text, FontType, FontSize * FontSizeFactor, FontWeight, false);
            Width = _formattedTextExtended.GetFullWidth() + LeftMarginFactor * FontSize + RightMarginFactor * FontSize; // * WidthFactor;
            Height = _formattedTextExtended.Extent;
        }

        public double OverhangTrailing => _formattedTextExtended.OverhangTrailing;

        public double OverhangLeading => _formattedTextExtended.OverhangLeading;
    }
}
