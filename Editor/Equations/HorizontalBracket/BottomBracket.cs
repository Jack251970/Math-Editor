namespace Editor
{
    public sealed class BottomBracket : HorizontalBracket
    {
        public BottomBracket(IMainWindow owner, EquationContainer parent, HorizontalBracketSignType signType)
             : base(owner, parent, signType)
        {
            _bottomEquation.FontFactor = SubFontFactor;
            ActiveChild = _topEquation;
        }
    }
}
