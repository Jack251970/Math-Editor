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
        var rowsCount = rows.Count;
        for (var i = 0; i < rowsCount; i++)
        {
            var row = rows[i];
            for (var j = 0; j < row.Length; j++)
            {
                foreach (var c in ConvertToLatexSymbol(row[j], false))
                {
                    escaped.Append(c);
                }
            }
            if (i < rowsCount - 1)
            {
                foreach (var c in RowSeparator)
                {
                    escaped.Append(c);
                }
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
            '}' => convertWrapper ? ['\\', '}'] : ['}'],
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

    private static readonly char[] BeginMatrix = ['\\', 'b', 'e', 'g', 'i', 'n', '{', 'a', 'r', 'r', 'a', 'y', '}', '{', '*', '{', '2', '0', '}', '{', 'c', '}', '}'];
    private static char[] EndMatrix => EndArray;
    private static char[] MatrixColumnSeparator => ['&'];
    private static char[] MatrixRowSeparator => RowSeparator;
    public static StringBuilder? ToMatrix(int rows, int columns, List<StringBuilder> matrix)
    {
        if (matrix.Count == 0 || rows * columns != matrix.Count)
        {
            return null;
        }
        var sb = new StringBuilder();
        foreach (var c in BeginMatrix)
        {
            sb.Append(c);
        }
        sb.Append('\n');
        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < columns; j++)
            {
                var index = i * columns + j;
                var cell = matrix[index];
                for (var k = 0; k < cell.Length; k++)
                {
                    sb.Append(cell[k]);
                }
                if (j < columns - 1)
                {
                    foreach (var c in MatrixColumnSeparator)
                    {
                        sb.Append(c);
                    }
                }
            }
            if (i < rows - 1)
            {
                foreach (var c in MatrixRowSeparator)
                {
                    sb.Append(c);
                }
            }
            sb.Append('\n');
        }
        foreach (var c in EndMatrix)
        {
            sb.Append(c);
        }
        return sb;
    }
}
