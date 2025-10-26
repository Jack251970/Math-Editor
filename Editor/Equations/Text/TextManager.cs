using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace Editor
{
    //Only those EquationBase classes should use it which are able to remeber the formats (as of May 15, 2013 only TextEquation)!!
    public sealed class TextManager
    {
        private List<TextFormat> formattingList = [];
        private readonly List<TextDecorationCollection> decorations = [];
        private readonly Dictionary<int, int> mapping = [];
        private List<TextFormat>? formattingListBeforeSave = null;

        public TextManager()
        {
            var tdc = new TextDecorationCollection
            {
                TextDecorations.Underline
            };
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
        }

        public void DeSerialize(XElement xElement)
        {
            formattingList.Clear();
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
                var match = formattingList.Where(x =>
                    {
                        return x.FontSize == Math.Round(tf.FontSize, 1) &&
                               x.FontType == tf.FontType &&
                               x.FontStyle == tf.FontStyle &&
                               x.UseUnderline == tf.UseUnderline &&
                               Color.AreClose(x.TextBrush.Color, tf.TextBrush.Color) &&
                               x.FontWeight == tf.FontWeight;

                    }).FirstOrDefault();

                var newValue = 0;
                if (match == null)
                {
                    AddToList(tf);
                    newValue = tf.Index;
                }
                else
                {
                    newValue = match.Index;
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
            var num = Math.Round(fontSize, 1);
            var tf = formattingList.Where(x =>
            {
                return x.FontSize == Math.Round(fontSize, 1) &&
                       x.FontType == fontType &&
                       x.FontStyle == fontStyle &&
                       x.UseUnderline == useUnderline &&
                       Color.AreClose(x.TextBrush.Color, textBrush.Color) &&
                       x.FontWeight == fontWeight;

            }).FirstOrDefault();
            if (tf == null)
            {
                tf = new TextFormat(fontSize, fontType, fontStyle, fontWeight, textBrush, useUnderline);
                AddToList(tf);
            }
            return tf.Index;
        }

        public int GetFormatIdForNewFont(int oldId, FontType fontType)
        {
            var oldFormat = formattingList[oldId];

            var tf = formattingList.Where(x =>
            {
                return x.FontSize == oldFormat.FontSize &&
                       x.FontType == fontType &&
                       x.FontStyle == oldFormat.FontStyle &&
                       x.UseUnderline == oldFormat.UseUnderline &&
                       Color.AreClose(x.TextBrush.Color, oldFormat.TextBrush.Color) &&
                       x.FontWeight == oldFormat.FontWeight;

            }).FirstOrDefault();
            if (tf == null)
            {
                tf = new TextFormat(oldFormat.FontSize, fontType, oldFormat.FontStyle, oldFormat.FontWeight, oldFormat.TextBrush, oldFormat.UseUnderline);
                AddToList(tf);
            }
            return tf.Index;
        }

        public int GetFormatIdForNewSolidBrush(int oldId, SolidColorBrush brush)
        {
            var oldFormat = formattingList[oldId];

            var tf = formattingList.Where(x =>
            {
                return x.FontSize == oldFormat.FontSize &&
                       x.FontType == oldFormat.FontType &&
                       x.FontStyle == oldFormat.FontStyle &&
                       x.UseUnderline == oldFormat.UseUnderline &&
                       Color.AreClose(x.TextBrush.Color, brush.Color) &&
                       x.FontWeight == oldFormat.FontWeight;

            }).FirstOrDefault();
            if (tf == null)
            {
                tf = new TextFormat(oldFormat.FontSize, oldFormat.FontType, oldFormat.FontStyle, oldFormat.FontWeight, brush, oldFormat.UseUnderline);
                AddToList(tf);
            }
            return tf.Index;
        }

        public int GetFormatIdForNewSize(int oldId, double newSize)
        {
            var oldFormat = formattingList[oldId];

            var tf = formattingList.Where(x =>
            {
                return x.FontSize == Math.Round(newSize, 1) &&
                       x.FontType == oldFormat.FontType &&
                       x.FontStyle == oldFormat.FontStyle &&
                       x.UseUnderline == oldFormat.UseUnderline &&
                       Color.AreClose(x.TextBrush.Color, oldFormat.TextBrush.Color) &&
                       x.FontWeight == oldFormat.FontWeight;

            }).FirstOrDefault();
            if (tf == null)
            {
                tf = new TextFormat(newSize, oldFormat.FontType, oldFormat.FontStyle, oldFormat.FontWeight, oldFormat.TextBrush, oldFormat.UseUnderline);
                AddToList(tf);
            }
            return tf.Index;
        }

        public int GetFormatIdForNewStyle(int oldId, FontStyle newStyle)
        {
            var oldFormat = formattingList[oldId];

            var tf = formattingList.Where(x =>
            {
                return x.FontSize == oldFormat.FontSize &&
                       x.FontType == oldFormat.FontType &&
                       x.FontStyle == newStyle &&
                       x.UseUnderline == oldFormat.UseUnderline &&
                       Color.AreClose(x.TextBrush.Color, oldFormat.TextBrush.Color) &&
                       x.FontWeight == oldFormat.FontWeight;

            }).FirstOrDefault();
            if (tf == null)
            {
                tf = new TextFormat(oldFormat.FontSize, oldFormat.FontType, newStyle, oldFormat.FontWeight, oldFormat.TextBrush, oldFormat.UseUnderline);
                AddToList(tf);
            }
            return tf.Index;
        }

        public int GetFormatIdForNewWeight(int oldId, FontWeight newWeight)
        {
            var oldFormat = formattingList[oldId];

            var tf = formattingList.Where(x =>
            {
                return x.FontSize == oldFormat.FontSize &&
                       x.FontType == oldFormat.FontType &&
                       x.FontStyle == oldFormat.FontStyle &&
                       x.UseUnderline == oldFormat.UseUnderline &&
                       Color.AreClose(x.TextBrush.Color, oldFormat.TextBrush.Color) &&
                       x.FontWeight == newWeight;

            }).FirstOrDefault();
            if (tf == null)
            {
                tf = new TextFormat(oldFormat.FontSize, oldFormat.FontType, oldFormat.FontStyle, newWeight, oldFormat.TextBrush, oldFormat.UseUnderline);
                AddToList(tf);
            }
            return tf.Index;
        }

        public int GetFormatIdForNewUnderline(int oldId, bool newUnderline)
        {
            var oldFormat = formattingList[oldId];

            var tf = formattingList.Where(x =>
            {
                return x.FontSize == oldFormat.FontSize &&
                       x.FontType == oldFormat.FontType &&
                       x.FontStyle == oldFormat.FontStyle &&
                       x.UseUnderline == newUnderline &&
                       Color.AreClose(x.TextBrush.Color, oldFormat.TextBrush.Color) &&
                       x.FontWeight == oldFormat.FontWeight;

            }).FirstOrDefault();
            if (tf == null)
            {
                tf = new TextFormat(oldFormat.FontSize, oldFormat.FontType, oldFormat.FontStyle, oldFormat.FontWeight, oldFormat.TextBrush, newUnderline);
                AddToList(tf);
            }
            return tf.Index;
        }

        public FormattedText GetFormattedText(string text, List<int> formats, bool forceBlackBrush = false)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var formattedText = new FormattedText(text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                formattingList[formats[0]].TypeFace,
                formattingList[formats[0]].FontSize,
                forceBlackBrush ? Brushes.Black : formattingList[formats[0]].TextBrush);
#pragma warning restore CS0618 // Type or member is obsolete
            for (var i = 0; i < formats.Count; i++)
            {
                FormatText(formats, formattedText, i, forceBlackBrush);
            }
            return formattedText;
        }

        private void FormatText(List<int> formats, FormattedText formattedText, int i, bool forceBlackBrush = false)
        {
            formattedText.SetFontFamily(formattingList[formats[i]].FontFamily, i, 1);
            formattedText.SetFontSize(formattingList[formats[i]].FontSize, i, 1);
            formattedText.SetFontStyle(formattingList[formats[i]].FontStyle, i, 1);
            formattedText.SetFontWeight(formattingList[formats[i]].FontWeight, i, 1);
            formattedText.SetForegroundBrush(forceBlackBrush ? Brushes.Black : formattingList[formats[i]].TextBrush, i, 1);
            if (formattingList[formats[i]].UseUnderline)
            {
                formattedText.SetTextDecorations(decorations[0], i, 1);
            }
        }

        public bool IsBold(int formatId)
        {
            return formattingList[formatId].FontWeight == FontWeights.Bold;
        }

        public bool IsItalic(int formatId)
        {
            return formattingList[formatId].FontStyle == FontStyles.Italic;
        }

        public bool IsUnderline(int formatId)
        {
            return formattingList[formatId].UseUnderline;
        }

        public FontType GetFontType(int formatId)
        {
            return formattingList[formatId].FontType;
        }

        public FormattedText GetFormattedText(string text, int format)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var formattedText = new FormattedText(text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                formattingList[format].TypeFace,
                formattingList[format].FontSize,
                formattingList[format].TextBrush);
#pragma warning restore CS0618 // Type or member is obsolete
            formattedText.SetFontStyle(formattingList[format].FontStyle);
            formattedText.SetFontWeight(formattingList[format].FontWeight);
            return formattedText;
        }

        public double GetBaseline(int formatId)
        {
            return formattingList[formatId].FontFamily.Baseline;
        }

        public double GetLineSpacing(int formatId)
        {
            return formattingList[formatId].FontFamily.LineSpacing;
        }

        public double GetFontHeight(int formatId)
        {
            double fontDpiSize = 16;
            return fontDpiSize * formattingList[formatId].FontFamily.LineSpacing;
        }
    }
}
