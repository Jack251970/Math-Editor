namespace Editor
{
    public abstract class CompositeBase : EquationContainer
    {
        protected bool IsCompositeBig;
        protected RowContainer mainRowContainer;
        protected double bottomGap;
        protected double SubOverlap => FontSize * .4;
        protected double SuperOverlap => FontSize * 0.32;

        public CompositeBase(IMainWindow owner, EquationContainer parent, bool isCompositeBig)
            : base(owner, parent)
        {
            ActiveChild = mainRowContainer = new RowContainer(owner, this);
            DetermineBottomGap();
            IsCompositeBig = isCompositeBig;
            if (isCompositeBig)
            {
                mainRowContainer.FontFactor = 1.3;
            }
        }

        private void DetermineBottomGap()
        {
            bottomGap = FontSize / 20;
        }

        public override double FontSize
        {
            get => base.FontSize;
            set
            {
                base.FontSize = value;
                DetermineBottomGap();
            }
        }
    }
}
