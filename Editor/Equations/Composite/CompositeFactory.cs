using System;

namespace Editor
{
    public static class CompositeFactory
    {
        public static EquationBase CreateEquation(IMainWindow owner, EquationContainer equationParent, Position position)
        {
            return position switch
            {
                Position.Bottom => new CompositeBottom(owner, equationParent, false),
                Position.Top => new CompositeTop(owner, equationParent, false),
                Position.BottomAndTop => new CompositeBottomTop(owner, equationParent, false),
                _ => throw new InvalidOperationException("Invalid position for composite equation."),
            };
        }
    }
}
