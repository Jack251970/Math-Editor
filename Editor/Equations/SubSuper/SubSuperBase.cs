namespace Editor
{
    public abstract class SubSuperBase : EquationContainer
    {
        public Position Position { get; set; }
        protected double Padding
        {
            get
            {
                if (buddy != null && buddy.GetType() == typeof(TextEquation))
                {
                    return FontSize * .01;
                }
                else
                {
                    return FontSize * .05;
                }
            }
        }
        protected double SuperOverlap => FontSize * 0.35;
        protected double SubOverlap
        {
            get
            {
                double oha = 0;
                if (buddy is TextEquation te)
                {
                    oha = te.GetCornerDescent(Position);
                }
                return FontSize * .1 - oha;
            }
        }

        private EquationBase? buddy = null;
        protected EquationBase Buddy
        {
            get => buddy ?? ParentEquation.ActiveChild; set => buddy = value;
        }

        public SubSuperBase(IMainWindow owner, EquationRow parent, Position position)
            : base(owner, parent)
        {
            ApplySymbolGap = false;
            SubLevel++;
            Position = position;
        }

        public void SetBuddy(EquationBase buddy)
        {
            Buddy = buddy;
            CalculateHeight();
        }
    }
}
