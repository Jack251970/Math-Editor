using System;
using System.Text;

namespace Editor
{
    public sealed class StaticSign : StaticText
    {
        private bool integralSignItalic = false;
        public SignCompositeSymbol Symbol { get; set; }
        public bool UseItalicIntegralSign
        {
            get => integralSignItalic;
            set
            {
                integralSignItalic = value;
                DetermineMargin();
                DetermineFontType();
                ReformatSign();
            }
        }

        public StaticSign(MainWindow owner, EquationContainer parent, SignCompositeSymbol symbol, bool useItalic)
            : base(owner, parent)
        {
            integralSignItalic = useItalic;
            Symbol = symbol;
            DetermineSignString();
            DetermineFontType();
            DetermineFontSizeFactor();
            DetermineMargin();
            ReformatSign();
        }

        private void DetermineMargin()
        {
            switch (Symbol)
            {
                case SignCompositeSymbol.Integral:
                case SignCompositeSymbol.DoubleIntegral:
                case SignCompositeSymbol.TripleIntegral:
                    LeftMarginFactor = 0.02;
                    break;
                case SignCompositeSymbol.ContourIntegral:
                case SignCompositeSymbol.SurfaceIntegral:
                case SignCompositeSymbol.VolumeIntegral:
                case SignCompositeSymbol.ClockContourIntegral:
                case SignCompositeSymbol.AntiClockContourIntegral:
                    RightMarginFactor = .2;
                    LeftMarginFactor = 0.1;
                    break;
                case SignCompositeSymbol.Union:
                case SignCompositeSymbol.Intersection:
                    LeftMarginFactor = 0.1;
                    RightMarginFactor = 0.05;
                    break;
                default:
                    RightMarginFactor = 0.05;
                    break;
            }
        }

        private void DetermineFontType()
        {
            var fontType = FontType.STIXSizeOneSym;
            switch (Symbol)
            {
                case SignCompositeSymbol.Integral:
                case SignCompositeSymbol.DoubleIntegral:
                case SignCompositeSymbol.TripleIntegral:
                case SignCompositeSymbol.ContourIntegral:
                case SignCompositeSymbol.SurfaceIntegral:
                case SignCompositeSymbol.VolumeIntegral:
                case SignCompositeSymbol.ClockContourIntegral:
                case SignCompositeSymbol.AntiClockContourIntegral:
                    if (UseItalicIntegralSign)
                    {
                        fontType = FontType.STIXGeneral;
                    }
                    else
                    {
                        fontType = FontType.STIXIntegralsUp;
                    }
                    break;
                case SignCompositeSymbol.Intersection:
                case SignCompositeSymbol.Union:
                    fontType = FontType.STIXGeneral;
                    break;
            }
            FontType = fontType;
        }

        private void DetermineFontSizeFactor()
        {
            double factor = 1;
            switch (Symbol)
            {
                case SignCompositeSymbol.Integral:
                case SignCompositeSymbol.DoubleIntegral:
                case SignCompositeSymbol.TripleIntegral:
                case SignCompositeSymbol.ContourIntegral:
                case SignCompositeSymbol.SurfaceIntegral:
                case SignCompositeSymbol.VolumeIntegral:
                case SignCompositeSymbol.ClockContourIntegral:
                case SignCompositeSymbol.AntiClockContourIntegral:
                    factor = 1.5;
                    break;
                case SignCompositeSymbol.Intersection:
                case SignCompositeSymbol.Union:
                    factor = 1.2;
                    break;
            }
            FontSizeFactor = factor;
        }

        private void DetermineSignString()
        {
            Text = Symbol switch
            {
                SignCompositeSymbol.Sum => "\u2211",
                SignCompositeSymbol.Product => "\u220F",
                SignCompositeSymbol.CoProduct => "\u2210",
                SignCompositeSymbol.Intersection => "\u22C2",
                SignCompositeSymbol.Union => "\u22C3",
                SignCompositeSymbol.Integral => "\u222B",
                SignCompositeSymbol.DoubleIntegral => "\u222C",
                SignCompositeSymbol.TripleIntegral => "\u222D",
                SignCompositeSymbol.ContourIntegral => "\u222E",
                SignCompositeSymbol.SurfaceIntegral => "\u222F",
                SignCompositeSymbol.VolumeIntegral => "\u2230",
                SignCompositeSymbol.ClockContourIntegral => "\u2232",
                SignCompositeSymbol.AntiClockContourIntegral => "\u2233",
                _ => throw new InvalidOperationException($"Unknown sign composite symbol: {Symbol}"),
            };
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ConvertToLatexSymbol(Text, true);
        }
    }
}
