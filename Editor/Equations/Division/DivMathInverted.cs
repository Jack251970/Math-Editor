namespace Editor
{
    public sealed class DivMathInverted : DivMath
    {
        public DivMathInverted(EquationContainer parent)
            : base(parent)
        {
            _divMathSign.IsInverted = true;
        }

        protected override void AdjustVertical()
        {
            _insideEquation.Top = Top + VerticalGap;
            _divMathSign.Bottom = Bottom;
        }

        public override double RefY => _insideEquation.LastRow.MidY - Top;
    }
}
