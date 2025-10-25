using System.Text;

namespace Editor;

public static class LatexExtensions
{
    public static StringBuilder AppendWithWrapper(this StringBuilder sb, StringBuilder? equ)
    {
        return sb.Append('{').Append(equ).Append('}');
    }
}
