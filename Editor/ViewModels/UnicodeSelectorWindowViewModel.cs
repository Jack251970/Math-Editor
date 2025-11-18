using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

public partial class UnicodeSelectorWindowViewModel : ObservableObject, ICultureInfoChanged
{
    private static readonly string ClassName = nameof(UnicodeSelectorWindowViewModel);

    public List<string> Categories { get; }

    [ObservableProperty]
    public partial string SelectedCategory { get; set; } = string.Empty;

    public FontFamily SymbolFontFamily { get; } = FontFactory.GetFontFamily(FontType.STIXGeneral);

    [ObservableProperty]
    public partial List<UnicodeItem> CategorySource { get; set; } = [];

    [ObservableProperty]
    public partial UnicodeItem? SelectedSymbolItem { get; set; } = null;

    public Settings Settings { get; } = App.Settings;

    [ObservableProperty]
    public partial UnicodeItem? SelectedRecentSymbolItem { get; set; } = null;

    [ObservableProperty]
    public partial bool? SelectRecentList { get; set; } = null;

    [ObservableProperty]
    public partial string CharacterCodeText { get; set; } = string.Empty;

    public List<UnicodeFormatLocalized> AllUnicodeFormats { get; } = UnicodeFormatLocalized.GetValues();

    [ObservableProperty]
    public partial UnicodeFormat UnicodeFormat { get; set; } = UnicodeFormat.Decimal;

    private static bool _isCategoriesSetup = false;
    private static readonly Dictionary<string, List<UnicodeItem>> _categories = [];
    private static readonly List<UnicodeItem> _allList = [];

    public UnicodeSelectorWindowViewModel()
    {
        if (!_isCategoriesSetup)
        {
            SetupCategories();
            _isCategoriesSetup = true;
        }
        Categories = [.. _categories.Keys];
        SelectedCategory = Categories[0];
    }

    private static void SetupCategories()
    {
        SetupCategory("Mathematical Operators & Number Forms", 0x2150, 0x218F);
        SetupCategory("Mathematical Operators & Number Forms", 0x2200, 0x22FF); //Mathematical Operators    
        SetupCategory("Miscellaneous Mathematical", 0x27C0, 0x27EF); // Symbols-A
        SetupCategory("Basic Latin", 0x021, 0x7E);
        SetupCategory("Latin-1 Supplement", 0x0A1, 0xAC);
        SetupCategory("Latin-1 Supplement", 0x0AE, 0xFF);
        SetupCategory("Latin Extended", 0x100, 0x17F); //A
        SetupCategory("Latin Extended", 0x180, 0x237); //B
        SetupCategory("Punctuation & Diacritical Marks", 0x2000, 0x206F); //General Punctuation
        SetupCategory("Punctuation & Diacritical Marks", 0x2B0, 0x2FF); //Spacing Modifier Letters
        SetupCategory("Punctuation & Diacritical Marks", 0x300, 0x36F); //Combining Diacritical Marks
        SetupCategory("Greek and Coptic", 0x370, 0x3FF);
        SetupCategory("Cyrillic", 0x400, 0x4FF);
        SetupCategory("Currency Symbols & Phonetic Extensions", 0x20A0, 0x20CF); //Currency Symbols
        SetupCategory("Currency Symbols & Phonetic Extensions", 0x1D00, 0x1D7F); //Phonetic Extensions
        SetupCategory("Currency Symbols & Phonetic Extensions", 0x1D80, 0x1DBF); //Phonetic Extensions Supplement
        SetupCategory("Latin Extended", 0x1E00, 0x1EFF); //Latin Extended Additional
        SetupCategory("Punctuation & Diacritical Marks", 0x20D0, 0x20FF); //Combining Diacritical Marks for Symbols
        SetupCategory("Letterlike Symbols", 0x2100, 0x214F);
        SetupCategory("Arrows", 0x2190, 0x21FF);
        SetupCategory("Miscellaneous", 0x2300, 0x23FF); //Miscellaneous Technical
        SetupCategory("Miscellaneous", 0x2400, 0x243F); //Control Pictures
        SetupCategory("Enclosed Alphanumerics", 0x2460, 0x24FF);
        SetupCategory("Shapes", 0x2700, 0x27BF); //Dingbats 
        SetupCategory("Shapes", 0x2500, 0x257F); //Box Drawing
        SetupCategory("Shapes", 0x25A0, 0x25FF); //Geometric Shapes
        SetupCategory("Miscellaneous", 0x2600, 0x26FF); //Miscellaneous Symbols
        SetupCategory("Arrows", 0x27F0, 0x27FF);
        SetupCategory("Arrows", 0x2900, 0x297F);
        SetupCategory("Miscellaneous Mathematical", 0x2980, 0x29FF); // Symbols-B
        SetupCategory("Supplemental Mathematical Operators", 0x2A00, 0x2AFF);
        SetupCategory("Miscellaneous", 0x2B12, 0x2B54); //Miscellaneous Symbols and Arrows
        SetupCategory("Miscellaneous", 0xFB00, 0xFB4F); //Alphabetic Presentation Forms
        //SetupCategory("Mathematical Alphanumeric Symbols", 0x1D400, 0x1D7FF);

        _categories.Add("All", _allList);
    }

    private static void SetupCategory(string categoryName, int start, int end)
    {
        var list = new List<UnicodeItem>();
        for (var i = start; i <= end; i++)
        {
            if (TypefaceContainsCharacter(FontFactory.GetTypeface(FontType.STIXGeneral, FontStyle.Normal, FontWeight.Normal), Convert.ToChar(i)))
            {
                var item = new UnicodeItem
                {
                    //FontFamily = family,
                    //HexString = "0x" + i.ToString("X4"),
                    CodePoint = i,
                    UnicodeText = string.Format("{0}", Convert.ToChar(i))
                };
                list.Add(item);
                _allList.Add(item);
            }
        }
        if (_categories.TryGetValue(categoryName, out var oldList))
        {
            foreach (var item in list)
            {
                oldList.Add(item);
            }
        }
        else
        {
            _categories.Add(categoryName, list);
        }
    }

    private static bool TypefaceContainsCharacter(Typeface typeface, char characterToCheck)
    {
        try
        {
            var glyphTypeface = typeface.GlyphTypeface;
            uint codepoint = characterToCheck;
            return glyphTypeface.TryGetGlyph(codepoint, out _);
        }
        catch (Exception e)
        {
            EditorLogger.Error(ClassName, $"Error checking typeface for character {characterToCheck}: {e}");
            return false;
        }
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        CategorySource = _categories[value];
    }

    partial void OnCategorySourceChanged(List<UnicodeItem> value)
    {
        SelectedSymbolItem = null;
        // If the recent list has selected item, we should not clear it
        // So the selected item is still valid and we do not reset this flag
        if (SelectRecentList != true)
        {
            SelectRecentList = null;
        }
    }

    partial void OnSelectedSymbolItemChanged(UnicodeItem? value)
    {
        if (value != null)
        {
            ChangeCharacterCode(UnicodeFormat, value);
            SelectRecentList = false;
            SelectedRecentSymbolItem = null;
        }
    }

    partial void OnSelectedRecentSymbolItemChanged(UnicodeItem? value)
    {
        if (value != null)
        {
            ChangeCharacterCode(UnicodeFormat, value);
            SelectRecentList = true;
            SelectedSymbolItem = null;
        }
    }

    partial void OnUnicodeFormatChanged(UnicodeFormat value)
    {
        if (GetSelectedItem() is UnicodeItem item)
        {
            ChangeCharacterCode(value, item);
        }
    }

    private void ChangeCharacterCode(UnicodeFormat value, UnicodeItem item)
    {
        var numberBase = value switch
        {
            UnicodeFormat.Octal => 8,
            UnicodeFormat.Decimal => 10,
            UnicodeFormat.Hexadecimal => 16,
            _ => throw new NotImplementedException(),
        };
        var numberString = Convert.ToString(item.CodePoint, numberBase);
        if (numberBase == 16)
        {
            numberString = numberString.ToUpper().PadLeft(4, '0');
        }
        else if (numberBase == 8)
        {
            numberString = numberString.PadLeft(6, '0');
        }
        CharacterCodeText = numberString;
    }

    public UnicodeItem? GetSelectedItem()
    {
        if (SelectRecentList.HasValue)
        {
            return SelectRecentList.Value ? SelectedRecentSymbolItem : SelectedSymbolItem;
        }
        else
        {
            return null;
        }
    }

    public void OnCultureInfoChanged(CultureInfo newCultureInfo)
    {
        UnicodeFormatLocalized.UpdateLabels(AllUnicodeFormats);
    }
}
