using Editor.Localization.Shared;
using Microsoft.CodeAnalysis;

namespace Editor.Localization.SourceGenerators
{
    public static class SourceGeneratorDiagnostics
    {
        public static readonly DiagnosticDescriptor CouldNotFindResourceDictionaries = new DiagnosticDescriptor(
            "FLSG0001",
            "Could not find resource dictionaries",
            "Could not find resource dictionaries. There must be a `en.xaml` file under `Language` folder.",
            "Localization",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor LocalizationKeyUnused = new DiagnosticDescriptor(
            "FLSG0002",
            "Localization key is unused",
            $"Method `{Constants.ClassName}.{{0}}` is never used",
            "Localization",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor EnumFieldLocalizationKeyValueInvalid = new DiagnosticDescriptor(
            "FLSG0003",
            "Enum field localization key and value invalid",
            $"Enum field `{{0}}` does not have a valid localization key or value",
            "Localization",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );
    }
}
