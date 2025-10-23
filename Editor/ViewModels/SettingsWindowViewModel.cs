using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

public partial class SettingsWindowViewModel(Settings settings, Internationalization translater) : ObservableObject
{
    public Settings Settings { get; init; } = settings;

    #region Language

    private readonly Internationalization _translater = translater;

    public List<Language> Languages => _translater.LoadAvailableLanguages();

    public string Language
    {
        get => Settings.Language;
        set
        {
            _translater.ChangeLanguage(value);
            UpdateTranslations();
        }
    }

    private void UpdateTranslations()
    {
        EditorModeLocalized.UpdateLabels(AllEditModes);
        FontTypeLocalized.UpdateLabels(AllFontTypes);
    }

    #endregion

    #region Editor Mode

    public List<EditorModeLocalized> AllEditModes { get; } = EditorModeLocalized.GetValues();

    #endregion

    #region Font Type

    public List<FontTypeLocalized> AllFontTypes { get; } = FontTypeLocalized.GetValues();

    #endregion

    #region Copy Type

    public List<CopyTypeLocalized> AllCopyTypes { get; } = CopyTypeLocalized.GetValues();

    #endregion
}
