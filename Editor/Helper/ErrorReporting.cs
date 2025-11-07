using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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
        // handle non-ui thread exceptions
        Report((Exception)e.ExceptionObject);
    }

    public static void DispatcherUnhandledException(Exception e)
    {
        // log ui thread exceptions but do not handle unobserved task exceptions on UI thread
        Report(e, true);
    }

    public static void TaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        // log exception but do not handle unobserved task exceptions on UI thread
        Report(e.Exception, true);
        // prevent application exit, so the user can copy the prompted error info
        e.SetObserved();
    }
}
