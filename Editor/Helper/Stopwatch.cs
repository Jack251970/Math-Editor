using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Editor;

public static class Stopwatch
{
    /// <summary>
    /// This stopwatch will appear only in Debug mode
    /// </summary>
    public static long Debug(string className, string message, Action action, [CallerMemberName] string methodName = "")
    {
        var stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();
        action();
        stopWatch.Stop();
        var milliseconds = stopWatch.ElapsedMilliseconds;
        EditorLogger.Debug(className, $"{message} <{milliseconds}ms>", methodName);
        return milliseconds;
    }

    /// <summary>
    /// This stopwatch will appear only in Debug mode
    /// </summary>
    public static async Task<long> DebugAsync(string className, string message, Func<Task> action, [CallerMemberName] string methodName = "")
    {
        var stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();
        await action();
        stopWatch.Stop();
        var milliseconds = stopWatch.ElapsedMilliseconds;
        EditorLogger.Debug(className, $"{message} <{milliseconds}ms>", methodName);
        return milliseconds;
    }

    public static long Info(string className, string message, Action action, [CallerMemberName] string methodName = "")
    {
        var stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();
        action();
        stopWatch.Stop();
        var milliseconds = stopWatch.ElapsedMilliseconds;
        EditorLogger.Info(className, $"{message} <{milliseconds}ms>", methodName);
        return milliseconds;
    }

    public static async Task<long> InfoAsync(string className, string message, Func<Task> action, [CallerMemberName] string methodName = "")
    {
        var stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();
        await action();
        stopWatch.Stop();
        var milliseconds = stopWatch.ElapsedMilliseconds;
        EditorLogger.Info(className, $"{message} <{milliseconds}ms>", methodName);
        return milliseconds;
    }
}
