using System;
using System.IO;

namespace Editor;

public static class DataLocation
{
    public const string PortableFolderName = "UserData";
    public const string DeletionIndicatorFile = ".dead";
    public static readonly string PortableDataPath = Path.Combine(Constants.ProgramDirectory, PortableFolderName);
    public static readonly string RoamingDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.MathEditor);
    public static string DataDirectory() =>
        PortableDataLocationInUse() ? PortableDataPath : RoamingDataPath;

    public static bool PortableDataLocationInUse() =>
        Directory.Exists(PortableDataPath) && !File.Exists(Path.Combine(PortableDataPath, DeletionIndicatorFile));

    public static string VersionLogDirectory => Path.Combine(LogDirectory, Constants.Version);
    public static string LogDirectory => Path.Combine(DataDirectory(), Constants.Logs);

    public static readonly string SettingsDirectory = Path.Combine(DataDirectory(), Constants.Settings);
}
