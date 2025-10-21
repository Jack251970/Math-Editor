using System.Text;

namespace Editor
{
    public sealed class LeftBracket : Bracket
    {
        public LeftBracket(EquationContainer parent, BracketSignType bracketType)
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
                bracketSign.Left = value;
                insideEq.Left = bracketSign.Right;
            }
        }
    }
}
