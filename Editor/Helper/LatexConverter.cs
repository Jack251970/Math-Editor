using System;
using System.Collections.Generic;
using System.Text;
using iNKORE.UI.WPF.Modern.Controls;

namespace Editor;

// TODO: Convert to non-static and use App.LatexConverter?
public static class LatexConverter
{
    private const char WhiteSpace = ' ';

    /// <summary>
    /// Convert to latex symbol.
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="start"></param>
    /// <param name="count"></param>
    /// <param name="convertWrapper"></param>
    /// <param name="wrap"></param>
    /// <returns></returns>
    public static StringBuilder? ConvertToLatexSymbol(StringBuilder sb, int start, int count, bool convertWrapper, 
        bool wrap = false)
    {
        if (count <= 0 || start < 0 || start + count > sb.Length)
        {
            return null;
        }

        var escaped = new StringBuilder();
        if (wrap) escaped.Append(LeftBrace);
        for (var i = start; i < start + count; i++)
        {
            escaped.Append(ConvertToLatexSymbol(sb[i], convertWrapper));
        }
        if (wrap) escaped.Append(RightBrace);
        return escaped;
    }

    /// <summary>
    /// Convert to latex symbol.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="convertWrapper"></param>
    /// <param name="wrap"></param>
    /// <returns></returns>
    public static StringBuilder? ConvertToLatexSymbol(List<StringBuilder> row, bool convertWrapper, bool wrap = false)
    {
        if (row.Count == 0) return null;
        else if (row.Count == 1) return row[0];

        var escaped = new StringBuilder();
        if (wrap) escaped.Append(LeftBrace);
        foreach (var sb in row)
        {
            for (var i = 0; i < sb.Length; i++)
            {
                escaped.Append(ConvertToLatexSymbol(sb[i], convertWrapper));
            }
        }
        if (wrap) escaped.Append(RightBrace);
        return escaped;
    }

    private static readonly char[] BeginArray = ToChars("\\begin{array}{l}");
    private static readonly char[] EndArray = ToChars("\\end{array}");
    private static readonly char[] RowSeparator = ToChars("\\\\");

    /// <summary>
    /// Escapes a row of text parts with \begin{array}{l} and \end{array}.
    /// For each row in rows, appends the escaped row followed by \\.
    /// </summary>
    /// <param name="rows"></param>
    /// <returns></returns>
    // TODO: Support \begin{gathered} \hfill \\ \hfill \\ \end{gathered}??
    // \[\begin{gathered}
    //  1111 \hfill \\
    //  2222 \hfill \\ 
    // \end{gathered
    // } \]
    // \[\begin{array}{ l}
    // 1111\\
    // 2222
    // \end{array}\]
    public static StringBuilder? EscapeRows(List<StringBuilder> rows)
    {
        if (rows.Count == 0) return null;
        else if (rows.Count == 1) return rows[0];

        var escaped = new StringBuilder();
        escaped.Append(BeginArray).Append('\n');
        var rowsCount = rows.Count;
        for (var i = 0; i < rowsCount; i++)
        {
            var row = rows[i];
            for (var j = 0; j < row.Length; j++)
            {
                escaped.Append(ConvertToLatexSymbol(row[j], false));
            }
            if (i < rowsCount - 1)
            {
                escaped.Append(RowSeparator);
            }
            escaped.Append('\n');
        }
        escaped.Append(EndArray);
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
            sb.Append(ConvertToLatexSymbol(c, convertWrapper));
        }
        return sb;
    }

    /// <summary>
    /// Converts a character to its Latex representation.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="convertWrapper"></param>
    /// <returns></returns>
    private static readonly char[] Sum = ToChars("\\sum");
    private static readonly char[] Prod = ToChars("\\prod");
    private static readonly char[] CoProd = ToChars("\\coprod");
    private static readonly char[] BigCap = ToChars("\\bigcap");
    private static readonly char[] BigCup = ToChars("\\bigcup");
    private static readonly char[] Int = ToChars("\\int");
    private static readonly char[] IInt = ToChars("\\iint");
    private static readonly char[] IIInt = ToChars("\\iiint");
    private static readonly char[] OInt = ToChars("\\oint");
    private static readonly char[] IntBigCirc = ToChars("\\mathop{{\\int\\!\\!\\!\\!\\!\\int}\\mkern-21mu \\bigcirc}");
    private static readonly char[] IntBigOdot = ToChars("\\mathop{{\\int\\!\\!\\!\\!\\!\\int\\!\\!\\!\\!\\!\\int}\\mkern-31.2mu \\bigodot}");
    private static readonly char[] IntCircleArrowLeft = ToChars("\\mathop{\\int\\mkern-20.8mu \\circlearrowleft}");
    private static readonly char[] IntCircleArrowRight = ToChars("\\mathop{\\int\\mkern-20.8mu \\circlearrowright}");
    private static readonly char[] EscapedLeftBrace = ToChars("\\{");
    private static readonly char[] EscapedRightBrace = ToChars("\\}");
    private static readonly char[] LeftBrace = ToChars("{");
    private static readonly char[] RightBrace = ToChars("}");
    private static char[] ConvertToLatexSymbol(char c, bool convertWrapper)
    {
        return c switch
        {
            // TODO: Handle more special characters for Latex
            '{' => convertWrapper ? EscapedLeftBrace : LeftBrace,
            '}' => convertWrapper ? EscapedRightBrace : RightBrace,
            '\u2211' => Sum, // ∑
            '\u220F' => Prod, // ∏
            '\u2210' => CoProd, // ∐
            '\u22C2' => BigCap, // ⋂
            '\u22C3' => BigCup, // ⋃
            '\u222B' => Int, // ∫
            '\u222C' => IInt, // ∬
            '\u222D' => IIInt, // ∭
            '\u222E' => OInt, // ∮
            '\u222F' => IntBigCirc, // ∯
            '\u2230' => IntBigOdot, // ∰
            '\u2232' => IntCircleArrowLeft, // ∲
            '\u2233' => IntCircleArrowRight, // ∳
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

    private static StringBuilder AppendWithWrapper(this StringBuilder sb, StringBuilder? equ)
    {
        return sb.Append(LeftBrace).Append(equ).Append(RightBrace);
    }

    private static readonly char[] SquareRoot = ToChars("\\sqrt");
    public static StringBuilder? ToSquareRoot(StringBuilder? insideEquation)
    {
        /// \sqrt{insideEquation}
        var sb = new StringBuilder();
        sb.Append(SquareRoot).Append(WhiteSpace).AppendWithWrapper(insideEquation);
        return sb;
    }

    private static readonly char[] BeginMatrix = ToChars("\\begin{array}{*{20}{c}}");
    private static char[] EndMatrix => EndArray;
    private static char[] MatrixColumnSeparator => ToChars("&");
    private static char[] MatrixRowSeparator => RowSeparator;
    public static StringBuilder? ToMatrix(int rows, int columns, List<StringBuilder> matrix)
    {
        /// \begin{array}{*{20}{c}}
        /// {cell}&{cell}&{cell}\\
        /// {cell}&{cell}&{cell}\\
        /// {cell}&{cell}&{cell}\\
        /// \end{array}
        if (matrix.Count == 0 || rows * columns != matrix.Count) return null;

        var sb = new StringBuilder();
        sb.Append(BeginMatrix).Append('\n');
        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < columns; j++)
            {
                var index = i * columns + j;
                var cell = matrix[index];
                sb.Append(cell);
                if (j < columns - 1)
                {
                    sb.Append(MatrixColumnSeparator);
                }
            }
            if (i < rows - 1)
            {
                sb.Append(MatrixRowSeparator);
            }
            sb.Append('\n');
        }
        sb.Append(EndMatrix);
        return sb;
    }

    private static readonly char[] LeftSuper = ToChars("{}^");
    private static readonly char[] RightSuper = ToChars("^");
    public static StringBuilder? ToSuper(Position position, StringBuilder? insideEquation)
    {
        var sb = new StringBuilder();
        return position switch
        {
            /// {}^{insideEquation}
            Position.Left => sb.Append(LeftSuper).AppendWithWrapper(insideEquation),
            /// ^{insideEquation}
            Position.Right => sb.Append(RightSuper).AppendWithWrapper(insideEquation),
            _ => throw new InvalidOperationException($"Invalid position for Super: {position}"),
        };
    }

    private static readonly char[] LeftSub = ToChars("{}_");
    private static readonly char[] RightSub = ToChars("_");
    public static StringBuilder? ToSub(Position position, StringBuilder? insideEquation)
    {
        var sb = new StringBuilder();
        return position switch
        {
            /// {}_{insideEquation}
            Position.Left => sb.Append(LeftSub).AppendWithWrapper(insideEquation),
            /// _{insideEquation}
            Position.Right => sb.Append(RightSub).AppendWithWrapper(insideEquation),
            _ => throw new InvalidOperationException($"Invalid position for Sub: {position}"),
        };
    }

    public static StringBuilder? ToSuperSub(Position position, StringBuilder? superEquation, StringBuilder? subEquation)
    {
        var sb = new StringBuilder();
        return position switch
        {
            /// _{subEquation}^{superEquation}
            Position.Left => sb.Append(LeftSub).AppendWithWrapper(subEquation).Append(LeftSuper).AppendWithWrapper(superEquation),
            /// {}_{subEquation}^{superEquation}
            Position.Right => sb.Append(RightSub).AppendWithWrapper(subEquation).Append(RightSuper).AppendWithWrapper(superEquation),
            _ => throw new InvalidOperationException($"Invalid positions for SuperSub: {position}"),
        };
    }

    private static readonly char[] NRoot1 = ToChars("\\sqrt[");
    private static readonly char[] NRoot2 = ToChars("]");
    public static StringBuilder? ToNRoot(StringBuilder? insideEquation, StringBuilder? nthRootEquation)
    {
        /// \sqrt[nthRootEquation]{insideEquation}
        var sb = new StringBuilder();
        return sb.Append(NRoot1).Append(nthRootEquation).Append(NRoot2).AppendWithWrapper(insideEquation);
    }

    public static StringBuilder? ToSignSimple(StringBuilder? sign, StringBuilder? mainEquation)
    {
        /// sign {mainEquation}
        var sb = new StringBuilder();
        return sb.Append(sign).Append(WhiteSpace).AppendWithWrapper(mainEquation);
    }

    private static readonly char[] Limits = ToChars("\\limits_");
    public static StringBuilder? ToSignBottom(StringBuilder? sign, StringBuilder? mainEquation, StringBuilder? bottomEquation)
    {
        /// sign\limits_{bottomEquation} {mainEquation}
        var sb = new StringBuilder();
        return sb.Append(sign).Append(Limits).AppendWithWrapper(bottomEquation).Append(WhiteSpace)
            .AppendWithWrapper(mainEquation);
    }

    public static StringBuilder? ToSignBottomTop(StringBuilder? sign, StringBuilder? mainEquation, StringBuilder? topEquation, StringBuilder? bottomEquation)
    {
        /// sign\limits_{bottomEquation}^{topEquation} {mainEquation}
        var sb = new StringBuilder();
        return sb.Append(sign).Append(Limits).AppendWithWrapper(bottomEquation).Append(RightSuper)
            .AppendWithWrapper(topEquation).Append(WhiteSpace).AppendWithWrapper(mainEquation);
    }

    private static readonly char[] NoLimits = ToChars("\\nolimits_");
    public static StringBuilder? ToSignSub(StringBuilder? sign, StringBuilder? mainEquation, StringBuilder? subEquation)
    {
        /// sign\nolimits_{subEquation} {mainEquation}
        var sb = new StringBuilder();
        return sb.Append(sign).Append(NoLimits).AppendWithWrapper(subEquation).Append(WhiteSpace)
            .AppendWithWrapper(mainEquation);
    }

    public static StringBuilder? ToSignSubSuper(StringBuilder? sign, StringBuilder? mainEquation, StringBuilder? superEquation, StringBuilder? subEquation)
    {
        /// {sign}\nolimits_{subEquation}^{superEquation} {mainEquation}
        var sb = new StringBuilder();
        return sb.Append(sign).Append(NoLimits).AppendWithWrapper(subEquation).Append(RightSuper)
            .AppendWithWrapper(superEquation).Append(WhiteSpace).AppendWithWrapper(mainEquation);
    }

    private static readonly char[] Boxed = ToChars("\\boxed");
    private static readonly char[] LeftTopBox1 = ToChars("\\left| \\!{\\overline {\\, ");
    private static readonly char[] LeftTopBox2 = ToChars(" \\,}} \\right.");
    private static readonly char[] RightTopBox1 = ToChars("\\left. {\\overline {\\, ");
    private static readonly char[] RightTopBox2 = ToChars(" \\,}}\\! \\right|");
    private static readonly char[] LeftBottomBox1 = ToChars("\\left| \\!{\\underline {\\, ");
    private static readonly char[] LeftBottomBox2 = ToChars(" \\,}} \\right.");
    private static readonly char[] RightBottomBox1 = ToChars("\\left. {\\underline {\\, ");
    private static readonly char[] RightBottomBox2 = ToChars(" \\,}}\\! \\right|");
    public static StringBuilder? ToBox(BoxType type, StringBuilder? insideEquation)
    {
        var sb = new StringBuilder();
        return type switch
        {
            /// \boxed{insideEquation}
            BoxType.All => sb.Append(Boxed).AppendWithWrapper(insideEquation),
            /// \left| \!{\overline {\, {insideEquation} \,}} \right.
            BoxType.LeftTop => sb.Append(LeftTopBox1).AppendWithWrapper(insideEquation).Append(LeftTopBox2),
            /// \left. {\overline {\, {insideEquation} \,}}\! \right|
            BoxType.RightTop => sb.Append(RightTopBox1).AppendWithWrapper(insideEquation).Append(RightTopBox2),
            /// \left| \!{\underline {\, {insideEquation} \,}} \right.
            BoxType.LeftBottom => sb.Append(LeftBottomBox1).AppendWithWrapper(insideEquation).Append(LeftBottomBox2),
            /// \left. {\underline {\, {insideEquation} \,}}\! \right|
            BoxType.RightBottom => sb.Append(RightBottomBox1).AppendWithWrapper(insideEquation).Append(RightBottomBox2),
            _ => throw new InvalidOperationException($"Invalid BoxType: {type}"),
        };
    }

    private static readonly char[] EmptyWrapper = ToChars("{}");
    private static readonly char[] LeftArrow = ToChars("\\xleftarrow");
    private static readonly char[] RightArrow = ToChars("\\xrightarrow");
    private static readonly char[] OverSet = ToChars("\\overset");
    private static readonly char[] UnderSet = ToChars("\\underset");
    private static readonly char[] LongLeftRightArrow = ToChars("\\longleftrightarrow");
    private static readonly char[] LeftRightArrows = ToChars("\\leftrightarrows");
    public static StringBuilder? ToArrow(ArrowType type, Position position, StringBuilder? rowContainer1, StringBuilder? rowContainer2)
    {
        var sb = new StringBuilder();
        switch (type)
        {
            case ArrowType.LeftArrow:
                /// \xleftarrow{1}
                /// \xleftarrow[1]{}
                /// \xleftarrow[2]{1}
                switch (position)
                {
                    case Position.Top:
                        sb.Append(LeftArrow);
                        sb.Append(rowContainer1);
                        break;
                    case Position.Bottom:
                        sb.Append(LeftArrow);
                        sb.Append('[');
                        sb.Append(rowContainer1);
                        sb.Append(']');
                        sb.Append(EmptyWrapper);
                        break;
                    case Position.BottomAndTop:
                        sb.Append(LeftArrow);
                        sb.Append('[');
                        sb.Append(rowContainer2);
                        sb.Append(']');
                        sb.Append(rowContainer1);
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported position for LaTeX conversion: {position}");
                }
                break;
            case ArrowType.RightArrow:
                /// \xrightarrow{1}
                /// \xrightarrow[1]{}
                /// \xrightarrow[2]{1}
                switch (position)
                {
                    case Position.Top:
                        sb.Append(RightArrow);
                        sb.Append(rowContainer1);
                        break;
                    case Position.Bottom:
                        sb.Append(RightArrow);
                        sb.Append('[');
                        sb.Append(rowContainer1);
                        sb.Append(']');
                        sb.Append(EmptyWrapper);
                        break;
                    case Position.BottomAndTop:
                        sb.Append(RightArrow);
                        sb.Append('[');
                        sb.Append(rowContainer2);
                        sb.Append(']');
                        sb.Append(rowContainer1);
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported position for LaTeX conversion: {position}");
                }
                break;
            case ArrowType.DoubleArrow:
                /// \overset 1 \longleftrightarrow
                /// \underset 1 \longleftrightarrow
                /// \underset{2}{\overset{1} {\longleftrightarrow}}
                switch (position)
                {
                    case Position.Top:
                        sb.Append(OverSet);
                        sb.Append(WhiteSpace);
                        sb.Append(rowContainer1);
                        sb.Append(WhiteSpace);
                        sb.Append(LongLeftRightArrow);
                        break;
                    case Position.Bottom:
                        sb.Append(UnderSet);
                        sb.Append(WhiteSpace);
                        sb.Append(rowContainer1);
                        sb.Append(WhiteSpace);
                        sb.Append(LongLeftRightArrow);
                        break;
                    case Position.BottomAndTop:
                        sb.Append(UnderSet);
                        sb.Append(rowContainer2);
                        sb.Append('{');
                        sb.Append(OverSet);
                        sb.Append(rowContainer1);
                        sb.Append(WhiteSpace);
                        sb.Append('{');
                        sb.Append(LongLeftRightArrow);
                        sb.Append('}');
                        sb.Append('}');
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported position for LaTeX conversion: {position}");
                }
                break;
            case ArrowType.RightLeftArrow:
                /// \overset 1 \leftrightarrows
                /// \underset 1 \leftrightarrows
                /// \underset{2}{\overset{1} {\longleftrightarrow}}
                switch (position)
                {
                    case Position.Top:
                        sb.Append(OverSet);
                        sb.Append(WhiteSpace);
                        sb.Append(rowContainer1);
                        sb.Append(WhiteSpace);
                        sb.Append(LeftRightArrows);
                        break;
                    case Position.Bottom:
                        sb.Append(UnderSet);
                        sb.Append(WhiteSpace);
                        sb.Append(rowContainer1);
                        sb.Append(WhiteSpace);
                        sb.Append(LeftRightArrows);
                        break;
                    case Position.BottomAndTop:
                        sb.Append(UnderSet);
                        sb.Append(rowContainer2);
                        sb.Append('{');
                        sb.Append(OverSet);
                        sb.Append(rowContainer1);
                        sb.Append(WhiteSpace);
                        sb.Append('{');
                        sb.Append(LongLeftRightArrow);
                        sb.Append('}');
                        sb.Append('}');
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported position for LaTeX conversion: {position}");
                }
                break;
            // TODO: Support editting these in the settings.
            case ArrowType.RightSmallLeftArrow:
                switch (position)
                {
                    case Position.Top:
                        MessageBox.Show("Translation Error", "No translation available for Large over small arrow with upper text slot.\nPlease add a translation for it in the settings.");
                        return null;
                    case Position.Bottom:
                        MessageBox.Show("Translation Error", "No translation available for Large over small arrow with lower text slot.\nPlease add a translation for it in the settings.");
                        return null;
                    case Position.BottomAndTop:
                        MessageBox.Show("Translation Error", "No translation available for Large over small arrow with upper and lower text slots.\nPlease add a translation for it in the settings.");
                        return null;
                    default:
                        throw new InvalidOperationException($"Unsupported position for LaTeX conversion: {position}");
                }
            case ArrowType.SmallRightLeftArrow:
                switch (position)
                {
                    case Position.Top:
                        MessageBox.Show("Translation Error", "No translation available for Small over large arrow with upper text slot.\nPlease add a translation for it in the settings.");
                        return null;
                    case Position.Bottom:
                        MessageBox.Show("Translation Error", "No translation available for Small over large arrow with lower text slot.\nPlease add a translation for it in the settings.");
                        return null;
                    case Position.BottomAndTop:
                        MessageBox.Show("Translation Error", "No translation available for Small over large arrow with upper and lower text slots.\nPlease add a translation for it in the settings.");
                        return null;
                    default:
                        throw new InvalidOperationException($"Unsupported position for LaTeX conversion: {position}");
                }
            case ArrowType.RightLeftHarpoon:
                switch (position)
                {
                    case Position.Top:
                        MessageBox.Show("Translation Error", "No translation available for Harpoons with upper text slot.\nPlease add a translation for it in the settings.");
                        return null;
                    case Position.Bottom:
                        MessageBox.Show("Translation Error", "No translation available for Harpoons with lower text slot.\nPlease add a translation for it in the settings.");
                        return null;
                    case Position.BottomAndTop:
                        MessageBox.Show("Translation Error", "No translation available for Harpoons with upper and lower text slots.\nPlease add a translation for it in the settings.");
                        return null;
                    default:
                        throw new InvalidOperationException($"Unsupported position for LaTeX conversion: {position}");
                }
            case ArrowType.RightSmallLeftHarpoon:
                switch (position)
                {
                    case Position.Top:
                        MessageBox.Show("Translation Error", "No translation available for Large over small harpoon with upper text slot.\nPlease add a translation for it in the settings.");
                        return null;
                    case Position.Bottom:
                        MessageBox.Show("Translation Error", "No translation available for Large over small harpoon with lower text slot.\nPlease add a translation for it in the settings.");
                        return null;
                    case Position.BottomAndTop:
                        MessageBox.Show("Translation Error", "No translation available for Large over small harpoon with upper and lower text slots.\nPlease add a translation for it in the settings.");
                        return null;
                    default:
                        throw new InvalidOperationException($"Unsupported position for LaTeX conversion: {position}");
                }
            case ArrowType.SmallRightLeftHarpoon:
                switch (position)
                {
                    case Position.Top:
                        MessageBox.Show("Translation Error", "No translation available for Small over large harpoon with upper text slot.\nPlease add a translation for it in the settings.");
                        return null;
                    case Position.Bottom:
                        MessageBox.Show("Translation Error", "No translation available for Small over large harpoon with lower text slot.\nPlease add a translation for it in the settings.");
                        return null;
                    case Position.BottomAndTop:
                        MessageBox.Show("Translation Error", "No translation available for Small over large harpoon with upper and lower text slots.\nPlease add a translation for it in the settings.");
                        return null;
                    default:
                        throw new InvalidOperationException($"Unsupported position for LaTeX conversion: {position}");
                }
            default:
                throw new InvalidOperationException($"Unsupported arrow type for LaTeX conversion: {type}");
        }
        return sb;
    }
}
