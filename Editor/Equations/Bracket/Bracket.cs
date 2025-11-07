using System;
using System.Xml.Linq;

namespace Editor
{
    public abstract class Bracket : EquationContainer
    {
        protected RowContainer insideEq;
        protected BracketSign bracketSign = null!;
        protected double ExtraHeight { get; set; }

        public Bracket(IMainWindow owner, EquationContainer parent)
            : base(owner, parent)
        {
            ExtraHeight = FontSize * 0.2;
            ActiveChild = insideEq = new RowContainer(owner, this);
        }

        //public override void DrawEquation(DrawingContext dc)
        //{
        //    base.DrawEquation(dc);
        //    dc.DrawLine(new Pen(Brushes.Red, 1), new Point(Left, insideEq.MidY), new Point(Right, insideEq.MidY));
        //    dc.DrawLine(new Pen(Brushes.Blue, 1), new Point(Left, bracketSign.MidY), new Point(Right, bracketSign.MidY));
        //}

        public override XElement Serialize()
        {
            var thisElement = new XElement(GetType().Name);
            var parameters = new XElement("parameters");
            parameters.Add(new XElement(bracketSign.SignType.GetType().Name, bracketSign.SignType));
            thisElement.Add(parameters);
            thisElement.Add(insideEq.Serialize());
            return thisElement;
        }

        public override void DeSerialize(XElement xElement)
        {
            insideEq.DeSerialize(xElement.Element(insideEq.GetType().Name)!);
            CalculateSize();
        }

        public override double Top
        {
            get => base.Top;
            set
            {
                base.Top = value;
                insideEq.MidY = MidY;
                bracketSign.MidY = MidY;
            }
        }

        public override void CalculateSize()
        {
            CalculateHeight();
            CalculateWidth();
        }

        protected override void CalculateWidth()
        {
            Width = insideEq.Width + bracketSign.Width;
        }

        protected override void CalculateHeight()
        {
            ExtraHeight = FontSize * 0.2;
            var upperMax = insideEq.RefY;
            var lowerMax = insideEq.RefYReverse;
            Height = Math.Max(upperMax, lowerMax) * 2 + ExtraHeight;
            bracketSign.Height = Height;
        }

        //public override double RefY
        //{
        //    get
        //    {
        //        return insideEq.RefY + ExtraHeight / 2;
        //    }
        //}
    }
}
