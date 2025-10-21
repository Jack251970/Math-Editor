namespace Editor
{
    public sealed class DivMathWithBottom : DivMathWithOuterBase
    {
        public DivMathWithBottom(EquationContainer parent)
            : base(parent)
        {
            _divMathSign.IsInverted = true;
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
