namespace Editor
{
    public sealed class RightBracket : Bracket
    {
        public RightBracket(EquationContainer parent, BracketSignType bracketType)
            : base(parent)
        {
            bracketSign = new BracketSign(this, bracketType);
            childEquations.AddRange([insideEq, bracketSign]);
        }

        public override double Left
        {
            get => base.Left;
            set
            {
                base.Left = value;
                insideEq.Left = value;
                bracketSign.Left = insideEq.Right;
            }
        }
    }
}
