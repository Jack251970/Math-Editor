using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace Editor;

public partial class ReportWindow : Window
{
    public ReportWindow(Exception exception)
    {
        InitializeComponent();
        SetException(exception);
    }

    private void SetException(Exception exception)
    {
        // Remove the default margin from the first block to avoid unnecessary indentation
        ErrorTextBox.Document.Blocks.FirstBlock.Margin = new Thickness(0);

        // Add a link to the issue tracker and instructions for uploading the log file
        var path = DataLocation.VersionLogDirectory;
        var directory = new DirectoryInfo(path);
        var log = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
        var paragraph = Hyperlink(Localize.ReportWindow_PleaseOpenIssue(), Constants.IssuesUrl);
        paragraph.Inlines.Add(Localize.ReportWindow_UploadLog(log.FullName));
        paragraph.Inlines.Add("\n");
        paragraph.Inlines.Add(Localize.ReportWindow_CopyBelow());
        ErrorTextBox.Document.Blocks.Add(paragraph);

        // Add content with runtime information and the exception details
        var content = new StringBuilder();
        content.AppendLine(ExceptionFormatter.RuntimeInfo());
        content.AppendLine();
        content.AppendLine($"Date: {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");
        content.AppendLine("Exception:");
        content.AppendLine(exception.ToString());
        paragraph = new Paragraph();
        paragraph.Inlines.Add(content.ToString());
        ErrorTextBox.Document.Blocks.Add(paragraph);
    }

    private static Paragraph Hyperlink(string textBeforeUrl, string url)
    {
        var paragraph = new Paragraph
        {
            Margin = new Thickness(0)
        };

        Hyperlink? link = null;
        try
        {
            var uri = new Uri(url);

            link = new Hyperlink
            {
                IsEnabled = true
            };
            link.Inlines.Add(url);
            link.NavigateUri = uri;
            link.Click += (s, e) => BrowserHelper.Open(url);
        }
        catch (Exception)
        {
            // Leave link as null if the URL is invalid
        }

        paragraph.Inlines.Add(textBeforeUrl);
        paragraph.Inlines.Add(" ");
        if (link is null)
        {
            // Add the URL as plain text if it is invalid
            paragraph.Inlines.Add(url);
        }
        else
        {
            // Add the hyperlink if it is valid
            paragraph.Inlines.Add(link);
        }
        paragraph.Inlines.Add("\n");

        return paragraph;
    }
}
