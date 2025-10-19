using System.Collections.Generic;
using System.Text;

namespace Editor;

public static class LatexConverter
{
    /// <summary>
    /// Escapes text with { and }.
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="start"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static StringBuilder? EscapeText(StringBuilder sb, int start, int count)
    {
        if (count <= 0 || start < 0 || start + count > sb.Length)
        {
            return null;
        }

        var escaped = new StringBuilder();
        escaped.Append('{');
        for (var i = start; i < start + count; i++)
        {
            foreach (var c in ConvertToLatexSymbol(sb[i], true))
            {
                escaped.Append(c);
            }
        }
        escaped.Append('}');
        return escaped;
    }

    /// <summary>
    /// Escapes a row of text parts with { and }.
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    public static StringBuilder? EscapeRowText(List<StringBuilder> row)
    {
        if (row.Count == 0)
        {
            return null;
        }
        else if (row.Count == 1)
        {
            return row[0];
        }

        var escaped = new StringBuilder();
        escaped.Append('{');
        foreach (var sb in row)
        {
            for (var i = 0; i < sb.Length; i++)
            {
                foreach (var c in ConvertToLatexSymbol(sb[i], false))
                {
                    escaped.Append(c);
                }
            }
        }
        escaped.Append('}');
        return escaped;
    }

    private static readonly char[] BeginArray = ['\\', 'b', 'e', 'g', 'i', 'n', '{', 'a', 'r', 'r', 'a', 'y', '}', '{', 'l', '}'];
    private static readonly char[] EndArray = ['\\', 'e', 'n', 'd', '{', 'a', 'r', 'r', 'a', 'y', '}'];
    private static readonly char[] RowSeparator = ['\\', '\\'];

    /// <summary>
    /// Escapes a row of text parts with \begin{array}{l} and \end{array}.
    /// For each row in rows, appends the escaped row followed by \\.
    /// </summary>
    /// <param name="rows"></param>
    /// <returns></returns>
    public static StringBuilder? EscapeRows(List<StringBuilder> rows)
    {
        if (rows.Count == 0)
        {
            return null;
        }
        else if (rows.Count == 1)
        {
            return rows[0];
        }

        var escaped = new StringBuilder();
        foreach (var c in BeginArray)
        {
            escaped.Append(c);
        }
        escaped.Append('\n');
        foreach (var row in rows)
        {
            for (var i = 0; i < row.Length; i++)
            {
                foreach (var c in ConvertToLatexSymbol(row[i], false))
                {
                    escaped.Append(c);
                }
            }
            foreach (var c in RowSeparator)
            {
                escaped.Append(c);
            }
            escaped.Append('\n');
        }
        foreach (var c in EndArray)
        {
            escaped.Append(c);
        }
        return escaped;
    }

    /// <summary>
    /// Converts a character to its Latex representation.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="convertWrapper"></param>
    /// <returns></returns>
    private static char[] ConvertToLatexSymbol(char c, bool convertWrapper)
    {
        return c switch
        {
            // TODO: Handle more special characters for Latex
            '{' => convertWrapper ? ['\\', '{'] : ['{'],
            '}' => convertWrapper?['\\', '}'] : ['}'],
            _ => [c],
        };
    }

    private const char WhiteSpace = ' ';

    private static readonly char[] SquareRoot = ['\\', 's', 'q', 'r', 't'];
    public static StringBuilder? ToSquareRoot(StringBuilder? insideEquation)
    {
        var sb = new StringBuilder();
        foreach (var c in SquareRoot)
        {
            sb.Append(c);
        }
        sb.Append(WhiteSpace);
        if (insideEquation != null)
        {
            for (var i = 0; i < insideEquation.Length; i++)
            {
                sb.Append(insideEquation[i]);
            }
        }
        return sb;
    }
}
