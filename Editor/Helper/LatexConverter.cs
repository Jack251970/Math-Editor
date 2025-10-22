using System;
using System.Collections.Generic;
using System.Text;
using iNKORE.UI.WPF.Modern.Controls;

namespace Editor;

// TODO: Convert to non-static and use App.LatexConverter?
public static class LatexConverter
{
    #region Helpers

    private const char WhiteSpace = ' ';

    private static char[] ToChars(string str)
    {
        var chars = new char[str.Length];
        for (var i = 0; i < str.Length; i++)
        {
            chars[i] = str[i];
        }
        return chars;
    }

    private static StringBuilder Append(char c)
    {
        var sb = new StringBuilder();
        return sb.Append(c);
    }

    private static StringBuilder Append(char[] chars)
    {
        var sb = new StringBuilder();
        return sb.Append(chars);
    }

    private static StringBuilder Append(StringBuilder? sb)
    {
        return sb ?? new StringBuilder();
    }

    private static StringBuilder AppendWithWrapper(this StringBuilder sb, StringBuilder? equ)
    {
        return sb.Append(LeftBrace).Append(equ).Append(RightBrace);
    }

    #endregion

    #region Convert to Latex Symbol

    /// <summary>
    /// Convert to latex symbol.
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="start"></param>
    /// <param name="count"></param>
    /// <param name="convertWrapper"></param>
    /// <returns></returns>
    public static StringBuilder? ConvertToLatexSymbol(StringBuilder sb, int start, int count, bool convertWrapper)
    {
        if (count <= 0 || start < 0 || start + count > sb.Length) return null;

        var escaped = new StringBuilder();
        var onlyOneChar = count == 1;
        if (!onlyOneChar) escaped.Append(LeftBrace);
        for (var i = start; i < start + count; i++)
        {
            escaped.Append(ConvertToLatexSymbol(sb[i], convertWrapper));
        }
        if (!onlyOneChar) escaped.Append(RightBrace);
        return escaped;
    }

    /// <summary>
    /// Convert to latex symbol.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="convertWrapper"></param>
    /// <returns></returns>
    public static StringBuilder? ConvertToLatexSymbol(List<StringBuilder> row, bool convertWrapper)
    {
        if (row.Count == 0) return null;
        else if (row.Count == 1) return row[0];

        var escaped = new StringBuilder();
        var onlyOneChar = row.Count == 1 && row[0].Length == 1;
        if (!onlyOneChar) escaped.Append(LeftBrace);
        foreach (var sb in row)
        {
            for (var i = 0; i < sb.Length; i++)
            {
                escaped.Append(ConvertToLatexSymbol(sb[i], convertWrapper));
            }
        }
        if (!onlyOneChar) escaped.Append(RightBrace);
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
    private static readonly Dictionary<char, char[]> LatexSymbolMapping = new()
    {
        { '{', ToChars("\\{ ") },
        { '}', ToChars("\\} ") },
        { '\u2211', ToChars("\\sum") }, // ∑
        { '\u220F', ToChars("\\prod") }, // ∏
        { '\u2210', ToChars("\\coprod") }, // ∐
        { '\u22C2', ToChars("\\bigcap") }, // ⋂
        { '\u22C3', ToChars("\\bigcup") }, // ⋃
        { '\u222B', ToChars("\\int") }, // ∫
        { '\u222C', ToChars("\\iint") }, // ∬
        { '\u222D', ToChars("\\iiint") }, // ∭
        { '\u222E', ToChars("\\oint") }, // ∮
        { '\u222F', ToChars("\\mathop{{\\int\\!\\!\\!\\!\\!\\int}\\mkern-21mu \\bigcirc}") }, // ∯
        { '\u2230', ToChars("\\mathop{{\\int\\!\\!\\!\\!\\!\\int\\!\\!\\!\\!\\!\\int}\\mkern-31.2mu \\bigodot}") }, // ∰
        { '\u2232', ToChars("\\mathop{\\int\\mkern-20.8mu \\circlearrowleft}") }, // ∲
        { '\u2233', ToChars("\\mathop{\\int\\mkern-20.8mu \\circlearrowright}") }, // ∳
    };
    private static readonly char[] EscapedLeftBrace = ToChars("\\{ ");
    private static readonly char[] EscapedRightBrace = ToChars("\\} ");
    private static readonly char[] LeftBrace = ToChars("{");
    private static readonly char[] RightBrace = ToChars("}");
    private static char[] ConvertToLatexSymbol(char c, bool convertWrapper)
    {
        if (c == '{')
        {
            return convertWrapper ? EscapedLeftBrace : LeftBrace;
        }
        else if (c == '}')
        {
            return convertWrapper ? EscapedRightBrace : RightBrace;
        }
        else if (LatexSymbolMapping.TryGetValue(c, out var symbol))
        {
            return symbol;
        }
        else
        {
            return [c];
        }
    }

    #endregion

    #region Escape Rows

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

    #endregion

    #region Convert Equations

    private static readonly char[] SquareRoot = ToChars("\\sqrt");
    public static StringBuilder? ToSquareRoot(StringBuilder? insideEquation)
    {
        /// \sqrt{insideEquation}
        return Append(SquareRoot).Append(WhiteSpace).AppendWithWrapper(insideEquation);
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
    private static readonly char[] Super = ToChars("^");
    private static readonly char[] LeftSub = ToChars("{}_");
    private static readonly char[] Sub = ToChars("_");
    public static StringBuilder? ToSubOrSuper(SubSuperType type, Position position, StringBuilder? superEquation, StringBuilder? subEquation)
    {
        return type switch
        {
            SubSuperType.Sub => position switch
            {
                /// {}_insideEquation
                Position.Left => Append(LeftSub).Append(subEquation),
                /// _insideEquation
                Position.Right => Append(Sub).Append(subEquation),
                _ => throw new InvalidOperationException($"Invalid position for Sub: {position}"),
            },
            SubSuperType.Super => position switch
            {
                /// {}^insideEquation
                Position.Left => Append(LeftSuper).Append(superEquation),
                /// ^insideEquation
                Position.Right => Append(Super).Append(superEquation),
                _ => throw new InvalidOperationException($"Invalid position for Super: {position}"),
            },
            SubSuperType.SubAndSuper => position switch
            {
                /// {}_subEquation^superEquation
                Position.Left => Append(LeftSub).Append(subEquation).Append(Super).Append(superEquation),
                /// {}_subEquation^superEquation
                Position.Right => Append(Sub).Append(subEquation).Append(Super).Append(superEquation),
                _ => throw new InvalidOperationException($"Invalid positions for SuperSub: {position}"),
            },
            _ => throw new InvalidOperationException($"Invalid SuperSub type: {type}"),
        };
    }

    private static readonly char[] Sqrt = ToChars("\\sqrt");
    public static StringBuilder? ToNRoot(StringBuilder? insideEquation, StringBuilder? nthRootEquation)
    {
        /// \sqrt[nthRootEquation]{insideEquation}
        var sb = new StringBuilder();
        return sb.Append(Sqrt).Append('[').Append(nthRootEquation).Append(']').AppendWithWrapper(insideEquation);
    }

    private static readonly char[] Limits = ToChars("\\limits");
    private static readonly char[] NoLimits = ToChars("\\nolimits");
    public static StringBuilder? ToSign(SignType type, StringBuilder? sign, StringBuilder? mainEquation, StringBuilder? topSuperEquation, StringBuilder? bottomSubEquation)
    {
        return type switch
        {
            /// sign {mainEquation}
            SignType.Simple => Append(sign).Append(WhiteSpace).AppendWithWrapper(mainEquation),
            /// sign\limits_{bottomEquation} {mainEquation}
            SignType.Bottom => Append(sign).Append(Limits).Append('_').AppendWithWrapper(bottomSubEquation)
                .Append(WhiteSpace).AppendWithWrapper(mainEquation),
            /// sign\limits_{bottomEquation}^{topEquation} {mainEquation}
            SignType.BottomTop => Append(sign).Append(Limits).Append('_').AppendWithWrapper(bottomSubEquation)
                .Append(Super).AppendWithWrapper(topSuperEquation).Append(WhiteSpace)
                .AppendWithWrapper(mainEquation),
            /// sign\nolimits_{subEquation} {mainEquation}
            SignType.Sub => Append(sign).Append(NoLimits).Append('_').AppendWithWrapper(bottomSubEquation)
                .Append(WhiteSpace).AppendWithWrapper(mainEquation),
            /// {sign}\nolimits_{subEquation}^{superEquation} {mainEquation}
            SignType.SubSuper => Append(sign).Append(NoLimits).Append('_').AppendWithWrapper(bottomSubEquation)
                .Append(Super).AppendWithWrapper(topSuperEquation).Append(WhiteSpace)
                .AppendWithWrapper(mainEquation),
            _ => throw new InvalidOperationException($"Invalid SignType: {type}"),
        };
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
        return type switch
        {
            /// \boxed{insideEquation}
            BoxType.All => Append(Boxed).AppendWithWrapper(insideEquation),
            /// \left| \!{\overline {\, {insideEquation} \,}} \right.
            BoxType.LeftTop => Append(LeftTopBox1).AppendWithWrapper(insideEquation).Append(LeftTopBox2),
            /// \left. {\overline {\, {insideEquation} \,}}\! \right|
            BoxType.RightTop => Append(RightTopBox1).AppendWithWrapper(insideEquation).Append(RightTopBox2),
            /// \left| \!{\underline {\, {insideEquation} \,}} \right.
            BoxType.LeftBottom => Append(LeftBottomBox1).AppendWithWrapper(insideEquation).Append(LeftBottomBox2),
            /// \left. {\underline {\, {insideEquation} \,}}\! \right|
            BoxType.RightBottom => Append(RightBottomBox1).AppendWithWrapper(insideEquation).Append(RightBottomBox2),
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
        switch (type)
        {
            case ArrowType.LeftArrow:
                return position switch
                {
                    /// \xleftarrow{1}
                    Position.Top => Append(LeftArrow).Append(rowContainer1),
                    /// \xleftarrow[1]{}
                    Position.Bottom => Append(LeftArrow).Append('[').Append(rowContainer1).Append(']').Append(EmptyWrapper),
                    /// \xleftarrow[2]{1}
                    Position.BottomAndTop => Append(LeftArrow).Append('[').Append(rowContainer2).Append(']').Append(rowContainer1),
                    _ => throw new InvalidOperationException($"Unsupported position for LaTeX conversion: {position}"),
                };
            case ArrowType.RightArrow:
                return position switch
                {
                    /// \xrightarrow{1}
                    Position.Top => Append(RightArrow).Append(rowContainer1),
                    /// \xrightarrow[1]{}
                    Position.Bottom => Append(RightArrow).Append('[').Append(rowContainer1).Append(']').Append(EmptyWrapper),
                    /// \xrightarrow[2]{1}
                    Position.BottomAndTop => Append(RightArrow).Append('[').Append(rowContainer2).Append(']').Append(rowContainer1),
                    _ => throw new InvalidOperationException($"Unsupported position for LaTeX conversion: {position}"),
                };
            case ArrowType.DoubleArrow:
                return position switch
                {
                    /// \overset 1 \longleftrightarrow
                    Position.Top => Append(OverSet).Append(WhiteSpace).Append(rowContainer1).Append(WhiteSpace)
                        .Append(LongLeftRightArrow),
                    /// \underset 1 \longleftrightarrow
                    Position.Bottom => Append(UnderSet).Append(WhiteSpace).Append(rowContainer1).Append(WhiteSpace)
                        .Append(LongLeftRightArrow),
                    /// \underset{2}{\overset{1} {\longleftrightarrow}}
                    Position.BottomAndTop => Append(UnderSet).Append(rowContainer2).Append('{').Append(OverSet)
                        .Append(rowContainer1).Append(WhiteSpace).Append('{').Append(LongLeftRightArrow)
                        .Append('}').Append('}'),
                    _ => throw new InvalidOperationException($"Unsupported position for LaTeX conversion: {position}"),
                };
            case ArrowType.RightLeftArrow:
                return position switch
                {
                    /// \overset 1 \leftrightarrows
                    Position.Top => Append(OverSet).Append(WhiteSpace).Append(rowContainer1).Append(WhiteSpace)
                        .Append(LeftRightArrows),
                    /// \underset 1 \leftrightarrows
                    Position.Bottom => Append(UnderSet).Append(WhiteSpace).Append(rowContainer1).Append(WhiteSpace)
                        .Append(LeftRightArrows),
                    /// \underset{2}{\overset{1} {\longleftrightarrow}}
                    Position.BottomAndTop => Append(UnderSet).Append(rowContainer2).Append('{').Append(OverSet)
                        .Append(rowContainer1).Append(WhiteSpace).Append('{').Append(LongLeftRightArrow)
                        .Append('}').Append('}'),
                    _ => throw new InvalidOperationException($"Unsupported position for LaTeX conversion: {position}"),
                };
            // TODO: Support editting these in the settings.
            case ArrowType.RightSmallLeftArrow:
                switch (position)
                {
                    case Position.Top:
                        MessageBox.Show("No translation available for Large over small arrow with upper text slot.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    case Position.Bottom:
                        MessageBox.Show("No translation available for Large over small arrow with lower text slot.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    case Position.BottomAndTop:
                        MessageBox.Show("No translation available for Large over small arrow with upper and lower text slots.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    default:
                        throw new InvalidOperationException($"Unsupported position for LaTeX conversion: {position}");
                }
            case ArrowType.SmallRightLeftArrow:
                switch (position)
                {
                    case Position.Top:
                        MessageBox.Show("No translation available for Small over large arrow with upper text slot.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    case Position.Bottom:
                        MessageBox.Show("No translation available for Small over large arrow with lower text slot.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    case Position.BottomAndTop:
                        MessageBox.Show("No translation available for Small over large arrow with upper and lower text slots.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    default:
                        throw new InvalidOperationException($"Unsupported position for LaTeX conversion: {position}");
                }
            case ArrowType.RightLeftHarpoon:
                switch (position)
                {
                    case Position.Top:
                        MessageBox.Show("No translation available for Harpoons with upper text slot.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    case Position.Bottom:
                        MessageBox.Show("No translation available for Harpoons with lower text slot.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    case Position.BottomAndTop:
                        MessageBox.Show("No translation available for Harpoons with upper and lower text slots.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    default:
                        throw new InvalidOperationException($"Unsupported position for LaTeX conversion: {position}");
                }
            case ArrowType.RightSmallLeftHarpoon:
                switch (position)
                {
                    case Position.Top:
                        MessageBox.Show("No translation available for Large over small harpoon with upper text slot.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    case Position.Bottom:
                        MessageBox.Show("No translation available for Large over small harpoon with lower text slot.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    case Position.BottomAndTop:
                        MessageBox.Show("No translation available for Large over small harpoon with upper and lower text slots.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    default:
                        throw new InvalidOperationException($"Unsupported position for LaTeX conversion: {position}");
                }
            case ArrowType.SmallRightLeftHarpoon:
                switch (position)
                {
                    case Position.Top:
                        MessageBox.Show("No translation available for Small over large harpoon with upper text slot.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    case Position.Bottom:
                        MessageBox.Show("No translation available for Small over large harpoon with lower text slot.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    case Position.BottomAndTop:
                        MessageBox.Show("No translation available for Small over large harpoon with upper and lower text slots.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    default:
                        throw new InvalidOperationException($"Unsupported position for LaTeX conversion: {position}");
                }
            default:
                throw new InvalidOperationException($"Unsupported arrow type for LaTeX conversion: {type}");
        }
    }

    private static readonly char[] WideTilde = ToChars("\\widetilde");
    private static readonly char[] WideHat = ToChars("\\widehat");
    private static readonly char[] OverLine = ToChars("\\overline");
    private static readonly char[] UnderLine = ToChars("\\underline");
    private static readonly char[] Cross = ToChars("\\xcancel");
    private static readonly char[] RightCross = ToChars("\\cancel");
    private static readonly char[] LeftCross = ToChars("\\bcancel");
    private static readonly char[] OverRightArrow = ToChars("\\overrightarrow");
    private static readonly char[] OverLeftArrow = ToChars("\\overleftarrow");
    private static readonly char[] OverDoubleArrow = ToChars("\\overleftrightarrow");
    private static readonly char[] UnderRightArrow = ToChars("\\underrightarrow");
    private static readonly char[] UnderLeftArrow = ToChars("\\underleftarrow");
    private static readonly char[] UnderDoubleArrow = ToChars("\\underleftrightarrow");
    public static StringBuilder? ToDecorated(DecorationType type, Position position, StringBuilder? insideEquation)
    {
        switch (type)
        {
            case DecorationType.Tilde:
                return position switch
                {
                    /// \widetilde insideEquation
                    Position.Top => Append(WideTilde).Append(WhiteSpace).Append(insideEquation),
                    _ => throw new InvalidOperationException($"Invalid position for Tilde decoration: {position}"),
                };
            case DecorationType.Hat:
                return position switch
                {
                    /// \widehat insideEquation
                    Position.Top => Append(WideHat).Append(WhiteSpace).Append(insideEquation),
                    _ => throw new InvalidOperationException($"Invalid position for Hat decoration: {position}"),
                };
            case DecorationType.Parenthesis:
                switch (position)
                {
                    case Position.Top:
                        MessageBox.Show("No translation available for Arc.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    default:
                        throw new InvalidOperationException($"Invalid position for Parenthesis decoration: {position}");
                }
            case DecorationType.Tortoise:
                switch (position)
                {
                    case Position.Top:
                        MessageBox.Show("No translation available for Joint status.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    default:
                        throw new InvalidOperationException($"Invalid position for Tortoise decoration: {position}");
                }
            case DecorationType.Bar:
                return position switch
                {
                    /// \overline insideEquation
                    Position.Top => Append(OverLine).Append(WhiteSpace).Append(insideEquation),
                    /// \underline insideEquation
                    Position.Bottom => Append(UnderLine).Append(WhiteSpace).Append(insideEquation),
                    _ => throw new InvalidOperationException($"Invalid position for Bar decoration: {position}"),
                };
            case DecorationType.DoubleBar:
                return position switch
                {
                    /// \overline{\overline insideEquation}
                    Position.Top => Append(OverLine).Append('{').Append(OverLine).Append(WhiteSpace)
                        .Append(insideEquation).Append('}'),
                    /// \underline{\underline insideEquation}
                    Position.Bottom => Append(UnderLine).Append('{').Append(UnderLine).Append(WhiteSpace)
                        .Append(insideEquation).Append('}'),
                    _ => throw new InvalidOperationException($"Invalid position for DoubleBar decoration: {position}"),
                };
            case DecorationType.RightArrow:
                return position switch
                {
                    /// \overrightarrow insideEquation
                    Position.Top => Append(OverRightArrow).Append(WhiteSpace).Append(insideEquation),
                    /// \underrightarrow insideEquation
                    Position.Bottom => Append(UnderRightArrow).Append(WhiteSpace).Append(insideEquation),
                    _ => throw new InvalidOperationException($"Invalid position for RightArrow decoration: {position}"),
                };
            case DecorationType.LeftArrow:
                return position switch
                {
                    /// \overleftarrow insideEquation
                    Position.Top => Append(OverLeftArrow).Append(WhiteSpace).Append(insideEquation),
                    /// \underleftarrow insideEquation
                    Position.Bottom => Append(UnderLeftArrow).Append(WhiteSpace).Append(insideEquation),
                    _ => throw new InvalidOperationException($"Invalid position for LeftArrow decoration: {position}"),
                };
            case DecorationType.RightHarpoonUpBarb:
                switch (position)
                {
                    case Position.Top:
                        MessageBox.Show("No translation available for Right harpoon over-bar.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    case Position.Bottom:
                        MessageBox.Show("No translation available for Right harpoon under-bar.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    default:
                        throw new InvalidOperationException($"Invalid position for RightHarpoonUpBarb decoration: {position}");
                }
            case DecorationType.LeftHarpoonUpBarb:
                switch (position)
                {
                    case Position.Top:
                        MessageBox.Show("No translation available for Left harpoon over-bar.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    case Position.Bottom:
                        MessageBox.Show("No translation available for Left harpoon under-bar.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    default:
                        throw new InvalidOperationException($"Invalid position for LeftHarpoonUpBarb decoration: {position}");
                }
            case DecorationType.DoubleArrow:
                return position switch
                {
                    /// \overleftrightarrow insideEquation
                    Position.Top => Append(OverDoubleArrow).Append(WhiteSpace).Append(insideEquation),
                    /// \underleftrightarrow insideEquation
                    Position.Bottom => Append(UnderDoubleArrow).Append(WhiteSpace).Append(insideEquation),
                    _ => throw new InvalidOperationException($"Invalid position for DoubleArrow decoration: {position}"),
                };
            case DecorationType.StrikeThrough:
                switch (position)
                {
                    case Position.Middle:
                        MessageBox.Show("No translation available for Mid-line strike-through.\nPlease add a translation for it in the settings.", "Translation Error");
                        return null;
                    default:
                        throw new InvalidOperationException($"Invalid position for StrikeThrough decoration: {position}");
                }
            case DecorationType.Cross:
                return position switch
                {
                    /// \xcancel{insideEquation}
                    Position.Middle => Append(Cross).AppendWithWrapper(insideEquation),
                    _ => throw new InvalidOperationException($"Invalid position for Cross decoration: {position}"),
                };
            case DecorationType.RightCross:
                return position switch
                {
                    /// \cancel{insideEquation}
                    Position.Middle => Append(RightCross).AppendWithWrapper(insideEquation),
                    _ => throw new InvalidOperationException($"Invalid position for RightCross decoration: {position}"),
                };
            case DecorationType.LeftCross:
                return position switch
                {
                    /// \bcancel{insideEquation}
                    Position.Middle => Append(LeftCross).AppendWithWrapper(insideEquation),
                    _ => throw new InvalidOperationException($"Invalid position for LeftCross decoration: {position}"),
                };
            default:
                throw new InvalidOperationException($"Unsupported decoration type for LaTeX conversion: {type}");
        }
    }

    private static readonly char[] OverBrace = ToChars("\\overbrace");
    private static readonly char[] UnderBrace = ToChars("\\underbrace");
    public static StringBuilder? ToHorizontalBracket(HorizontalBracketSignType type, StringBuilder? topEquation,
        StringBuilder? bottomEquation)
    {
        switch (type)
        {
            case HorizontalBracketSignType.TopCurly:
                /// \overbrace bottomEquation^topEquation
                return Append(OverBrace).Append(WhiteSpace).Append(bottomEquation).Append(Super)
                    .Append(topEquation);
            case HorizontalBracketSignType.BottomCurly:
                /// \underbrace topEquation_bottomEquation
                return Append(UnderBrace).Append(WhiteSpace).Append(topEquation).Append('_')
                    .Append(bottomEquation);
            case HorizontalBracketSignType.TopSquare:
                MessageBox.Show("No translation available for Upper horizontal bracket.\nPlease add a translation for it in the settings.", "Translation Error");
                return null;
            case HorizontalBracketSignType.BottomSquare:
                MessageBox.Show("No translation available for Lower horizontal bracket.\nPlease add a translation for it in the settings.", "Translation Error");
                return null;
            default:
                throw new InvalidOperationException($"Invalid HorizontalBracketSignType: {type}");
        }
    }

    private static readonly char[] MathOp = ToChars("\\mathop");
    public static StringBuilder? ToComposite(bool isCompositeBig, Position position, StringBuilder? mainEquation, StringBuilder? topSuperEquation, StringBuilder? bottomSubEquation)
    {
        if (isCompositeBig)
        {
            return position switch
            {
                /// \mathop mainEquation\limits_bottomSubEquation
                Position.Bottom => Append(MathOp).Append(WhiteSpace).Append(mainEquation)
                    .Append(Limits).Append('_').Append(bottomSubEquation),
                /// \mathop mainEquation\limits_bottomSubEquation
                Position.Top => Append(MathOp).Append(WhiteSpace).Append(mainEquation)
                    .Append(Limits).Append(Super).Append(topSuperEquation),
                /// \mathop mainEquation\limits_bottomSubEquation
                Position.BottomAndTop => Append(MathOp).Append(WhiteSpace).Append(mainEquation)
                    .Append(Limits).Append('_').Append(bottomSubEquation)
                    .Append(Super).Append(topSuperEquation),
                /// \mathop mainEquation\limits_bottomSubEquation
                Position.Sub => Append(MathOp).Append(WhiteSpace).Append(mainEquation)
                    .Append(NoLimits).Append('_').Append(bottomSubEquation),
                /// \mathop mainEquation\limits_bottomSubEquation
                Position.Super => Append(MathOp).Append(WhiteSpace).Append(mainEquation)
                    .Append(NoLimits).Append(Super).Append(topSuperEquation),
                /// \mathop mainEquation\limits_bottomSubEquation
                Position.SubAndSuper => Append(MathOp).Append(WhiteSpace).Append(mainEquation)
                    .Append(NoLimits).Append('_').Append(bottomSubEquation)
                    .Append(Super).Append(topSuperEquation),
                _ => throw new InvalidOperationException($"Invalid position for Composite: {position}"),
            };
        }
        else
        {
            return position switch
            {
                /// \mathop mainEquation\limits_bottomSubEquation
                Position.Bottom => Append(MathOp).Append(WhiteSpace).Append(mainEquation)
                    .Append(Limits).Append('_').Append(bottomSubEquation),
                /// \mathop mainEquation\limits_bottomSubEquation
                Position.Top => Append(MathOp).Append(WhiteSpace).Append(mainEquation)
                    .Append(Limits).Append(Super).Append(topSuperEquation),
                /// \mathop mainEquation\limits_bottomSubEquation
                Position.BottomAndTop => Append(MathOp).Append(WhiteSpace).Append(mainEquation)
                    .Append(Limits).Append('_').Append(bottomSubEquation)
                    .Append(Super).Append(topSuperEquation),
                _ => throw new InvalidOperationException($"Invalid position for Composite: {position}"),
            };
        }
    }

    private static readonly char[] DivMath1 = ToChars("\\[\\left){\\vphantom{1");
    private static readonly char[] DivMath2 = ToChars("}}\\right.\n\\!\\!\\!\\!\\overline{\\,\\,\\,\\vphantom 1");
    private static readonly char[] DivMath3 = ToChars(" }\\]");
    private static readonly char[] DivMathWithTop1 = ToChars("\\mathop{\\left){\\vphantom{1");
    private static readonly char[] DivMathWithTop2 = ToChars("}}\\right.\n\\!\\!\\!\\!\\overline{\\,\\,\\,\\vphantom 1");
    private static readonly char[] DivMathWithTop3 = ToChars("}}\n\\limits^{\\displaystyle\\hfill\\,\\,\\, ");
    private static readonly char[] Frac = ToChars("\\frac");
    private static readonly char[] DivRegularSmall1 = ToChars("{\\textstyle{");
    private static readonly char[] DivRegularSmall2 = ToChars(" \\over ");
    private static readonly char[] DivRegularSmall3 = ToChars("}}");
    private static readonly char[] DivSlanted1 = ToChars("{\\raise0.7ex\\hbox{$");
    private static readonly char[] DivSlanted2 = ToChars("$} \\!\\mathord{\\left/\n {\\vphantom {");
    private static readonly char[] DivSlanted3 = ToChars("}}\\right.\\kern-\\nulldelimiterspace}\n\\!\\lower0.7ex\\hbox{$");
    private static readonly char[] DivSlanted4 = ToChars("$}}");
    private static readonly char[] DivSlantedSmall1 = ToChars("{\\raise0.5ex\\hbox{$\\scriptstyle ");
    private static readonly char[] DivSlantedSmall2 = ToChars("\\kern-0.1em/\\kern-0.15em\n\\lower0.25ex\\hbox{$\\scriptstyle ");
    private static readonly char[] DivSlantedSmall3 = ToChars("$}}");
    private static readonly char[] DivHoriz1 = ToChars(" \\mathord{\\left/\n {\\vphantom {");
    private static readonly char[] DivHoriz2 = ToChars("}} \\right.\n \\kern-\\nulldelimiterspace} ");
    public static StringBuilder? ToDivision(DivisionType type, StringBuilder? insideOrTopEquation, StringBuilder? bottomEquation)
    {
        switch (type)
        {
            case DivisionType.DivMath:
                /// \[\left){\vphantom{1a}}\right.
                /// \!\!\!\!\overline{\,\,\,\vphantom 1{a} }\]
                return Append(DivMath1).Append(insideOrTopEquation).Append(DivMath2)
                    .AppendWithWrapper(insideOrTopEquation).Append(DivMath3);
            case DivisionType.DivMathWithTop:
                /// \mathop{\left){\vphantom{1a}}\right.
                /// \!\!\!\!\overline{\,\,\,\vphantom 1{a}}}
                /// \limits^{\displaystyle\hfill\,\,\, b}
                return Append(DivMathWithTop1).Append(insideOrTopEquation).Append(DivMathWithTop2)
                    .AppendWithWrapper(insideOrTopEquation).Append(DivMathWithTop3).Append(bottomEquation)
                    .Append('}');
            case DivisionType.DivRegular:
                /// \frac{a}{b}
                return Append(Frac).AppendWithWrapper(insideOrTopEquation).AppendWithWrapper(bottomEquation);
            case DivisionType.DivDoubleBar:
                MessageBox.Show("No translation available for Double bar division.\nPlease add a translation for it in the settings.", "Translation Error");
                return null;
            case DivisionType.DivTripleBar:
                MessageBox.Show("No translation available for Triple bar division.\nPlease add a translation for it in the settings.", "Translation Error");
                return null;
            case DivisionType.DivRegularSmall:
                /// {\textstyle{a \over b}}
                return Append(DivRegularSmall1).Append(insideOrTopEquation).Append(DivRegularSmall2)
                    .Append(bottomEquation).Append(DivRegularSmall3);
            case DivisionType.DivSlanted:
                /// {\raise0.7ex\hbox{$a$} \!\mathord{\left/
                ///  {\vphantom {a b}}\right.\kern-\nulldelimiterspace}
                /// \!\lower0.7ex\hbox{$b$}}
                return Append(DivSlanted1).Append(insideOrTopEquation).Append(DivSlanted2)
                    .Append(insideOrTopEquation).Append(WhiteSpace).Append(bottomEquation)
                    .Append(DivSlanted3).Append(bottomEquation).Append(DivSlanted4);
            case DivisionType.DivSlantedSmall:
                /// {\raise0.5ex\hbox{$\scriptstyle a$}
                /// \kern-0.1em/\kern-0.15em
                /// \lower0.25ex\hbox{$\scriptstyle b$}}
                return Append(DivSlantedSmall1).Append(insideOrTopEquation).Append(DivSlantedSmall2)
                    .Append(bottomEquation).Append(DivSlantedSmall3);
            case DivisionType.DivHoriz:
                /// {a \mathord{\left/
                ///  {\vphantom {a b}} \right.
                ///  \kern-\nulldelimiterspace} b}
                return Append('{').Append(insideOrTopEquation).Append(DivHoriz1)
                    .Append(insideOrTopEquation).Append(WhiteSpace).Append(bottomEquation)
                    .Append(DivHoriz2).Append(bottomEquation).Append('}');
            case DivisionType.DivHorizSmall:
                MessageBox.Show("No translation available for Horizontal small division.\nPlease add a translation for it in the settings.", "Translation Error");
                return null;
            case DivisionType.DivMathInverted:
                MessageBox.Show("No translation available for Inverted division.\nPlease add a translation for it in the settings.", "Translation Error");
                return null;
            case DivisionType.DivInvertedWithBottom:
                MessageBox.Show("No translation available for Inverted division with bottom.\nPlease add a translation for it in the settings.", "Translation Error");
                return null;
            case DivisionType.DivTriangleFixed:
                MessageBox.Show("No translation available for Triangle fixed division.\nPlease add a translation for it in the settings.", "Translation Error");
                return null;
            case DivisionType.DivTriangleExpanding:
                MessageBox.Show("No translation available for Triangle expanding division.\nPlease add a translation for it in the settings.", "Translation Error");
                return null;
            default:
                throw new InvalidOperationException($"Invalid DivisionType: {type}");
        }
    }

    private static readonly char[] DoubleArrowBarBracket1 = ToChars("\\left\\langle ");
    private static readonly char[] DoubleArrowBarBracket2 = ToChars("\n \\mathrel{\\left | {\\vphantom {");
    private static readonly char[] DoubleArrowBarBracket3 = ToChars("}}\n\\right. \\kern-\\nulldelimiterspace}\n ");
    private static readonly char[] DoubleArrowBarBracket4 = ToChars(" \\right\\rangle ");
    public static StringBuilder? ToDoubleArrowBarBracket(StringBuilder? leftEquation, StringBuilder? rightEquation)
    {
        /// \left\langle {a}
        ///  \mathrel{\left | {\vphantom {a b}}
        ///  \right. \kern-\nulldelimiterspace}
        ///  {b} \right\rangle 
        return Append(DoubleArrowBarBracket1).AppendWithWrapper(leftEquation).Append(DoubleArrowBarBracket2)
            .Append(leftEquation).Append(WhiteSpace).Append(rightEquation).Append(DoubleArrowBarBracket3)
            .AppendWithWrapper(rightEquation).Append(DoubleArrowBarBracket4);
    }

    private static readonly char[] Left = ToChars("\\left");
    private static readonly char[] Right = ToChars("\\right");
    private static readonly char[] Langle = ToChars("\\langle");
    private static readonly char[] Rangle = ToChars("\\rangle");
    private static readonly char[] LFloor = ToChars("\\lfloor");
    private static readonly char[] RFloor = ToChars("\\rfloor");
    private static readonly char[] LeftCeil = ToChars("\\lceil");
    private static readonly char[] RightCeil = ToChars("\\rceil");
    private static readonly char[] LeftSquareBar = ToChars("\\left[\\kern-0.15em\\left[ ");
    private static readonly char[] RightSquareBar = ToChars("\\right]\\kern-0.15em\\right]");
    public static StringBuilder? ToLeftRightBracket(BracketSignType leftBracketType, BracketSignType rightBracketType, StringBuilder? insideEquation)
    {
        if (leftBracketType == BracketSignType.LeftRound && rightBracketType == BracketSignType.RightRound)
        {
            /// \left( a \right)
            return Append(Left).Append('(').Append(WhiteSpace).Append(insideEquation).Append(WhiteSpace)
                .Append(Right).Append(')');
        }
        else if (leftBracketType == BracketSignType.LeftSquare && rightBracketType == BracketSignType.RightSquare)
        {
            /// \left[ a \right]
            return Append(Left).Append('[').Append(WhiteSpace).Append(insideEquation).Append(WhiteSpace)
                .Append(Right).Append(']');
        }
        else if (leftBracketType == BracketSignType.LeftCurly && rightBracketType == BracketSignType.RightCurly)
        {
            /// \left\{ a \right\}
            return Append(Left).Append('\\').Append('{').Append(WhiteSpace).Append(insideEquation).Append(WhiteSpace)
                .Append(Right).Append('\\').Append('}');
        }
        else if (leftBracketType == BracketSignType.LeftAngle && rightBracketType == BracketSignType.RightAngle)
        {
            /// \left\langle a \right\rangle 
            return Append(Left).Append(Langle).Append(WhiteSpace).Append(insideEquation).Append(WhiteSpace)
                .Append(Right).Append(Rangle).Append(WhiteSpace);
        }
        else if (leftBracketType == BracketSignType.LeftBar && rightBracketType == BracketSignType.RightBar)
        {
            /// \left| a \right|
            return Append(Left).Append('|').Append(WhiteSpace).Append(insideEquation).Append(WhiteSpace)
                .Append(Right).Append('|');
        }
        else if (leftBracketType == BracketSignType.LeftDoubleBar && rightBracketType == BracketSignType.RightDoubleBar)
        {
            /// \left\| a \right\|
            return Append(Left).Append('\\').Append('|').Append(WhiteSpace).Append(insideEquation)
                .Append(WhiteSpace).Append(Right).Append('\\').Append('|');
        }
        else if (leftBracketType == BracketSignType.LeftFloor && rightBracketType == BracketSignType.RightFloor)
        {
            /// \left\lfloor a \right\rfloor 
            return Append(Left).Append(LFloor).Append(WhiteSpace).Append(insideEquation)
                .Append(WhiteSpace).Append(Right).Append(RFloor).Append(WhiteSpace);
        }
        else if (leftBracketType == BracketSignType.LeftCeiling && rightBracketType == BracketSignType.RightCeiling)
        {
            /// \left\lceil a \right\rceil 
            return Append(Left).Append(LeftCeil).Append(WhiteSpace).Append(insideEquation)
                .Append(WhiteSpace).Append(Right).Append(RightCeil).Append(WhiteSpace);
        }
        else if (leftBracketType == BracketSignType.LeftSquare && rightBracketType == BracketSignType.RightRound)
        {
            /// \left[ a \right)
            return Append(Left).Append('[').Append(WhiteSpace).Append(insideEquation).Append(WhiteSpace)
                .Append(Right).Append(')');
        }
        else if (leftBracketType == BracketSignType.LeftRound && rightBracketType == BracketSignType.RightSquare)
        {
            /// \left( a \right]
            return Append(Left).Append('(').Append(WhiteSpace).Append(insideEquation).Append(WhiteSpace)
                .Append(Right).Append(']');
        }
        else if (leftBracketType == BracketSignType.LeftBar && rightBracketType == BracketSignType.RightAngle)
        {
            /// \left| a \right\rangle 
            return Append(Left).Append('|').Append(WhiteSpace).Append(insideEquation)
                .Append(WhiteSpace).Append(Right).Append(Rangle).Append(WhiteSpace);
        }
        else if (leftBracketType == BracketSignType.LeftAngle && rightBracketType == BracketSignType.RightBar)
        {
            /// \left\langle a \right|
            return Append(Left).Append(Langle).Append(WhiteSpace).Append(insideEquation)
                .Append(WhiteSpace).Append(Right).Append('|');
        }
        else if (leftBracketType == BracketSignType.LeftSquare && rightBracketType == BracketSignType.LeftSquare)
        {
            /// \left[ a \right[
            return Append(Left).Append('[').Append(WhiteSpace).Append(insideEquation).Append(WhiteSpace)
                .Append(Right).Append('[');
        }
        else if (leftBracketType == BracketSignType.RightSquare && rightBracketType == BracketSignType.RightSquare)
        {
            /// \left] a \right]
            return Append(Left).Append(']').Append(WhiteSpace).Append(insideEquation).Append(WhiteSpace)
                .Append(Right).Append(']');
        }
        else if (leftBracketType == BracketSignType.RightSquare && rightBracketType == BracketSignType.LeftSquare)
        {
            /// \left] a \right[
            return Append(Left).Append(']').Append(WhiteSpace).Append(insideEquation).Append(WhiteSpace)
                .Append(Right).Append('[');
        }
        else if (leftBracketType == BracketSignType.LeftSquareBar && rightBracketType == BracketSignType.RightSquareBar)
        {
            /// \left[\kern-0.15em\left[ a 
            ///  \right]\kern-0.15em\right]
            return Append(LeftSquareBar).Append(insideEquation).Append(WhiteSpace).Append('\n')
                .Append(WhiteSpace).Append(RightSquareBar);
        }
        else
        {
            throw new NotImplementedException($"Unsupported combination of left and right bracket types for LaTeX conversion: {leftBracketType}, {rightBracketType}");
        }
    }

    public static StringBuilder? ToLeftOrRightBracket(BracketSignType bracketType, StringBuilder? insideEquation)
    {
        if (bracketType == BracketSignType.LeftRound)
        {
            /// \left( a \right.
            return Append(Left).Append('(').Append(WhiteSpace).Append(insideEquation).Append(WhiteSpace)
                .Append(Right).Append('.');
        }
        else if (bracketType == BracketSignType.RightRound)
        {
            /// \left. a \right)
            return Append(Left).Append('.').Append(WhiteSpace).Append(insideEquation).Append(WhiteSpace)
                .Append(Right).Append(')');
        }
        else if (bracketType == BracketSignType.LeftSquare)
        {
            /// \left[ a \right.
            return Append(Left).Append('[').Append(WhiteSpace).Append(insideEquation).Append(WhiteSpace)
                .Append(Right).Append('.');
        }
        else if (bracketType == BracketSignType.RightSquare)
        {
            /// \left. a \right]
            return Append(Left).Append('.').Append(WhiteSpace).Append(insideEquation).Append(WhiteSpace)
                .Append(Right).Append(']');
        }
        else if (bracketType == BracketSignType.LeftCurly)
        {
            /// \left\{ a \right.
            return Append(Left).Append('\\').Append('{').Append(WhiteSpace).Append(insideEquation)
                .Append(WhiteSpace).Append(Right).Append('.');
        }
        else if (bracketType == BracketSignType.RightCurly)
        {
            /// \left. a \right\}
            return Append(Left).Append('.').Append(WhiteSpace).Append(insideEquation)
                .Append(WhiteSpace).Append(Right).Append('\\').Append('}');
        }
        else if (bracketType == BracketSignType.LeftAngle)
        {
            /// \left\langle a \right.
            return Append(Left).Append(Langle).Append(WhiteSpace).Append(insideEquation)
                .Append(WhiteSpace).Append(Right).Append('.');
        }
        else if (bracketType == BracketSignType.RightAngle)
        {
            /// \left. a \right\rangle 
            return Append(Left).Append('.').Append(WhiteSpace).Append(insideEquation)
                .Append(WhiteSpace).Append(Right).Append(Rangle).Append(WhiteSpace);
        }
        else if (bracketType == BracketSignType.LeftBar)
        {
            /// \left| a \right.
            return Append(Left).Append('|').Append(WhiteSpace).Append(insideEquation)
                .Append(WhiteSpace).Append(Right).Append('.');
        }
        else if (bracketType == BracketSignType.RightBar)
        {
            /// \left. a \right|
            return Append(Left).Append('.').Append(WhiteSpace).Append(insideEquation)
                .Append(WhiteSpace).Append(Right).Append('|');
        }
        else if (bracketType == BracketSignType.LeftDoubleBar)
        {
            /// \left\| a \right.
            return Append(Left).Append('\\').Append('|').Append(WhiteSpace).Append(insideEquation)
                .Append(WhiteSpace).Append(Right).Append('.');
        }
        else if (bracketType == BracketSignType.RightDoubleBar)
        {
            /// \left. a \right\|
            return Append(Left).Append('.').Append(WhiteSpace).Append(insideEquation)
                .Append(WhiteSpace).Append(Right).Append('\\').Append('|');
        }
        else if (bracketType == BracketSignType.LeftSquareBar)
        {
            /// \left[\kern-0.15em\left[ a 
            ///  \right.
            return Append(LeftSquareBar).Append(insideEquation).Append(WhiteSpace).Append('\n')
                .Append(WhiteSpace).Append(Right).Append('.');
        }
        else if (bracketType == BracketSignType.RightSquareBar)
        {
            /// \left. a 
            ///  \right]\kern-0.15em\right]
            return Append(Left).Append('.').Append(WhiteSpace).Append(insideEquation)
                .Append(WhiteSpace).Append('\n').Append(WhiteSpace).Append(RightSquareBar);
        }
        else
        {
            throw new NotImplementedException($"Unsupported left or right bracket types for LaTeX conversion: {bracketType}");
        }
    }

    #endregion
}
