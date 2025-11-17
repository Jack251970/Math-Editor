using System;
using System.Collections.Generic;
using Velopack.Logging;

namespace Editor;

public class VelopackLogger : IVelopackLogger
{
    private static readonly string ClassName = nameof(VelopackLogger);

    private readonly List<LogEntry> _logEntries = [];

    public void Log(VelopackLogLevel logLevel, string? message, Exception? exception)
    {
        // Since we need to build Velopack before EditorLogger is initialized, we cache log entries here
        if (!EditorLogger.IsInitialized)
        {
            _logEntries.Add(new LogEntry
            {
                LogLevel = logLevel,
                Message = message,
                Exception = exception
            });
            return;
        }

        // Log cached entries
        if (_logEntries.Count > 0)
        {
            foreach (LogEntry logEntry in _logEntries)
            {
                LogToEditorLogger(logEntry.LogLevel, logEntry.Message, logEntry.Exception);
            }
            _logEntries.Clear();
        }

        // Log current entry
        LogToEditorLogger(logLevel, message, exception);
    }

    private static void LogToEditorLogger(VelopackLogLevel logLevel, string? message, Exception? exception)
    {
        switch (logLevel)
        {
            case VelopackLogLevel.Trace:
                EditorLogger.Verbose(ClassName, message ?? string.Empty);
                break;
            case VelopackLogLevel.Debug:
                EditorLogger.Debug(ClassName, message ?? string.Empty);
                break;
            case VelopackLogLevel.Information:
                EditorLogger.Info(ClassName, message ?? string.Empty);
                break;
            case VelopackLogLevel.Warning:
                EditorLogger.Warning(ClassName, message ?? string.Empty);
                break;
            case VelopackLogLevel.Error:
                EditorLogger.Error(ClassName, message ?? string.Empty, exception);
                break;
            case VelopackLogLevel.Critical:
                EditorLogger.Fatal(ClassName, message ?? string.Empty, exception);
                break;
        }
    }

    private class LogEntry
    {
        public VelopackLogLevel LogLevel { get; set; }
        public string? Message { get; set; }
        public Exception? Exception { get; set; }
    }
}
