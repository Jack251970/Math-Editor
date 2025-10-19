using System;

namespace Editor
{
    public static class SignCompositeFactory
    {
        public static EquationBase CreateEquation(EquationContainer equationParent, Position position, SignCompositeSymbol symbol, bool useUpright)
        {
            EquationBase equation = position switch
            {
                Position.None => new SignSimple(equationParent, symbol, useUpright),
                Position.Bottom => new SignBottom(equationParent, symbol, useUpright),
                Position.BottomAndTop => new SignBottomTop(equationParent, symbol, useUpright),
                Position.Sub => new SignSub(equationParent, symbol, useUpright),
                Position.SubAndSuper => new SignSubSuper(equationParent, symbol, useUpright),
                _ => throw new InvalidOperationException($"Invalid position for SignCompositeFactory: {position}"),
            };
            return equation;
        }
    }
}
