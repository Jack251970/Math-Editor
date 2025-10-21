using System.Text;

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

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToLeftOrRightBracket(bracketSign.SignType, insideEq.ToLatex());
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
