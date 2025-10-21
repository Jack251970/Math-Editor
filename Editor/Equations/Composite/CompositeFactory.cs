using System;

namespace Editor
{
    public static class CompositeFactory
    {
        public static EquationBase CreateEquation(EquationContainer equationParent, Position position)
        {
            EquationBase equation = position switch
            {
                Position.Bottom => new CompositeBottom(equationParent),
                Position.Top => new CompositeTop(equationParent),
                Position.BottomAndTop => new CompositeBottomTop(equationParent),
                _ => throw new InvalidOperationException("Invalid position for composite equation."),
            };
            return equation;
        }
    }
}
