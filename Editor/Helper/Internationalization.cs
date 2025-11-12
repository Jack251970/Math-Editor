using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;

namespace Editor;

public class Internationalization(Settings settings) : IDisposable
{
    private static readonly string ClassName = nameof(Internationalization);

    private const string DefaultLanguageCode = "en";
    private readonly Settings _settings = settings;
    private readonly List<ResourceDictionary> _oldResources = [];
    private static string SystemLanguageCode = null!;
    private readonly SemaphoreSlim _langChangeLock = new(1, 1);

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

        // Change language
        await ChangeLanguageAsync(language);
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
        await _langChangeLock.WaitAsync();

        try
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
            await Dispatcher.UIThread.InvokeAsync(UpdateAllWindowsTranslations);
        }
        catch (Exception e)
        {
            EditorLogger.Fatal(ClassName, $"Failed to change language to <{language.LanguageCode}>", e);
        }
        finally
        {
            _langChangeLock.Release();
        }
    }

    private void UpdateAllWindowsTranslations()
    {
        foreach (var window in WindowTracker.GetAllWindows())
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
        var dicts = Application.Current!.Resources.MergedDictionaries;
        foreach (var r in _oldResources)
        {
            dicts.Remove(r);
        }
        _oldResources.Clear();
    }

    private void LoadLanguage(Language language)
    {
        if (Application.Current!.TryGetResource(language.ResourceKey, null, out var resource) &&
            resource is ResourceDictionary languageResource)
        {
            Application.Current!.Resources.MergedDictionaries.Add(languageResource);
            _oldResources.Add(languageResource);
        }
        else
        {
            EditorLogger.Error(ClassName, $"Language resource not found for language code <{language.LanguageCode}>" +
                $" with resource key <{language.ResourceKey}>");
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
        if (Application.Current!.TryGetResource(key, ThemeVariant.Default, out var value) && value is string translatedString)
        {
            return translatedString;
        }
        else
        {
            EditorLogger.Error(ClassName, $"No translation for key {key}");
            return $"No translation for key {key}";
        }
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        RemoveOldLanguageFiles();
        _langChangeLock.Dispose();
    }

    #endregion
}
