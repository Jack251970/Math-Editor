using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Editor;

public static class ExceptionFormatter
{
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
        sb.AppendLine($"* OS Version: {GetWindowsFullVersionFromRegistry()}");
        sb.AppendLine($"* IntPtr Length: {nint.Size}");
        sb.AppendLine($"* x64: {Environment.Is64BitOperatingSystem}");
        sb.AppendLine($"* CLR Version: {Environment.Version}");
        sb.AppendLine($"* Installed .NET Framework: ");
        foreach (var result in GetFrameworkVersionFromRegistry())
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

    public static string RuntimeInfo()
    {
        var info =
            $"""

             App version: {Constants.Version}
             OS Version: {GetWindowsFullVersionFromRegistry()}
             IntPtr Length: {nint.Size}
             x64: {Environment.Is64BitOperatingSystem}
             """;
        return info;
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
}
