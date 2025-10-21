namespace Editor
{
    public sealed class TopBracket : HorizontalBracket
    {
        public TopBracket(EquationContainer parent, HorizontalBracketSignType signType)
             : base(parent, signType)
        {
            _topEquation.FontFactor = SubFontFactor;
            ActiveChild = _bottomEquation;
        }
    }
}
