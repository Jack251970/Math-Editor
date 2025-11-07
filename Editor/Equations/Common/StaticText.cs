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

        private FormattedText _formattedText = null!;

        public StaticText(IMainWindow owner, EquationContainer parent)
            : base(owner, parent)
        {
            IsStatic = true;
        }

        public override void DrawEquation(DrawingContext dc, bool forceBlackBrush)
        {
            //dc.DrawText(formattedText, new Point(Left + LeftMarginFactor * FontSize, Top + TopOffestFactor * Height));
            _formattedText.DrawTextTopLeftAligned(dc, new Point(Left + LeftMarginFactor * FontSize, Top + TopOffestFactor * Height), forceBlackBrush);
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
            _formattedText = FontFactory.GetFormattedText(Text, FontType, FontSize * FontSizeFactor, FontWeight, false);
            Width = _formattedText.GetFullWidth() + LeftMarginFactor * FontSize + RightMarginFactor * FontSize; // * WidthFactor;
            Height = _formattedText.Extent;
        }

        public double OverhangTrailing => _formattedText.OverhangTrailing;

        public double OverhangLeading => _formattedText.OverhangLeading;
    }
}
