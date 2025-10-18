using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using iNKORE.UI.WPF.Modern;

namespace Editor;

public sealed class FontFactory
{
    private FontFactory() { }
    private static readonly Dictionary<FontType, FontFamily> fontFamilies = [];

    static FontFactory()
    {
        foreach (FontType ft in Enum.GetValues<FontType>())
        {
            fontFamilies.Add(ft, CreateFontFamily(ft));
        }
    }

    public static FormattedText GetFormattedText(string textToFormat, FontType fontType, double fontSize)
    {
        return GetFormattedText(textToFormat, fontType, fontSize, FontStyles.Normal, FontWeights.Normal);
    }

    public static FormattedText GetFormattedText(string textToFormat, FontType fontType, double fontSize, FontWeight fontWeight)
    {
        return GetFormattedText(textToFormat, fontType, fontSize, FontStyles.Normal, fontWeight);
    }

    public static FormattedText GetFormattedText(string textToFormat, FontType fontType, double fontSize, FontStyle fontStyle, FontWeight fontWeight)
    {
        return GetFormattedText(textToFormat, fontType, fontSize, fontStyle, fontWeight, (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Light ?
            Brushes.Black : Brushes.White));
    }

    public static FormattedText GetFormattedText(string textToFormat, FontType fontType, double fontSize, Brush brush)
    {
        return GetFormattedText(textToFormat, fontType, fontSize, FontStyles.Normal, FontWeights.Normal, brush);
    }

    public static FormattedText GetFormattedText(string textToFormat, FontType fontType, double fontSize, FontStyle fontStyle, FontWeight fontWeight, Brush brush)
    {
        var typeface = GetTypeface(fontType, fontStyle, fontWeight);
#pragma warning disable CS0618 // Type or member is obsolete
        return new FormattedText(textToFormat, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, typeface, fontSize, brush);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public static FontFamily GetFontFamily(FontType fontType)
    {
        if (fontFamilies.TryGetValue(fontType, out var value))
        {
            return value;
        }
        else
        {
            return new FontFamily("Segoe UI");
        }
    }

    private static FontFamily CreateFontFamily(FontType ft)
    {
        return ft switch
        {
            FontType.STIXGeneral => new FontFamily(new Uri("pack://application:,,,/Fonts/STIX/"), "./#STIXGeneral"),
            FontType.STIXIntegralsD => new FontFamily(new Uri("pack://application:,,,/Fonts/STIX/"), "./#STIXIntegralsD"),
            FontType.STIXIntegralsSm => new FontFamily(new Uri("pack://application:,,,/Fonts/STIX/"), "./#STIXIntegralsSm"),
            FontType.STIXIntegralsUp => new FontFamily(new Uri("pack://application:,,,/Fonts/STIX/"), "./#STIXIntegralsUp"),
            FontType.STIXIntegralsUpD => new FontFamily(new Uri("pack://application:,,,/Fonts/STIX/"), "./#STIXIntegralsUpD"),
            FontType.STIXIntegralsUpSm => new FontFamily(new Uri("pack://application:,,,/Fonts/STIX/"), "./#STIXIntegralsUpSm"),
            FontType.STIXNonUnicode => new FontFamily(new Uri("pack://application:,,,/Fonts/STIX/"), "./#STIXNonUnicode"),
            FontType.STIXSizeFiveSym => new FontFamily(new Uri("pack://application:,,,/Fonts/STIX/"), "./#STIXSizeFiveSym"),
            FontType.STIXSizeFourSym => new FontFamily(new Uri("pack://application:,,,/Fonts/STIX/"), "./#STIXSizeFourSym"),
            FontType.STIXSizeOneSym => new FontFamily(new Uri("pack://application:,,,/Fonts/STIX/"), "./#STIXSizeOneSym"),
            FontType.STIXSizeThreeSym => new FontFamily(new Uri("pack://application:,,,/Fonts/STIX/"), "./#STIXSizeThreeSym"),
            FontType.STIXSizeTwoSym => new FontFamily(new Uri("pack://application:,,,/Fonts/STIX/"), "./#STIXSizeTwoSym"),
            FontType.STIXVariants => new FontFamily(new Uri("pack://application:,,,/Fonts/STIX/"), "./#STIXVariants"),
            FontType.Arial => new FontFamily("Arial"),
            FontType.ArialBlack => new FontFamily("Arial Black"),
            FontType.ComicSansMS => new FontFamily("Comic Sans MS"),
            FontType.Courier => new FontFamily("Courier"),
            FontType.CourierNew => new FontFamily("Courier New"),
            FontType.Georgia => new FontFamily("Georgia"),
            FontType.Impact => new FontFamily("Impact"),
            FontType.LucidaConsole => new FontFamily("Lucida Console"),
            FontType.LucidaSansUnicode => new FontFamily("Lucida Sans Unicode"),
            FontType.MSSerif => new FontFamily("MS Serif"),
            FontType.MSSansSerif => new FontFamily("MS Sans Serif"),
            FontType.PalatinoLinotype => new FontFamily("Palatino Linotype"),
            FontType.Segoe => new FontFamily("Segoe UI"),
            FontType.Symbol => new FontFamily("Symbol"),
            FontType.Tahoma => new FontFamily("Tahoma"),
            FontType.TimesNewRoman => new FontFamily("Times New Roman"),
            FontType.TrebuchetMS => new FontFamily("Trebuchet MS"),
            FontType.Verdana => new FontFamily("Verdana"),
            FontType.Webdings => new FontFamily("Webdings"),
            FontType.Wingdings => new FontFamily("Wingdings"),
            _ => new FontFamily("Segoe UI"),
        };
    }

    public static Typeface GetTypeface(FontType fontType, FontStyle fontStyle, FontWeight fontWeight)
    {
        return new Typeface(GetFontFamily(fontType), fontStyle, fontWeight, FontStretches.Normal, GetFontFamily(FontType.STIXGeneral));
    }
}
