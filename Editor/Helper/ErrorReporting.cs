using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Editor;

public static class ErrorReporting
{
    private static readonly string ClassName = nameof(ErrorReporting);

    private static void Report(Exception e, bool silent = false, [CallerMemberName] string methodName = "UnHandledException")
    {
        EditorLogger.Fatal(ClassName, ExceptionFormatter.FormatException(e), e, methodName);
        if (silent) return;
        var reportWindow = new ReportWindow(e);
        reportWindow.Show();
    }

    public static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // Handle non-ui thread exceptions
        Report((Exception)e.ExceptionObject);
    }

    public static void DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // Handle ui thread exceptions
        Report(e.Exception);
        // Prevent application exit, so the user can copy prompted error info
        e.Handled = true;
    }

    public static void DispatcherUnhandledExceptionFilter(object sender, DispatcherUnhandledExceptionFilterEventArgs e)
    {
#if DEBUG
        // Log and break the debugger without showing the report window
        Report(e.Exception, true);
        e.RequestCatch = false;
#else
        e.RequestCatch = true;
#endif
    }

    public static void TaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        // Log exception but do not handle unobserved task exceptions on UI thread
        Report(e.Exception, true);
        // Prevent application exit, so the user can copy the prompted error info
        e.SetObserved();
    }
}
