using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Editor.Localization.Shared
{
    public static class Helper
    {
        #region Tab String

        public static string Spacing(int n)
        {
            return new string(' ', n * 4);
        }

        #endregion
    }
}
