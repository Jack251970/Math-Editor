using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Media;

namespace Editor;

public sealed class FontFactory
{
    private FontFactory() { }
    private static readonly Dictionary<FontType, FontFamily> fontFamilies = [];

    // Resolve assembly name once to build avares:// URIs dynamically
    private static readonly string assemblyName = typeof(FontFactory).Assembly.GetName().Name!;

    static FontFactory()
    {
        foreach (var ft in Enum.GetValues<FontType>())
        {
            fontFamilies.Add(ft, CreateFontFamily(ft));
        }
    }

    public static FormattedText GetFormattedText(string textToFormat, FontType fontType, double fontSize, bool forceBlackBrush)
    {
        return GetFormattedText(textToFormat, fontType, fontSize, FontStyle.Normal, FontWeight.Normal, forceBlackBrush);
    }

    public static FormattedText GetFormattedText(string textToFormat, FontType fontType, double fontSize, FontWeight fontWeight, bool forceBlackBrush)
    {
        return GetFormattedText(textToFormat, fontType, fontSize, FontStyle.Normal, fontWeight, forceBlackBrush);
    }

    public static FormattedText GetFormattedText(string textToFormat, FontType fontType, double fontSize, FontStyle fontStyle, FontWeight fontWeight, bool forceBlackBrush)
    {
        return GetFormattedText(textToFormat, fontType, fontSize, fontStyle, fontWeight, forceBlackBrush ? PenManager.Black : PenManager.TextFillColorPrimaryBrush);
    }

    public static FormattedText GetFormattedText(string textToFormat, FontType fontType, double fontSize, SolidColorBrush brush)
    {
        return GetFormattedText(textToFormat, fontType, fontSize, FontStyle.Normal, FontWeight.Normal, brush);
    }

    public static FormattedText GetFormattedText(string textToFormat, FontType fontType, double fontSize, FontStyle fontStyle, FontWeight fontWeight, SolidColorBrush brush)
    {
        var typeface = GetTypeface(fontType, fontStyle, fontWeight);
        return new FormattedText(textToFormat,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            brush);
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

    private static string BuildAvaresUri(string subfolder, string familyName)
        => $"avares://{assemblyName}/Fonts/{subfolder}#{familyName}";

    private static FontFamily CreateFontFamily(FontType ft)
    {
        return ft switch
        {
            // STIX families embedded as Avalonia resources:
            // Ensure the font files are included in your .csproj as <AvaloniaResource Include="Fonts\**\*" />
            FontType.STIXGeneral => new FontFamily(BuildAvaresUri("STIX", "STIXGeneral")),
            FontType.STIXIntegralsD => new FontFamily(BuildAvaresUri("STIX", "STIXIntegralsD")),
            FontType.STIXIntegralsSm => new FontFamily(BuildAvaresUri("STIX", "STIXIntegralsSm")),
            FontType.STIXIntegralsUp => new FontFamily(BuildAvaresUri("STIX", "STIXIntegralsUp")),
            FontType.STIXIntegralsUpD => new FontFamily(BuildAvaresUri("STIX", "STIXIntegralsUpD")),
            FontType.STIXIntegralsUpSm => new FontFamily(BuildAvaresUri("STIX", "STIXIntegralsUpSm")),
            FontType.STIXNonUnicode => new FontFamily(BuildAvaresUri("STIX", "STIXNonUnicode")),
            FontType.STIXSizeFiveSym => new FontFamily(BuildAvaresUri("STIX", "STIXSizeFiveSym")),
            FontType.STIXSizeFourSym => new FontFamily(BuildAvaresUri("STIX", "STIXSizeFourSym")),
            FontType.STIXSizeOneSym => new FontFamily(BuildAvaresUri("STIX", "STIXSizeOneSym")),
            FontType.STIXSizeThreeSym => new FontFamily(BuildAvaresUri("STIX", "STIXSizeThreeSym")),
            FontType.STIXSizeTwoSym => new FontFamily(BuildAvaresUri("STIX", "STIXSizeTwoSym")),
            FontType.STIXVariants => new FontFamily(BuildAvaresUri("STIX", "STIXVariants")),

            // System-installed fonts (platform-dependent availability)
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

            // Fallback/default
            _ => new FontFamily("Segoe UI"),
        };
    }

    public static Typeface GetTypeface(FontType fontType, FontStyle fontStyle, FontWeight fontWeight)
    {
        return new Typeface(GetFontFamily(fontType) ?? GetFontFamily(FontType.STIXGeneral), fontStyle, fontWeight, FontStretch.Normal);
    }
}
