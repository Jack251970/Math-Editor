using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Avalonia.Media;

namespace Editor
{
    //Only those EquationBase classes should use it which are able to remeber the formats (as of May 15, 2013 only TextEquation)!!
    public sealed class TextManager
    {
        private List<TextFormat> formattingList = [];
        private readonly List<TextDecorationCollection> decorations = [];
        private readonly Dictionary<int, int> mapping = [];
        private List<TextFormat>? formattingListBeforeSave = null;

        // Cache for fast format lookup - uses a composite key to avoid O(n) searches
        private Dictionary<TextFormatKey, int> formatCache = [];

        /// <summary>
        /// Composite key for efficient format lookup
        /// </summary>
        private readonly record struct TextFormatKey(
            double FontSize,
            FontType FontType,
            FontStyle FontStyle,
            FontWeight FontWeight,
            uint BrushColorArgb,
            bool UseUnderline
        )
        {
            public static TextFormatKey FromTextFormat(TextFormat tf) =>
                new(tf.FontSize, tf.FontType, tf.FontStyle, tf.FontWeight,
                    ColorToArgb(tf.TextBrush.Color), tf.UseUnderline);

            public static TextFormatKey Create(double fontSize, FontType fontType, FontStyle fontStyle,
                FontWeight fontWeight, Color brushColor, bool useUnderline) =>
                new(Math.Round(fontSize, 1), fontType, fontStyle, fontWeight,
                    ColorToArgb(brushColor), useUnderline);

            private static uint ColorToArgb(Color c) =>
                ((uint)c.A << 24) | ((uint)c.R << 16) | ((uint)c.G << 8) | c.B;
        }

        public TextManager()
        {
            var tdc = TextDecorations.Underline;
            decorations.Add(tdc);
        }

        public void OptimizeForSave(EquationRoot root)
        {
            mapping.Clear();
            List<TextFormat> newList = [];
            var usedOnes = root.GetUsedTextFormats();
            foreach (var i in usedOnes)
            {
                var tf = formattingList[i];
                tf.Index = newList.Count;
                newList.Add(tf);
                mapping.Add(i, tf.Index);
            }
            root.ResetTextFormats(mapping);
            formattingListBeforeSave = formattingList;
            formattingList = newList;
        }

        public void RestoreAfterSave(EquationRoot root)
        {
            Dictionary<int, int> oldMapping = [];
            foreach (var i in mapping.Keys)
            {
                oldMapping.Add(mapping[i], i);
                var tf = formattingListBeforeSave![i];
                tf.Index = i;
            }
            root.ResetTextFormats(oldMapping);
            formattingList = formattingListBeforeSave!;
        }

        public XElement Serialize(bool themeAwareBrush = false)
        {
            var thisElement = new XElement(GetType().Name);
            var children = new XElement("Formats");
            foreach (var tf in formattingList)
            {
                children.Add(tf.Serialize(themeAwareBrush));
            }
            thisElement.Add(children);
            return thisElement;
        }

        private void AddToList(TextFormat tf)
        {
            tf.Index = formattingList.Count;
            formattingList.Add(tf);
            // Add to cache for O(1) lookups
            formatCache[TextFormatKey.FromTextFormat(tf)] = tf.Index;
        }

        public void DeSerialize(XElement xElement)
        {
            formattingList.Clear();
            formatCache.Clear(); // Clear cache when deserializing
            var children = xElement.Element("Formats");
            foreach (var xe in children!.Elements())
            {
                AddToList(TextFormat.DeSerialize(xe));
            }
        }

        public void ProcessPastedXML(XElement rootXE)
        {
            //XElement thisElement = rootXE.Element(GetType().Name);
            XElement[] formatElements = [.. rootXE.Element(GetType().Name)!.Elements("Formats").Elements()];
            var formats = rootXE.Descendants(typeof(TextEquation).Name).Descendants("Formats");
            Dictionary<int, int> allFormatIds = [];
            foreach (var xe in formats)
            {
                if (xe.Value.Length > 0)
                {
                    var formatStrings = xe.Value.Split(',');
                    foreach (var s in formatStrings)
                    {
                        var id = int.Parse(s);
                        allFormatIds.TryAdd(id, id);
                    }
                }
            }
            for (var i = 0; i < allFormatIds.Count; i++)
            {
                var key = allFormatIds.ElementAt(i).Key;
                var tf = TextFormat.DeSerialize(formatElements[key]);
                var cacheKey = TextFormatKey.FromTextFormat(tf);

                int newValue;
                if (formatCache.TryGetValue(cacheKey, out var cachedIndex))
                {
                    newValue = cachedIndex;
                }
                else
                {
                    AddToList(tf);
                    newValue = tf.Index;
                }
                allFormatIds[key] = newValue;
            }
            var textElements = rootXE.Descendants(typeof(TextEquation).Name);
            foreach (var xe in textElements)
            {
                var formatsElement = xe.Elements("Formats").FirstOrDefault();
                if (formatsElement != null)
                {
                    var strBuilder = new StringBuilder();
                    var formatStrings = formatsElement.Value.Split(',');
                    foreach (var s in formatStrings)
                    {
                        if (s.Length > 0)
                        {
                            var id = int.Parse(s);
                            strBuilder.Append(allFormatIds[id] + ",");
                        }
                    }
                    if (strBuilder.Length > 0)
                    {
                        strBuilder.Remove(strBuilder.Length - 1, 1);
                    }
                    formatsElement.Value = strBuilder.ToString();
                }
            }
        }

        public int GetFormatId(double fontSize, FontType fontType, FontStyle fontStyle, FontWeight fontWeight, SolidColorBrush textBrush, bool useUnderline)
        {
            var key = TextFormatKey.Create(fontSize, fontType, fontStyle, fontWeight, textBrush.Color, useUnderline);
            if (formatCache.TryGetValue(key, out var cachedIndex))
            {
                return cachedIndex;
            }

            var tf = new TextFormat(fontSize, fontType, fontStyle, fontWeight, textBrush, useUnderline);
            AddToList(tf);
            return tf.Index;
        }

        public int GetFormatIdForNewFont(int oldId, FontType fontType)
        {
            var oldFormat = formattingList[oldId];
            var key = TextFormatKey.Create(oldFormat.FontSize, fontType, oldFormat.FontStyle,
                oldFormat.FontWeight, oldFormat.TextBrush.Color, oldFormat.UseUnderline);

            if (formatCache.TryGetValue(key, out var cachedIndex))
            {
                return cachedIndex;
            }

            var tf = new TextFormat(oldFormat.FontSize, fontType, oldFormat.FontStyle, oldFormat.FontWeight, oldFormat.TextBrush, oldFormat.UseUnderline);
            AddToList(tf);
            return tf.Index;
        }

        public int GetFormatIdForNewSolidBrush(int oldId, SolidColorBrush brush)
        {
            var oldFormat = formattingList[oldId];
            var key = TextFormatKey.Create(oldFormat.FontSize, oldFormat.FontType, oldFormat.FontStyle,
                oldFormat.FontWeight, brush.Color, oldFormat.UseUnderline);

            if (formatCache.TryGetValue(key, out var cachedIndex))
            {
                return cachedIndex;
            }

            var tf = new TextFormat(oldFormat.FontSize, oldFormat.FontType, oldFormat.FontStyle, oldFormat.FontWeight, brush, oldFormat.UseUnderline);
            AddToList(tf);
            return tf.Index;
        }

        public int GetFormatIdForNewSize(int oldId, double newSize)
        {
            var oldFormat = formattingList[oldId];
            var key = TextFormatKey.Create(newSize, oldFormat.FontType, oldFormat.FontStyle,
                oldFormat.FontWeight, oldFormat.TextBrush.Color, oldFormat.UseUnderline);

            if (formatCache.TryGetValue(key, out var cachedIndex))
            {
                return cachedIndex;
            }

            var tf = new TextFormat(newSize, oldFormat.FontType, oldFormat.FontStyle, oldFormat.FontWeight, oldFormat.TextBrush, oldFormat.UseUnderline);
            AddToList(tf);
            return tf.Index;
        }

        public int GetFormatIdForNewStyle(int oldId, FontStyle newStyle)
        {
            var oldFormat = formattingList[oldId];
            var key = TextFormatKey.Create(oldFormat.FontSize, oldFormat.FontType, newStyle,
                oldFormat.FontWeight, oldFormat.TextBrush.Color, oldFormat.UseUnderline);

            if (formatCache.TryGetValue(key, out var cachedIndex))
            {
                return cachedIndex;
            }

            var tf = new TextFormat(oldFormat.FontSize, oldFormat.FontType, newStyle, oldFormat.FontWeight, oldFormat.TextBrush, oldFormat.UseUnderline);
            AddToList(tf);
            return tf.Index;
        }

        public int GetFormatIdForNewWeight(int oldId, FontWeight newWeight)
        {
            var oldFormat = formattingList[oldId];
            var key = TextFormatKey.Create(oldFormat.FontSize, oldFormat.FontType, oldFormat.FontStyle,
                newWeight, oldFormat.TextBrush.Color, oldFormat.UseUnderline);

            if (formatCache.TryGetValue(key, out var cachedIndex))
            {
                return cachedIndex;
            }

            var tf = new TextFormat(oldFormat.FontSize, oldFormat.FontType, oldFormat.FontStyle, newWeight, oldFormat.TextBrush, oldFormat.UseUnderline);
            AddToList(tf);
            return tf.Index;
        }

        public int GetFormatIdForNewUnderline(int oldId, bool newUnderline)
        {
            var oldFormat = formattingList[oldId];
            var key = TextFormatKey.Create(oldFormat.FontSize, oldFormat.FontType, oldFormat.FontStyle,
                oldFormat.FontWeight, oldFormat.TextBrush.Color, newUnderline);

            if (formatCache.TryGetValue(key, out var cachedIndex))
            {
                return cachedIndex;
            }

            var tf = new TextFormat(oldFormat.FontSize, oldFormat.FontType, oldFormat.FontStyle, oldFormat.FontWeight, oldFormat.TextBrush, newUnderline);
            AddToList(tf);
            return tf.Index;
        }

        public FormattedTextExtended GetFormattedTextExtended(string text, List<int> formats, bool forceBlackBrush = false)
        {
            var formattedTextExtended = new FormattedTextExtended(text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                formattingList[formats[0]].TypeFace,
                formattingList[formats[0]].FontSize,
                forceBlackBrush ? Brushes.Black : formattingList[formats[0]].TextBrush);
            for (var i = 0; i < formats.Count; i++)
            {
                FormatText(formats, formattedTextExtended, i, forceBlackBrush);
            }
            return formattedTextExtended;
        }

        private void FormatText(List<int> formats, FormattedTextExtended formattedTextExtended, int i, bool forceBlackBrush = false)
        {
            formattedTextExtended.SetFontFamily(formattingList[formats[i]].FontFamily, i, 1);
            formattedTextExtended.SetFontSize(formattingList[formats[i]].FontSize, i, 1);
            formattedTextExtended.SetFontStyle(formattingList[formats[i]].FontStyle, i, 1);
            formattedTextExtended.SetFontWeight(formattingList[formats[i]].FontWeight, i, 1);
            formattedTextExtended.SetForegroundBrush(forceBlackBrush ? Brushes.Black : formattingList[formats[i]].TextBrush, i, 1);
            if (formattingList[formats[i]].UseUnderline)
            {
                formattedTextExtended.SetTextDecorations(decorations[0], i, 1);
            }
        }

        public bool IsBold(int formatId)
        {
            return formattingList[formatId].FontWeight == FontWeight.Bold;
        }

        public bool IsItalic(int formatId)
        {
            return formattingList[formatId].FontStyle == FontStyle.Italic;
        }

        public bool IsUnderline(int formatId)
        {
            return formattingList[formatId].UseUnderline;
        }

        public FontType GetFontType(int formatId)
        {
            return formattingList[formatId].FontType;
        }

        public FormattedTextExtended GetFormattedTextExtended(string text, int format)
        {
            var formattedTextExtended = new FormattedTextExtended(text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                formattingList[format].TypeFace,
                formattingList[format].FontSize,
                formattingList[format].TextBrush);
            formattedTextExtended.SetFontStyle(formattingList[format].FontStyle);
            formattedTextExtended.SetFontWeight(formattingList[format].FontWeight);
            return formattedTextExtended;
        }

        public double GetBaseline(int formatId)
        {
            var tf = formattingList[formatId];
            var ft = new FormattedTextExtended("d", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, tf.TypeFace, tf.FontSize, tf.TextBrush);
            return ft.Baseline;
        }

        public double GetLineSpacing(int formatId)
        {
            var tf = formattingList[formatId];
            var ft = new FormattedTextExtended("d", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, tf.TypeFace, tf.FontSize, tf.TextBrush);
            // Return line spacing as ratio (height / font size) which approximates LineSpacing from WPF
            return ft.Height / tf.FontSize;
        }

        public double GetFontHeight(int formatId)
        {
            var tf = formattingList[formatId];
            var ft = new FormattedTextExtended("d", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, tf.TypeFace, tf.FontSize, tf.TextBrush);
            return ft.Height;
        }
    }
}
