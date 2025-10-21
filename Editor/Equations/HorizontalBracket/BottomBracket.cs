namespace Editor
{
    public sealed class BottomBracket : HorizontalBracket
    {
        public BottomBracket(EquationContainer parent, HorizontalBracketSignType signType)
             : base(parent, signType)
        {
            _bottomEquation.FontFactor = SubFontFactor;
            ActiveChild = _topEquation;
        }
    }
}
