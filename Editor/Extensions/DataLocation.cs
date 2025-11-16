using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Editor;

public static class DataLocation
{
    private static readonly string ClassName = nameof(DataLocation);

    public const string PortableFolderName = "UserData";
    public const string DeletionIndicatorFile = ".dead";

    // Portable data lives next to the executable in a "UserData" folder (if writable and not deleted)
    public static readonly string PortableDataPath = Path.Combine(Constants.ProgramDirectory, PortableFolderName);

    // Default roaming/config data path depending on OS
    public static readonly string RoamingDataPath = GetDefaultDataHome();

    private static bool? _portableDataLocationInUse;

    public static string DataDirectory()
    {
        return PortableDataLocationInUse() ? PortableDataPath : RoamingDataPath;
    }

    private static bool PortableDataLocationInUse()
    {
        // Check cached value first
        if (_portableDataLocationInUse.HasValue)
        {
            return _portableDataLocationInUse.Value;
        }

        // Determine if portable data location is usable
        try
        {
            if (!Directory.Exists(PortableDataPath))
            {
                _portableDataLocationInUse = false;
                return false;
            }

            if (File.Exists(Path.Combine(PortableDataPath, DeletionIndicatorFile)))
            {
                _portableDataLocationInUse = false;
                return false;
            }

            _portableDataLocationInUse = IsDirectoryWritable(PortableDataPath);
            return _portableDataLocationInUse.Value;
        }
        catch (Exception e)
        {
            EditorLogger.Error(ClassName, "Error checking portable data location", e);
            _portableDataLocationInUse = false;
            return false;
        }
    }

    public static string VersionLogDirectory => Path.Combine(LogDirectory, Constants.Version);
    public static string LogDirectory => Path.Combine(DataDirectory(), Constants.Logs);

    public static string SettingsDirectory => Path.Combine(DataDirectory(), Constants.Settings);

    private static string GetDefaultDataHome()
    {
        try
        {
            // Windows: %AppData%\MathEditor
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appData, Constants.MathEditor);
            }

            // macOS: ~/Library/Application Support/MathEditor
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appData, Constants.MathEditor);
            }

            // Linux and other Unix-like: XDG_CONFIG_HOME or ~/.config/MathEditor
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
                var baseConfig = !string.IsNullOrWhiteSpace(xdgConfigHome)
                    ? xdgConfigHome
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
                return Path.Combine(baseConfig, Constants.MathEditor);
            }

            throw new PlatformNotSupportedException("Unsupported operating system for determining data directory.");
        }
        catch (Exception e)
        {
            EditorLogger.Fatal(ClassName, "Error determining default data home", e);
            // Fallback to portable data path if all else fails
            return PortableDataPath;
        }
    }

    private static bool IsDirectoryWritable(string path)
    {
        string testFile;
        try
        {
            testFile = Path.Combine(path, $".write_test_{Guid.NewGuid():N}");
        }
        catch (Exception e)
        {
            EditorLogger.Error(ClassName, $"Failed to create test file path: {path}", e);
            return false;
        }
        try
        {
            Directory.CreateDirectory(path);
            // Try with DeleteOnClose first
            using var _ = File.Create(testFile, 1, FileOptions.DeleteOnClose);
            return true;
        }
        catch (Exception e)
        {
            EditorLogger.Error(ClassName, $"Failed to write test file with DeleteOnClose option: {path}", e);
            try
            {
                File.WriteAllText(testFile, string.Empty);
                File.Delete(testFile);
                return true;
            }
            catch (Exception ex)
            {
                EditorLogger.Error(ClassName, $"Both write test methods failed for test file: {path}", ex);
                return false;
            }
        }
    }
}
