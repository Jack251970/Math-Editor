using System;

namespace Editor
{
    public static class DivisionFactory
    {
        public static EquationBase CreateEquation(EquationContainer equationParent, DivisionType divType)
        {
            EquationBase equation = divType switch
            {
                DivisionType.DivRegular => new DivRegular(equationParent),
                DivisionType.DivRegularSmall => new DivRegularSmall(equationParent),
                DivisionType.DivDoubleBar => new DivDoubleBar(equationParent),
                DivisionType.DivTripleBar => new DivTripleBar(equationParent),
                DivisionType.DivHoriz => new DivHorizontal(equationParent),
                DivisionType.DivHorizSmall => new DivHorizSmall(equationParent),
                DivisionType.DivMath => new DivMath(equationParent),
                DivisionType.DivMathWithTop => new DivMathWithTop(equationParent),
                DivisionType.DivSlanted => new DivSlanted(equationParent),
                DivisionType.DivSlantedSmall => new DivSlantedSmall(equationParent),
                DivisionType.DivMathInverted => new DivMathInverted(equationParent),
                DivisionType.DivInvertedWithBottom => new DivMathWithBottom(equationParent),
                DivisionType.DivTriangleFixed => new DivTriangle(equationParent, true),
                DivisionType.DivTriangleExpanding => new DivTriangle(equationParent, false),
                _ => throw new InvalidOperationException("Unsupported DivisionType in DivisionFactory"),
            };
            return equation;
        }
    }
}
