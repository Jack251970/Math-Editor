using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Editor;

public static class EditorLogger
{
    private const string SourceContext = "SourceContext";

    public static void Initialize()
    {
        Environment.SetEnvironmentVariable("LOGGING_ROOT", DataLocation.VersionLogDirectory);
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }

    public static IHostBuilder ConfigureFBLogger(this IHostBuilder builder)
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
        Log.CloseAndFlush();
    }
}
