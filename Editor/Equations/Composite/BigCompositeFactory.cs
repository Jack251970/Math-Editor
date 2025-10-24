using System;

namespace Editor
{
    public static class BigCompositeFactory
    {
        public static EquationBase CreateEquation(MainWindow owner, EquationContainer equationParent, Position position)
        {
            return position switch
            {
                Position.Bottom => new CompositeBottom(owner, equationParent, true),
                Position.Top => new CompositeTop(owner, equationParent, true),
                Position.BottomAndTop => new CompositeBottomTop(owner, equationParent, true),
                Position.Sub => new CompositeSub(owner, equationParent, true),
                Position.Super => new CompositeSuper(owner, equationParent, true),
                Position.SubAndSuper => new CompositeSubSuper(owner, equationParent, true),
                _ => throw new InvalidOperationException("Invalid position for big composite equation."),
            };
        }
    }
}
