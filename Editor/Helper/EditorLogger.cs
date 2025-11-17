using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Editor;

public static class EditorLogger
{
    private const string SourceContext = "SourceContext";
    private static bool _initialized = false;

    public static void Initialize()
    {
        // Ensure logging directory exists
        var logDir = DataLocation.VersionLogDirectory;
        if (!Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        // Set LOGGING_ROOT environment variable for compatibility with other components
        Environment.SetEnvironmentVariable("LOGGING_ROOT", logDir);

        var filePath = Path.Combine(logDir, "math-editor-.log");
        var outputTemplate = "[{Timestamp:yyyy/MM/dd HH:mm:ss.fff} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}";

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(
                outputTemplate: outputTemplate,
                restrictedToMinimumLevel: LogEventLevel.Verbose)
            .WriteTo.File(
                path: filePath,
                outputTemplate: outputTemplate,
                restrictedToMinimumLevel: LogEventLevel.Debug,
                rollingInterval: RollingInterval.Day)
            .WriteTo.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithProperty(SourceContext, Constants.MathEditor)
            .CreateLogger();

        _initialized = true;
    }

    public static IHostBuilder ConfigureLogger(this IHostBuilder builder)
    {
        return builder.ConfigureLogging(builder => builder.AddSerilog(dispose: true));
    }

    public static void Verbose(string className, string message, [CallerMemberName] string methodName = "")
    {
        var logger = GetLogger(className, methodName);
        logger.Verbose(message);
    }

    public static void Debug(string className, string message, [CallerMemberName] string methodName = "")
    {
        var logger = GetLogger(className, methodName);
        logger.Debug(message);
    }

    public static void Info(string className, string message, [CallerMemberName] string methodName = "")
    {
        var logger = GetLogger(className, methodName);
        logger.Information(message);
    }

    public static void Warning(string className, string message, [CallerMemberName] string methodName = "")
    {
        var logger = GetLogger(className, methodName);
        logger.Warning(message);
    }

    public static void Error(string className, string message, Exception? exception = null, [CallerMemberName] string methodName = "")
    {
        var logger = GetLogger(className, methodName);
        if (exception is not null)
        {
            logger.Error(exception, message);
        }
        else
        {
            logger.Error(message);
        }
    }

    public static void Fatal(string className, string message, Exception? exception = null, [CallerMemberName] string methodName = "")
    {
        var logger = GetLogger(className, methodName);
        if (exception is not null)
        {
            logger.Fatal(exception, message);
        }
        else
        {
            logger.Fatal(message);
        }
#if DEBUG
        Debugger.Break();
#endif
    }

    private static ILogger GetLogger(string className, [CallerMemberName] string methodName = "")
    {
        var classNameWithMethod = CheckClassAndMessageAndReturnFullClassWithMethod(className, methodName);

        return Log.ForContext(SourceContext, classNameWithMethod);
    }

    private static string CheckClassAndMessageAndReturnFullClassWithMethod(string className, string methodName)
    {
        if (string.IsNullOrWhiteSpace(className) && string.IsNullOrWhiteSpace(methodName))
        {
            return Constants.MathEditor;
        }
        else
        {
            return $"{className}.{methodName}";
        }
    }

    public static void Close()
    {
        if (!_initialized) return;

        Log.CloseAndFlush();
    }
}
