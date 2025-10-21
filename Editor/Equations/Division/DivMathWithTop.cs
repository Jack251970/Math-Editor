using System.Text;

namespace Editor
{
    public sealed class DivMathWithTop : DivMathWithOuterBase
    {
        public DivMathWithTop(EquationContainer parent)
            : base(parent)
        {
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToDivision(DivisionType.DivMathWithTop, _insideEquation.ToLatex(), outerEquation.ToLatex());
        }

        protected override void AdjustVertical()
        {
            outerEquation.Top = Top;
            _insideEquation.Top = outerEquation.Bottom + VerticalGap;
            _divMathSign.Top = outerEquation.Bottom + VerticalGap;
        }

        public override double RefY => outerEquation.Height + _insideEquation.FirstRow.RefY + VerticalGap;
    }
}
