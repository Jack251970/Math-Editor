using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Editor;

public class Internationalization(Settings settings)
{
    private static readonly string ClassName = nameof(Internationalization);

    private const string Folder = "Languages";
    private const string DefaultLanguageCode = "en";
    private const string DefaultFile = "en.xaml";
    private const string Extension = ".xaml";
    private readonly Settings _settings = settings;
    private readonly List<string> _languageDirectories = [];
    private readonly List<ResourceDictionary> _oldResources = [];
    private static string SystemLanguageCode = null!;

    #region Initialization

    /// <summary>
    /// Initialize the system language code based on the current culture.
    /// </summary>
    public static void InitSystemLanguageCode()
    {
        var availableLanguages = AvailableLanguages.GetAvailableLanguages();

        // Retrieve the language identifiers for the current culture.
        // ChangeLanguage method overrides the CultureInfo.CurrentCulture, so this needs to
        // be called at startup in order to get the correct lang code of system. 
        var currentCulture = CultureInfo.CurrentCulture;
        var twoLetterCode = currentCulture.TwoLetterISOLanguageName;
        var threeLetterCode = currentCulture.ThreeLetterISOLanguageName;
        var fullName = currentCulture.Name;

        // Try to find a match in the available languages list
        foreach (var language in availableLanguages)
        {
            var languageCode = language.LanguageCode;

            if (string.Equals(languageCode, twoLetterCode, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(languageCode, threeLetterCode, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(languageCode, fullName, StringComparison.OrdinalIgnoreCase))
            {
                SystemLanguageCode = languageCode;
            }
        }

        SystemLanguageCode = DefaultLanguageCode;
    }

    /// <summary>
    /// Initialize language. Will change app language based on settings.
    /// </summary>
    public async Task InitializeLanguageAsync()
    {
        // Get actual language
        var languageCode = _settings.Language;
        if (languageCode == Constants.SystemLanguageCode)
        {
            languageCode = SystemLanguageCode;
        }

        // Get language by language code and change language
        var language = GetLanguageByLanguageCode(languageCode);

        // Add app language directory
        AddAppLanguageDirectory();

        // Load default language resources
        LoadDefaultLanguage();

        // Change language
        await ChangeLanguageAsync(language);
    }

    private void AddAppLanguageDirectory()
    {
        // Check if app language directory exists
        var directory = Path.Combine(Constants.ProgramDirectory, Folder);
        if (!Directory.Exists(directory))
        {
            EditorLogger.Error(ClassName, $"App language directory can't be found <{directory}>");
            return;
        }

        _languageDirectories.Add(directory);
    }

    private void LoadDefaultLanguage()
    {
        // Prevents the language app started in from overwriting English if the user switches back to English
        RemoveOldLanguageFiles();
        LoadLanguage(AvailableLanguages.English);
        _oldResources.Clear();
    }

    #endregion

    #region Change Language

    /// <summary>
    /// Change language during runtime. Will change app language & save settings.
    /// </summary>
    /// <param name="languageCode"></param>
    public void ChangeLanguage(string languageCode)
    {
        // Get actual language if language code is system
        var isSystem = false;
        if (languageCode == Constants.SystemLanguageCode)
        {
            languageCode = SystemLanguageCode;
            isSystem = true;
        }

        // Get language by language code and change language
        var language = GetLanguageByLanguageCode(languageCode);

        // Change language
        _ = ChangeLanguageAsync(language);

        // Save settings
        _settings.Language = isSystem ? Constants.SystemLanguageCode : language.LanguageCode;
    }

    private static Language GetLanguageByLanguageCode(string languageCode)
    {
        var language = AvailableLanguages.GetAvailableLanguages().
            FirstOrDefault(o => o.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase));
        if (language == null)
        {
            EditorLogger.Error(ClassName, $"Language code can't be found <{languageCode}>");
            return AvailableLanguages.English;
        }
        else
        {
            return language;
        }
    }

    private async Task ChangeLanguageAsync(Language language)
    {
        // Remove old language files and load language
        RemoveOldLanguageFiles();
        if (language != AvailableLanguages.English)
        {
            LoadLanguage(language);
        }

        // Change culture info
        ChangeCultureInfo(language.LanguageCode);

        // Update translations for all windows
        await Application.Current.Dispatcher.InvokeAsync(UpdateAllWindowsTranslations);
    }

    private void UpdateAllWindowsTranslations()
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window is ICultureInfoChanged localizable)
            {
                localizable.OnCultureInfoChanged(CultureInfo.CurrentCulture);
            }
        }
    }

    public static void ChangeCultureInfo(string languageCode)
    {
        // Culture of main thread
        // Use CreateSpecificCulture to preserve possible user-override settings in Windows
        // if app language culture is the same as Windows's
        CultureInfo currentCulture;
        try
        {
            currentCulture = CultureInfo.CreateSpecificCulture(languageCode);
        }
        catch (CultureNotFoundException)
        {
            currentCulture = CultureInfo.CreateSpecificCulture(SystemLanguageCode);
        }
        CultureInfo.CurrentCulture = currentCulture;
        CultureInfo.CurrentUICulture = currentCulture;
        var thread = Thread.CurrentThread;
        thread.CurrentCulture = currentCulture;
        thread.CurrentUICulture = currentCulture;
    }

    #endregion

    #region Language Resources Management

    private void RemoveOldLanguageFiles()
    {
        var dicts = Application.Current.Resources.MergedDictionaries;
        foreach (var r in _oldResources)
        {
            dicts.Remove(r);
        }
        _oldResources.Clear();
    }

    private void LoadLanguage(Language language)
    {
        var appEnglishFile = Path.Combine(Constants.ProgramDirectory, Folder, DefaultFile);
        var dicts = Application.Current.Resources.MergedDictionaries;
        var filename = $"{language.LanguageCode}{Extension}";
        var files = _languageDirectories
            .Select(d => LanguageFile(d, filename))
            // Exclude app English language file since it's built into the binary, and there's no need to load
            // it again from the file system.
            .Where(f => !string.IsNullOrEmpty(f) && f != appEnglishFile)
            .ToArray();

        if (files.Length > 0)
        {
            foreach (var f in files)
            {
                var r = new ResourceDictionary
                {
                    Source = new Uri(f, UriKind.Absolute)
                };
                dicts.Add(r);
                _oldResources.Add(r);
            }
        }
    }

    private static string LanguageFile(string folder, string language)
    {
        if (Directory.Exists(folder))
        {
            var path = Path.Combine(folder, language);
            if (File.Exists(path))
            {
                return path;
            }
            else
            {
                EditorLogger.Error(ClassName, $"Language path can't be found <{path}>");
                var english = Path.Combine(folder, DefaultFile);
                if (File.Exists(english))
                {
                    return english;
                }
                else
                {
                    EditorLogger.Error(ClassName, $"Default English Language path can't be found <{path}>");
                    return string.Empty;
                }
            }
        }
        else
        {
            return string.Empty;
        }
    }

    #endregion

    #region Available Languages

    public static List<Language> LoadAvailableLanguages()
    {
        var list = AvailableLanguages.GetAvailableLanguages();
        list.Insert(0, new Language(Constants.SystemLanguageCode, AvailableLanguages.GetSystemTranslation(SystemLanguageCode)));
        return list;
    }

    #endregion

    #region Get Translations

    public static string GetTranslation(string key)
    {
        var translation = Application.Current.TryFindResource(key);
        if (translation is string)
        {
            return translation.ToString()!;
        }
        else
        {
            EditorLogger.Error(ClassName, $"No translation for key {key}");
            return $"No translation for key {key}";
        }
    }

    #endregion
}
