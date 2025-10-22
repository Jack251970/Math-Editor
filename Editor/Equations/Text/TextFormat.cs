using System;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using iNKORE.UI.WPF.Modern;

namespace Editor
{
    public sealed class TextFormat
    {
        private static readonly string ThemeAwareTextBrushString = new BrushConverter().ConvertToString(Brushes.Transparent);

        public double FontSize { get; private set; }
        public FontType FontType { get; private set; }
        public FontFamily FontFamily { get; private set; }
        public FontStyle FontStyle { get; private set; }
        public FontWeight FontWeight { get; private set; }
        public SolidColorBrush TextBrush { get; private set; }
        public Typeface TypeFace { get; private set; }
        public string TextBrushString { get; private set; }
        public bool UseUnderline { get; set; }
        public int Index { get; set; }

        public TextFormat(double size, FontType ft, FontStyle fs, FontWeight fw, SolidColorBrush brush, bool useUnderline)
        {
            this.FontSize = Math.Round(size, 1);
            this.FontType = ft;
            this.FontFamily = FontFactory.GetFontFamily(ft);
            this.FontStyle = fs;
            this.UseUnderline = useUnderline;
            this.FontWeight = fw;
            this.TextBrush = brush;
            this.TypeFace = new Typeface(FontFamily, fs, fw, FontStretches.Normal, FontFactory.GetFontFamily(FontType.STIXGeneral));
            var bc = new BrushConverter();
            TextBrushString = bc.ConvertToString(brush);
        }

        public XElement Serialize(bool themeAwareBrush = false)
        {
            var thisElement = new XElement(GetType().Name);
            thisElement.Add(new XElement("FontSize", FontSize),
                             new XElement("FontType", FontType),
                             new XElement("FontStyle", FontStyle),
                             new XElement("Underline", UseUnderline),
                             new XElement("FontWeight", FontWeight),
                             new XElement("Brush", themeAwareBrush ?
                                 ThemeAwareTextBrushString : TextBrushString));
            return thisElement;
        }

        public static TextFormat DeSerialize(XElement xe)
        {
            var fontSize = double.Parse(xe.Element("FontSize")!.Value);
            var fontType = Enum.Parse<FontType>(xe.Element("FontType")!.Value);
            var fontStyle = xe.Element("FontStyle")!.Value == "Italic" ? FontStyles.Italic : FontStyles.Normal;
            var fontWeight = xe.Element("FontWeight")!.Value == "Bold" ? FontWeights.Bold : FontWeights.Normal;
            var bc = new BrushConverter();
            var brushString = xe.Element("Brush")!.Value;
            SolidColorBrush brush;
            if (string.Equals(brushString, ThemeAwareTextBrushString, StringComparison.OrdinalIgnoreCase))
            {
                brush = ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Light ?
                    Brushes.Black : Brushes.White;
            }
            else
            {
                brush = (SolidColorBrush)bc.ConvertFrom(brushString)!;
            }
            var useUnderline = Convert.ToBoolean(xe.Element("Underline")!.Value);
            return new TextFormat(fontSize, fontType, fontStyle, fontWeight, brush, useUnderline);
        }
    }
}
