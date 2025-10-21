using System;

namespace Editor
{
    public static class BigCompositeFactory
    {
        public static EquationBase CreateEquation(EquationContainer equationParent, Position position)
        {
            CompositeBase equation = position switch
            {
                Position.Bottom => new CompositeBottom(equationParent),
                Position.Top => new CompositeTop(equationParent),
                Position.BottomAndTop => new CompositeBottomTop(equationParent),
                Position.Sub => new CompositeSub(equationParent),
                Position.Super => new CompositeSuper(equationParent),
                Position.SubAndSuper => new CompositeSubSuper(equationParent),
                _ => throw new InvalidOperationException("Invalid position for big composite equation."),
            };
            equation.ChangeMainEquationSize(1.3);
            return equation;
        }
    }
}
