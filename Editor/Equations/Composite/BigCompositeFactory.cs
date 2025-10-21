using System;

namespace Editor
{
    public static class BigCompositeFactory
    {
        public static EquationBase CreateEquation(EquationContainer equationParent, Position position)
        {
            return position switch
            {
                Position.Bottom => new CompositeBottom(equationParent, true),
                Position.Top => new CompositeTop(equationParent, true),
                Position.BottomAndTop => new CompositeBottomTop(equationParent, true),
                Position.Sub => new CompositeSub(equationParent, true),
                Position.Super => new CompositeSuper(equationParent, true),
                Position.SubAndSuper => new CompositeSubSuper(equationParent, true),
                _ => throw new InvalidOperationException("Invalid position for big composite equation."),
            };
        }
    }
}
