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

    private static readonly char[] BeginArray = ToChars("\\begin{array}{l}");
    private static readonly char[] EndArray = ToChars("\\end{array}");
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
    /// Converts a string to its Latex representation.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="convertWrapper"></param>
    /// <returns></returns>
    public static StringBuilder? ConvertToLatexSymbol(string str, bool convertWrapper)
    {
        var sb = new StringBuilder();
        foreach (var c in str)
        {
            foreach (var latexChar in ConvertToLatexSymbol(c, convertWrapper))
            {
                sb.Append(latexChar);
            }
        }
        return sb;
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
            '\u2211' => ['\\', 's', 'u', 'm'], // ∑
            '\u220F' => ['\\', 'p', 'r', 'o', 'd'], // ∏
            '\u2210' => ['\\', 'c', 'o', 'p', 'r', 'o', 'd'], // ∐
            '\u22C2' => ['\\', 'b', 'i', 'g', 'c', 'a', 'p'], // ⋂
            '\u22C3' => ['\\', 'b', 'i', 'g', 'c', 'u', 'p'], // ⋃
            '\u222B' => ['\\', 'i', 'n', 't'], // ∫
            '\u222C' => ['\\', 'i', 'i', 'n', 't'], // ∬
            '\u222D' => ['\\', 'i', 'i', 'i', 'n', 't'], // ∭
            '\u222E' => ['\\', 'o', 'i', 'n', 't'], // ∮
            '\u222F' => ToChars("\\mathop{{\\int\\!\\!\\!\\!\\!\\int}\\mkern-21mu \\bigcirc}"), // ∯
            '\u2230' => ToChars("\\mathop{{\\int\\!\\!\\!\\!\\!\\int\\!\\!\\!\\!\\!\\int}\\mkern-31.2mu \\bigodot}"), // ∰
            '\u2232' => ToChars("\\mathop{\\int\\mkern-20.8mu \\circlearrowleft}"), // ∲
            '\u2233' => ToChars("\\mathop{\\int\\mkern-20.8mu \\circlearrowright}"), // ∳
            _ => [c],
        };
    }

    private static char[] ToChars(string str)
    {
        var chars = new char[str.Length];
        for (var i = 0; i < str.Length; i++)
        {
            chars[i] = str[i];
        }
        return chars;
    }

    /// <summary>
    /// \sqrt{insideEquation}
    /// </summary>
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

    /// <summary>
    /// \begin{array}{*{20}{c}}
    /// {cell}&{cell}&{cell}\\
    /// {cell}&{cell}&{cell}\\
    /// {cell}&{cell}&{cell}\\
    /// \end{array}
    /// </summary>
    private static readonly char[] BeginMatrix = ToChars("\\begin{array}{*{20}{c}}");
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

    /// <summary>
    /// {}^{insideEquation}
    /// </summary>
    private static readonly char[] LeftSuper = ['{', '}', '^'];
    public static StringBuilder? ToLeftSuper(StringBuilder? insideEquation)
    {
        var sb = new StringBuilder();
        foreach (var c in LeftSuper)
        {
            sb.Append(c);
        }
        if (insideEquation != null)
        {
            for (var i = 0; i < insideEquation.Length; i++)
            {
                sb.Append(insideEquation[i]);
            }
        }
        return sb;
    }

    /// <summary>
    /// ^{insideEquation}
    /// </summary>
    private static readonly char[] RightSuper = ['^'];
    public static StringBuilder? ToRightSuper(StringBuilder? insideEquation)
    {
        var sb = new StringBuilder();
        foreach (var c in RightSuper)
        {
            sb.Append(c);
        }
        if (insideEquation != null)
        {
            for (var i = 0; i < insideEquation.Length; i++)
            {
                sb.Append(insideEquation[i]);
            }
        }
        return sb;
    }

    /// <summary>
    /// {}_{insideEquation}
    /// </summary>
    private static readonly char[] LeftSub = ['{', '}', '_'];
    public static StringBuilder? ToLeftSub(StringBuilder? insideEquation)
    {
        var sb = new StringBuilder();
        foreach (var c in LeftSub)
        {
            sb.Append(c);
        }
        if (insideEquation != null)
        {
            for (var i = 0; i < insideEquation.Length; i++)
            {
                sb.Append(insideEquation[i]);
            }
        }
        return sb;
    }

    /// <summary>
    /// _{insideEquation}
    /// </summary>
    private static readonly char[] RightSub = ['_'];
    public static StringBuilder? ToRightSub(StringBuilder? insideEquation)
    {
        var sb = new StringBuilder();
        foreach (var c in RightSub)
        {
            sb.Append(c);
        }
        if (insideEquation != null)
        {
            for (var i = 0; i < insideEquation.Length; i++)
            {
                sb.Append(insideEquation[i]);
            }
        }
        return sb;
    }

    /// <summary>
    /// _{subEquation}^{superEquation}
    /// </summary>
    public static StringBuilder? ToLeftSuperSub(StringBuilder? superEquation, StringBuilder? subEquation)
    {
        var sb = new StringBuilder();
        foreach (var c in LeftSub)
        {
            sb.Append(c);
        }
        if (subEquation != null)
        {
            for (var i = 0; i < subEquation.Length; i++)
            {
                sb.Append(subEquation[i]);
            }
        }
        foreach (var c in LeftSuper)
        {
            sb.Append(c);
        }
        if (superEquation != null)
        {
            for (var i = 0; i < superEquation.Length; i++)
            {
                sb.Append(superEquation[i]);
            }
        }
        return sb;
    }

    /// <summary>
    /// {}_{subEquation}^{superEquation}
    /// </summary>
    public static StringBuilder? ToRightSuperSub(StringBuilder? superEquation, StringBuilder? subEquation)
    {
        var sb = new StringBuilder();
        foreach (var c in RightSub)
        {
            sb.Append(c);
        }
        if (subEquation != null)
        {
            for (var i = 0; i < subEquation.Length; i++)
            {
                sb.Append(subEquation[i]);
            }
        }
        foreach (var c in RightSuper)
        {
            sb.Append(c);
        }
        if (superEquation != null)
        {
            for (var i = 0; i < superEquation.Length; i++)
            {
                sb.Append(superEquation[i]);
            }
        }
        return sb;
    }

    /// <summary>
    /// \sqrt[nthRootEquation]{insideEquation}
    /// </summary>
    private static readonly char[] NRoot1 = ['\\', 's', 'q', 'r', 't', '['];
    private static readonly char[] NRoot2 = [']'];
    public static StringBuilder? ToNRoot(StringBuilder? insideEquation, StringBuilder? nthRootEquation)
    {
        var sb = new StringBuilder();
        foreach (var c in NRoot1)
        {
            sb.Append(c);
        }
        if (nthRootEquation != null)
        {
            for (var i = 0; i < nthRootEquation.Length; i++)
            {
                sb.Append(nthRootEquation[i]);
            }
        }
        foreach (var c in NRoot2)
        {
            sb.Append(c);
        }
        if (insideEquation != null)
        {
            // No need to wrap it with {} since insideEquation is already wrapped
            for (var i = 0; i < insideEquation.Length; i++)
            {
                sb.Append(insideEquation[i]);
            }
        }
        return sb;
    }

    /// <summary>
    /// {sign} {mainEquation}
    /// </summary>
    public static StringBuilder? ToSignSimple(StringBuilder? sign, StringBuilder? mainEquation)
    {
        var sb = new StringBuilder();
        if (sign != null)
        {
            for (var i = 0; i < sign.Length; i++)
            {
                sb.Append(sign[i]);
            }
        }
        sb.Append(WhiteSpace);
        if (mainEquation != null)
        {
            for (var i = 0; i < mainEquation.Length; i++)
            {
                sb.Append(mainEquation[i]);
            }
        }
        return sb;
    }
}
