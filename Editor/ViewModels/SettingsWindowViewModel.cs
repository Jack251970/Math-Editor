using System.Collections.Generic;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

public partial class SettingsWindowViewModel(Settings settings, Internationalization translator) : ObservableObject, ICultureInfoChanged
{
    public Settings Settings { get; init; } = settings;

    private readonly Internationalization _translater = translator;

    public List<Language> Languages { get; } = Internationalization.LoadAvailableLanguages();

    public string Language
    {
        get => Settings.Language;
        set => _translater.ChangeLanguage(value);
    }

    public List<EditorModeLocalized> AllEditModes { get; } = EditorModeLocalized.GetValues();

    public List<FontTypeLocalized> AllFontTypes { get; } = FontTypeLocalized.GetValues();

    public List<CopyTypeLocalized> AllCopyTypes { get; } = CopyTypeLocalized.GetValues();

    public void OnCultureInfoChanged(CultureInfo newCultureInfo)
    {
        EditorModeLocalized.UpdateLabels(AllEditModes);
        FontTypeLocalized.UpdateLabels(AllFontTypes);
        CopyTypeLocalized.UpdateLabels(AllCopyTypes);
    }
}
