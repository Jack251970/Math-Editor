using Avalonia.Controls;

namespace Editor;

public static class Constants
{
    public const string MathEditor = "MathEditor";
    public const string MathEditorFullName = "Math Editor";

    public static string Version
    {
        get => field ??= Design.IsDesignMode ?
            "1.0.0" :
            App.UpdateManager.CurrentVersion?.ToString() ?? "1.0.0";
    }
    public static readonly string Dev = "Dev";

    public const string Settings = "Settings";
    public const string Logs = "Logs";

    public const string SystemLanguageCode = "system";

    public const string MedExtension = ".med";

    public const string RepositoryUrl = "https://github.com/Jack251970/Math-Editor";
    public const string IssuesUrl = "https://github.com/Jack251970/Math-Editor/issues";
    public const string WikiUrl = "https://github.com/Jack251970/Math-Editor/wiki";
    public const string SponsorUrl = "https://ko-fi.com/jackye";

    public const int MaxSymbols = 30;

    public const string Latex2UnicodePath = "avares://Editor/Resources/unicode2latex.json";
}
