using System.Collections.Generic;

namespace Editor;

internal static class AvailableLanguages
{
    public static Language English = new("en", "English");
    public static Language Chinese = new("zh-cn", "中文");

    public static List<Language> GetAvailableLanguages()
    {
        return
        [
            English,
            Chinese,
        ];
    }

    public static string GetSystemTranslation(string languageCode)
    {
        return languageCode switch
        {
            "zh-cn" => "系统",
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

    public string ResourceKey => LanguageCode;
}
