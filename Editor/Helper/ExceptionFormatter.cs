using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace Editor;

public static class ExceptionFormatter
{
    private static readonly string ClassName = nameof(ExceptionFormatter);

    #region Exception Formatting

    public static string FormatException(Exception exception)
    {
        return CreateExceptionReport(exception);
    }

    private static string CreateExceptionReport(Exception? ex)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("## Exception");
        sb.AppendLine();
        sb.AppendLine("```");

        var exlist = new List<StringBuilder>();

        while (ex != null)
        {
            var exsb = new StringBuilder();
            exsb.Append(ex.GetType().FullName);
            exsb.Append(": ");
            exsb.AppendLine(ex.Message);
            if (ex.Source != null)
            {
                exsb.Append("   Source: ");
                exsb.AppendLine(ex.Source);
            }
            if (ex.TargetSite != null)
            {
                exsb.Append("   TargetAssembly: ");
                exsb.AppendLine(ex.TargetSite.Module.Assembly.ToString());
                exsb.Append("   TargetModule: ");
                exsb.AppendLine(ex.TargetSite.Module.ToString());
                exsb.Append("   TargetSite: ");
                exsb.AppendLine(ex.TargetSite.ToString());
            }
            exsb.AppendLine(ex.StackTrace);
            exlist.Add(exsb);

            ex = ex.InnerException;
        }

        foreach (var result in exlist.Select(o => o.ToString()).Reverse())
        {
            sb.AppendLine(result);
        }
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("## Environment");
        sb.AppendLine($"* Command Line: {Environment.CommandLine}");
        sb.AppendLine($"* Timestamp: {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"* App version: {Constants.Version}");
        sb.AppendLine($"* OS Version: {GetFullVersion()}");
        sb.AppendLine($"* IntPtr Length: {nint.Size}");
        sb.AppendLine($"* x64: {Environment.Is64BitOperatingSystem}");
        sb.AppendLine($"* CLR Version: {Environment.Version}");
        sb.AppendLine($"* Installed .NET Framework: ");
        foreach (var result in GetFrameworkVersion())
        {
            sb.Append("   * ");
            sb.AppendLine(result);
        }

        sb.AppendLine();
        sb.AppendLine("## Assemblies - " + AppDomain.CurrentDomain.FriendlyName);
        sb.AppendLine();
        foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
        {
            sb.Append("* ");
            sb.Append(ass.FullName);
            sb.Append(" (");

            if (ass.IsDynamic)
            {
                sb.Append("dynamic assembly doesn't has location");
            }
            else if (string.IsNullOrEmpty(ass.Location))
            {
                sb.Append("location is null or empty");
            }
            else
            {
                sb.Append(ass.Location);

            }
            sb.AppendLine(")");
        }

        return sb.ToString();
    }

    #endregion

    #region DotNet Framework

    private static List<string> GetFrameworkVersion()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetFrameworkVersionFromRegistry();
            }

            // On non-Windows, '.NET Framework' doesn't apply. Report current runtime
            // and try to enumerate installed .NET runtimes if possible.
            var result = new List<string>
            {
                $"Runtime: {RuntimeInformation.FrameworkDescription}",
                $"CLR: {Environment.Version}"
            };

            foreach (var line in TryGetDotnetRuntimesViaCli())
            {
                result.Add(line);
            }

            // If CLI enumeration returned nothing, try to enumerate known install locations.
            if (result.Count <= 2)
            {
                foreach (var line in TryGetDotnetRuntimesFromDisk())
                {
                    result.Add(line);
                }
            }

            // If still nothing extra, annotate that enumeration was not available
            if (result.Count <= 2)
            {
                result.Add("Installed runtimes: unavailable (dotnet not found)");
            }

            return result;
        }
        catch (Exception e)
        {
            EditorLogger.Error(ClassName, "Failed to get framework/runtime version info", e);
            return [];
        }
    }

    // http://msdn.microsoft.com/en-us/library/hh925568%28v=vs.110%29.aspx
    private static List<string> GetFrameworkVersionFromRegistry()
    {
        try
        {
            var result = new List<string>();
            using (var ndpKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
            {
                foreach (var versionKeyName in ndpKey!.GetSubKeyNames())
                {
                    if (versionKeyName.StartsWith('v'))
                    {
                        var versionKey = ndpKey.OpenSubKey(versionKeyName);
                        var name = versionKey!.GetValue("Version", "") as string;
                        var sp = versionKey.GetValue("SP", "").ToString();
                        var install = versionKey.GetValue("Install", "").ToString();
                        if (install != "")
                        {
                            if (sp != "" && install == "1")
                            {
                                result.Add(string.Format("{0} {1} SP{2}", versionKeyName, name, sp));
                            }
                            else
                            {
                                result.Add(string.Format("{0} {1}", versionKeyName, name));
                            }
                        }

                        if (name != "")
                        {
                            continue;
                        }

                        foreach (var subKeyName in versionKey.GetSubKeyNames())
                        {
                            var subKey = versionKey.OpenSubKey(subKeyName);
                            name = subKey!.GetValue("Version", "") as string;
                            if (name != "")
                            {
                                sp = subKey.GetValue("SP", "").ToString();
                            }
                            install = subKey.GetValue("Install", "").ToString();
                            if (install != "")
                            {
                                if (sp != "" && install == "1")
                                {
                                    result.Add(string.Format("{0} {1} {2} SP{3}", versionKeyName, subKeyName, name, sp));
                                }
                                else if (install == "1")
                                {
                                    result.Add(string.Format("{0} {1} {2}", versionKeyName, subKeyName, name));
                                }
                            }
                        }
                    }
                }
            }
            using (var ndpKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"))
            {
                if (ndpKey!.GetValue("Release") is int releaseKey)
                {
                    if (releaseKey == 378389)
                    {
                        result.Add("v4.5");
                    }
                    else if (releaseKey == 378675)
                    {
                        result.Add("v4.5.1 installed with Windows 8.1");
                    }
                    else if (releaseKey == 378758)
                    {
                        result.Add("4.5.1 installed on Windows 8, Windows 7 SP1, or Windows Vista SP2");
                    }
                }
            }
            return result;
        }
        catch
        {
            return [];
        }
    }

    // Helpers to discover installed .NET runtimes on non-Windows
    private static List<string> TryGetDotnetRuntimesViaCli()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--list-runtimes",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            if (proc == null)
            {
                return [];
            }

            var output = proc.StandardOutput.ReadToEnd();
            // Best effort timeout
            if (!proc.WaitForExit(2000))
            {
                proc.Kill(entireProcessTree: true);
                proc.WaitForExit();
            }

            var lines = output
                .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .ToArray();

            if (lines.Length == 0)
            {
                return [];
            }

            // Prefix to make it clear these are runtimes
            return [.. lines.Select(l => $"runtime: {l}")];
        }
        catch (Exception e)
        {
            EditorLogger.Error(ClassName, "Failed to list runtimes via dotnet CLI", e);
            return [];
        }
    }

    private static List<string> TryGetDotnetRuntimesFromDisk()
    {
        try
        {
            var results = new List<string>();

            var candidates = new List<string?>
            {
                Environment.GetEnvironmentVariable("DOTNET_ROOT"),
                "/usr/local/share/dotnet",
                "/usr/share/dotnet",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet")
            };

            foreach (var root in candidates.Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                try
                {
                    var shared = Path.Combine(root!, "shared");
                    results.AddRange(EnumerateFrameworkFolders(Path.Combine(shared, "Microsoft.NETCore.App"), "Microsoft.NETCore.App"));
                    results.AddRange(EnumerateFrameworkFolders(Path.Combine(shared, "Microsoft.AspNetCore.App"), "Microsoft.AspNetCore.App"));
                }
                catch (Exception e)
                {
                    EditorLogger.Error(ClassName, "Failed to enumerate dotnet runtimes from disk", e);
                }
            }

            return results;
        }
        catch (Exception e)
        {
            EditorLogger.Error(ClassName, "Failed to enumerate runtimes from disk", e);
            return [];
        }
    }

    private static List<string> EnumerateFrameworkFolders(string path, string name)
    {
        if (!Directory.Exists(path))
        {
            return [];
        }

        try
        {
            return [.. Directory.GetDirectories(path).Select(d => BuildRuntimeInfoLine(name, Path.GetFileName(d), d))];
        }
        catch (Exception e)
        {
            EditorLogger.Error(ClassName, $"Failed to enumerate framework folders in {path}", e);
            return [];
        }
    }

    private static string BuildRuntimeInfoLine(string name, string version, string path)
    {
        var sb = new StringBuilder();
        sb.Append("runtime: ");
        sb.Append(name);
        sb.Append(' ');
        sb.Append(version);
        sb.Append(' ');
        sb.Append('[');
        sb.Append(path);
        sb.Append(']');
        return sb.ToString();
    }

    #endregion

    #region Runtime Info

    public static string RuntimeInfo()
    {
        var info =
            $"""

             App version: {Constants.Version}
             OS Version: {GetFullVersion()}
             IntPtr Length: {nint.Size}
             x64: {Environment.Is64BitOperatingSystem}
             """;
        return info;
    }

    #endregion

    #region OS Version

    private static string GetFullVersion()
    {
        // Cross-platform full version/friendly OS string.
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowsFullVersionFromRegistry();
            }

            // For non-Windows platforms, fall back to RuntimeInformation.
            var osDescription = RuntimeInformation.OSDescription.Trim();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Try to read /etc/os-release for a friendlier distro name.
                try
                {
                    const string osReleasePath = "/etc/os-release";
                    if (File.Exists(osReleasePath))
                    {
                        var lines = File.ReadAllLines(osReleasePath);
                        var dict = lines
                            .Select(l => l.Split('=', 2))
                            .Where(p => p.Length == 2)
                            .ToDictionary(p => p[0], p => p[1].Trim('"'));

                        if (dict.TryGetValue("PRETTY_NAME", out var pretty))
                        {
                            return pretty;
                        }

                        if (dict.TryGetValue("NAME", out var name))
                        {
                            if (dict.TryGetValue("VERSION", out var version))
                            {
                                return $"{name} {version}";
                            }
                            if (dict.TryGetValue("VERSION_ID", out var versionId))
                            {
                                return $"{name} {versionId}";
                            }
                            return name;
                        }
                    }
                }
                catch (Exception e)
                {
                    EditorLogger.Error(ClassName, "Failed to read /etc/os-release for OS version", e);
                }

                return osDescription;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // OSDescription usually contains Darwin kernel + version. Good enough.
                return osDescription;
            }

            throw new PlatformNotSupportedException("Unsupported OS platform");
        }
        catch (Exception e)
        {
            EditorLogger.Error(ClassName, "Failed to read OS version", e);
            return Environment.OSVersion.VersionString;
        }
    }

    private static string GetWindowsFullVersionFromRegistry()
    {
        try
        {
            var buildRevision = GetWindowsRevisionFromRegistry();
            var currentBuild = Environment.OSVersion.Version.Build;
            return currentBuild.ToString() + "." + buildRevision;
        }
        catch
        {
            return Environment.OSVersion.VersionString;
        }
    }

    private static string GetWindowsRevisionFromRegistry()
    {
        try
        {
            using var registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\");
            var buildRevision = registryKey?.GetValue("UBR")?.ToString();
            return buildRevision ?? throw new ArgumentNullException();
        }
        catch
        {
            return "0";
        }
    }

    #endregion
}
