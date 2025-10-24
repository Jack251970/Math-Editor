namespace Editor
{
    public sealed class TopBracket : HorizontalBracket
    {
        public TopBracket(MainWindow owner, EquationContainer parent, HorizontalBracketSignType signType)
             : base(owner, parent, signType)
        {
            _topEquation.FontFactor = SubFontFactor;
            ActiveChild = _bottomEquation;
        }
    }
}
