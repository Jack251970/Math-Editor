using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Avalonia.Controls;

namespace Editor;

public partial class ReportWindow : Window
{
    public ReportWindow()
    {
        InitializeComponent();
    }

    public ReportWindow(Exception exception)
    {
        InitializeComponent();
        SetException(exception);
    }

    private void SetException(Exception exception)
    {
        var path = DataLocation.VersionLogDirectory;
        var directory = new DirectoryInfo(path);
        var log = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
        var sb = new StringBuilder();

        // Add a link to the issue tracker and instructions for uploading the log file
        sb.Append(Localize.ReportWindow_PleaseOpenIssue());
        sb.Append(' ');
        sb.AppendLine(Constants.IssuesUrl);
        sb.AppendLine();
        sb.AppendLine(Localize.ReportWindow_UploadLog(log.FullName));
        sb.AppendLine();
        sb.AppendLine(Localize.ReportWindow_CopyBelow());

        // Add content with runtime information and the exception details
        sb.AppendLine(ExceptionFormatter.RuntimeInfo());
        sb.AppendLine();
        sb.AppendLine($"Date: {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine("Exception:");
        sb.AppendLine(exception.ToString());

        ErrorTextBox.Text = sb.ToString();

        // Constants.IssuesUrl
    }
}
