using System.Text.RegularExpressions;

namespace Editor.Localization.Shared
{
    public static class Constants
    {
        public const string DefaultNamespace = "Editor";
        public const string ClassName = "Localize";
        public const string SystemPrefixUri = "clr-namespace:System;assembly=mscorlib";
        public const string XamlPrefixUri = "http://schemas.microsoft.com/winfx/2006/xaml";
        public const string XamlTag = "String";
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
        public const string SuppressWarning = "#pragma warning disable FLAN0001 // Old localization API used";

        public static readonly Regex LanguagesXamlRegex = new Regex(@"\\Languages\\[^\\]+\.xaml$", RegexOptions.IgnoreCase);
        public static readonly string[] OldLocalizationClasses = { "Internationalization" };
    }
}
