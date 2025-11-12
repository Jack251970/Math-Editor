using System;

namespace Editor
{
    public static class DivisionFactory
    {
        public static EquationBase CreateEquation(IMainWindow owner, EquationContainer equationParent, DivisionType divType)
        {
            EquationBase equation = divType switch
            {
                DivisionType.DivRegular => new DivRegular(owner, equationParent),
                DivisionType.DivRegularSmall => new DivRegularSmall(owner, equationParent),
                DivisionType.DivDoubleBar => new DivDoubleBar(owner, equationParent),
                DivisionType.DivTripleBar => new DivTripleBar(owner, equationParent),
                DivisionType.DivHoriz => new DivHorizontal(owner, equationParent),
                DivisionType.DivHorizSmall => new DivHorizSmall(owner, equationParent),
                DivisionType.DivMath => new DivMath(owner, equationParent),
                DivisionType.DivMathWithTop => new DivMathWithTop(owner, equationParent),
                DivisionType.DivSlanted => new DivSlanted(owner, equationParent),
                DivisionType.DivSlantedSmall => new DivSlantedSmall(owner, equationParent),
                DivisionType.DivMathInverted => new DivMathInverted(owner, equationParent),
                DivisionType.DivInvertedWithBottom => new DivMathWithBottom(owner, equationParent),
                DivisionType.DivTriangleFixed => new DivTriangle(owner, equationParent, true),
                DivisionType.DivTriangleExpanding => new DivTriangle(owner, equationParent, false),
                _ => throw new InvalidOperationException("Unsupported DivisionType in DivisionFactory"),
            };
            return equation;
        }
    }
}
