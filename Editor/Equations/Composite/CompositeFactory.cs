using System;

namespace Editor
{
    public static class CompositeFactory
    {
        public static EquationBase CreateEquation(EquationContainer equationParent, Position position)
        {
            return position switch
            {
                Position.Bottom => new CompositeBottom(equationParent, false),
                Position.Top => new CompositeTop(equationParent, false),
                Position.BottomAndTop => new CompositeBottomTop(equationParent, false),
                _ => throw new InvalidOperationException("Invalid position for composite equation."),
            };
        }
    }
}
