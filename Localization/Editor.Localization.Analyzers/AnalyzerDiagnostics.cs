using Editor.Localization.Shared;
using Microsoft.CodeAnalysis;

namespace Editor.Localization.Analyzers
{
    public static class AnalyzerDiagnostics
    {
        public static readonly DiagnosticDescriptor OldLocalizationApiUsed = new DiagnosticDescriptor(
            Constants.OldLocalizationApiUsedId,
            Constants.OldLocalizationApiUsedTitle,
            $"Use `{Constants.ClassName}.{{0}}({{1}})` instead",
            "Localization",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );
    }
}
