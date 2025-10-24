using System.Text;

namespace Editor
{
    public sealed class DivMathInverted : DivMath
    {
        public DivMathInverted(MainWindow owner, EquationContainer parent)
            : base(owner, parent)
        {
            _divMathSign.IsInverted = true;
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToDivision(DivisionType.DivMathInverted, _insideEquation.ToLatex(), null);
        }

        protected override void AdjustVertical()
        {
            _insideEquation.Top = Top + VerticalGap;
            _divMathSign.Bottom = Bottom;
        }

        public override double RefY => _insideEquation.LastRow.MidY - Top;
    }
}
