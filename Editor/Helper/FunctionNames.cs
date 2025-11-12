using System.Collections.Generic;
using System.Linq;

namespace Editor
{
    internal static class FunctionNames
    {
        private static readonly List<string> _knownFunctionNames = [];
        private static readonly TrieNode _root = new();

        static FunctionNames()
        {
            _knownFunctionNames.AddRange([
                "arccos", "arcsin", "arctan", "arg", "cos", "cosh", "cot", "coth",
                "cov", "csc", "curl", "deg", "det", "dim", "div", "erf", "exp", "gcd", "glb", "grad", "hom", "lm",
                "inf", "int", "ker", "lg", "lim", "ln", "log", "lub", "max",
                "min", "mod", "Pr", "Re", "rot", "sec", "sgn", "sin", "sinh", "sup", "tan", "tanh", "var",
            ]);

            foreach (var name in _knownFunctionNames)
            {
                AddToTrie(name);
            }
        }

        private static void AddToTrie(string word)
        {
            var node = _root;
            for (var i = word.Length - 1; i >= 0; i--)
            {
                var c = word[i];

                if (!node.Children.TryGetValue(c, out var next))
                {
                    next = new TrieNode();
                    node.Children[c] = next;
                }
                node = next;
            }
            node.IsWord = true;
            node.Word = word;
        }

        public static string? CheckForFunctionName(string text)
        {
            var node = _root;
            for (var i = text.Length - 1; i >= 0; i--)
            {
                if (!node.Children.TryGetValue(text[i], out node))
                {
                    return null;
                }

                if (node.IsWord)
                {
                    return node.Word;
                }
            }

            return null;
        }

        private class TrieNode
        {
            public readonly Dictionary<char, TrieNode> Children = [];
            public bool IsWord;
            public string? Word;
        }
    }
}
