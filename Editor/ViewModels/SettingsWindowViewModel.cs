using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

public partial class SettingsWindowViewModel(Settings settings, Internationalization translater) : ObservableObject
{
    public Settings Settings { get; init; } = settings;

    private readonly Internationalization _translater = translater;

    public List<Language> Languages { get; } = Internationalization.LoadAvailableLanguages();

    public string Language
    {
        get => Settings.Language;
        set
        {
            _translater.ChangeLanguage(value);
            UpdateTranslations();
        }
    }

    public void UpdateTranslations()
    {
        EditorModeLocalized.UpdateLabels(AllEditModes);
        FontTypeLocalized.UpdateLabels(AllFontTypes);
        CopyTypeLocalized.UpdateLabels(AllCopyTypes);
    }

    public List<EditorModeLocalized> AllEditModes { get; } = EditorModeLocalized.GetValues();

    public List<FontTypeLocalized> AllFontTypes { get; } = FontTypeLocalized.GetValues();

    public List<CopyTypeLocalized> AllCopyTypes { get; } = CopyTypeLocalized.GetValues();
}
