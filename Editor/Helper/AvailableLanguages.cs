using System.Collections.Generic;

namespace Editor;

internal static class AvailableLanguages
{
    public static Language English = new("en", "English");

    public static List<Language> GetAvailableLanguages()
    {
        return
        [
            English,
        ];
    }

    public static string GetSystemTranslation(string languageCode)
    {
        return languageCode switch
        {
            _ => "System"
        };
    }
}

public class Language(string code, string display)
{
    /// <summary>
    /// E.g. En or Zh-CN
    /// </summary>
    public string LanguageCode { get; set; } = code;

    public string Display { get; set; } = display;
}
