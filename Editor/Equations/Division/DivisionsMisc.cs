using System.Text;

namespace Editor
{
    public sealed class DivRegularSmall : DivRegular
    {
        public DivRegularSmall(EquationContainer parent)
            : base(parent, true)
        {
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToDivision(DivisionType.DivRegularSmall, _topEquation.ToLatex(), _bottomEquation.ToLatex());
        }
    }

    public sealed class DivDoubleBar : DivRegular
    {
        public DivDoubleBar(EquationContainer parent)
            : base(parent)
        {
            barCount = 2;
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToDivision(DivisionType.DivDoubleBar, _topEquation.ToLatex(), _bottomEquation.ToLatex());
        }
    }

    public sealed class DivTripleBar : DivRegular
    {
        public DivTripleBar(EquationContainer parent)
            : base(parent)
        {
            barCount = 3;
        }

        public override StringBuilder? ToLatex()
        {
            return LatexConverter.ToDivision(DivisionType.DivTripleBar, _topEquation.ToLatex(), _bottomEquation.ToLatex());
        }
    }
}
