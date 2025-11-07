using System;

namespace Editor
{
    public static class SignCompositeFactory
    {
        public static EquationBase CreateEquation(IMainWindow owner, EquationContainer equationParent, Position position, SignCompositeSymbol symbol, bool useUpright)
        {
            EquationBase equation = position switch
            {
                Position.None => new SignSimple(owner, equationParent, symbol, useUpright),
                Position.Bottom => new SignBottom(owner, equationParent, symbol, useUpright),
                Position.BottomAndTop => new SignBottomTop(owner, equationParent, symbol, useUpright),
                Position.Sub => new SignSub(owner, equationParent, symbol, useUpright),
                Position.SubAndSuper => new SignSubSuper(owner, equationParent, symbol, useUpright),
                _ => throw new InvalidOperationException($"Invalid position for SignCompositeFactory: {position}"),
            };
            return equation;
        }
    }
}
