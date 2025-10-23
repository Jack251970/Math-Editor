using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

public partial class SettingsWindowViewModel(Settings settings, Internationalization translater) : ObservableObject
{
    public Settings Settings { get; init; } = settings;

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

    public List<EditorModeLocalized> AllEditModes { get; } = EditorModeLocalized.GetValues();

    public List<FontTypeLocalized> AllFontTypes { get; } = FontTypeLocalized.GetValues();

    public List<CopyTypeLocalized> AllCopyTypes { get; } = CopyTypeLocalized.GetValues();
}
