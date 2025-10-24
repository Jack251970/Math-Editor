using System.Collections.Generic;

namespace Editor
{
    internal static class FunctionNames
    {
        private static readonly List<string> _knownFunctionNames = [];

        static FunctionNames()
        {
            _knownFunctionNames.AddRange([
                "arccos", "arcsin", "arctan", "arg", "cos", "cosh", "cot", "coth",
                "cov", "csc", "curl", "deg", "det", "dim", "div", "erf", "exp", "gcd", "glb", "grad", "hom", "lm",
                "inf", "int", "ker", "lg", "lim", "ln", "log", "lub", "max",
                "min", "mod", "Pr", "Re", "rot", "sec", "sgn", "sin", "sinh", "sup", "tan", "tanh", "var",
            ]);
        }

        public static bool IsFunctionName(string text)
        {
            return _knownFunctionNames.Contains(text);
        }

        // TODO:
        // The CheckForFunctionName method uses a linear search with EndsWith checks,
        // which is inefficient for repeated calls.
        // Consider building a trie or using a more efficient data structure to improve performance,
        // especially since function names have fixed lengths.
        public static string? CheckForFunctionName(string text)
        {
            foreach (var name in _knownFunctionNames)
            {
                if (text.EndsWith(name))
                {
                    return name;
                }
            }
            return null;
        }
    }
}
