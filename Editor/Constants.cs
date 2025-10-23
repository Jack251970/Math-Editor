using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Editor;

public static class Constants
{
    public const string MathEditor = "MathEditor";
    public const string MathEditorFullName = "Math Editor";
    public const string ApplicationFileName = "Editor.exe";

    private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
    public static readonly string ProgramDirectory = Directory.GetParent(Assembly.Location)!.ToString();
    public static readonly string ExecutablePath = Path.Combine(ProgramDirectory, ApplicationFileName);
    public static readonly string ApplicationDirectory = Directory.GetParent(ProgramDirectory)!.ToString();
    public static readonly string RootDirectory = Directory.GetParent(ApplicationDirectory)!.ToString();

    public static readonly string Version = FileVersionInfo.GetVersionInfo(Assembly.Location).FileVersion!;
    public static readonly string Dev = "Dev";

    public static readonly string Images = "Images";
    private static readonly string ImagesDirectory = Path.Combine(ProgramDirectory, Images);
    public static readonly string AppIcon = Path.Combine(ImagesDirectory, "icon.png");

    private static readonly string ResourcesDirectory = Path.Combine(ProgramDirectory, "Resources");
    public static readonly string Latex2UnicodePath = Path.Combine(ResourcesDirectory, "unicode2latex.json");

    public const string Settings = "Settings";
    public const string Logs = "Logs";

    public const string SystemLanguageCode = "system";

    public const string IssuesUrl = "https://github.com/Jack251970/Math-Editor/issues";
}
