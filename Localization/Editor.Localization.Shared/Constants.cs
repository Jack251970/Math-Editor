using System.Text.RegularExpressions;

namespace Editor.Localization.Shared
{
    public static class Constants
    {
        public const string OldLocalizationApiUsedId = "FLAN0001";
        public const string OldLocalizationApiUsedTitle = "Old localization API used";
        public const string CouldNotFindResourceDictionariesId = "FLSG0001";
        public const string LocalizationKeyUnusedId = "FLSG0002";
        public const string EnumFieldLocalizationKeyValueInvalidId = "FLSG0003";

        public const string DefaultNamespace = "Editor";
        public const string ClassName = "Localize";
        public const string SystemPrefixUri = "http://schemas.microsoft.com/winfx/2006/xaml";
        public const string AxamlPrefixUri = "http://schemas.microsoft.com/winfx/2006/xaml";
        public const string AxamlTag = "String";
        public const string KeyAttribute = "Key";
        public const string SummaryElementName = "summary";
        public const string ParamElementName = "param";
        public const string IndexAttribute = "index";
        public const string NameAttribute = "name";
        public const string TypeAttribute = "type";
        public const string OldLocalizationMethodName = "GetTranslation";
        public const string StringFormatMethodName = "Format";
        public const string StringFormatTypeName = "string";
        public const string EnumLocalizeClassSuffix = "Localized";
        public const string EnumLocalizeAttributeName = "EnumLocalizeAttribute";
        public const string EnumLocalizeKeyAttributeName = "EnumLocalizeKeyAttribute";
        public const string EnumLocalizeValueAttributeName = "EnumLocalizeValueAttribute";
        public const string Internationalization = "Internationalization";
        public static readonly string SuppressWarning = $"#pragma warning disable {OldLocalizationApiUsedId} // {OldLocalizationApiUsedTitle}";
        // Regex to match file paths ending with /Languages/en.axaml on Linux & MacOS or \Languages\en.axaml on Windows
        public static readonly Regex LanguagesAxamlRegex = new Regex(@"[\\/]+Languages[\\/]en\.axaml$", RegexOptions.IgnoreCase);
        public static readonly string[] OldLocalizationClasses = { "Internationalization" };
    }
}
