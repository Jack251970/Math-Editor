using System.Text;

namespace Editor
{
    public sealed class DivMathWithBottom : DivMathWithOuterBase
    {
        public DivMathWithBottom(MainWindow owner, EquationContainer parent)
            : base(owner, parent)
        {
            _divMathSign.IsInverted = true;
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToDivision(DivisionType.DivInvertedWithBottom, _insideEquation.ToLatex(), null);
        }

        protected override void AdjustVertical()
        {
            outerEquation.Bottom = Bottom;
            _insideEquation.Top = Top;
            _divMathSign.Bottom = outerEquation.Top - VerticalGap;
        }

        public override double RefY => _insideEquation.Height - (_insideEquation.FirstRow.Height - _insideEquation.FirstRow.RefY);
    }
}
