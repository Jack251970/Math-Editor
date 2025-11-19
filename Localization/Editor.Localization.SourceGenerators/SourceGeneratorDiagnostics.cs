using Editor.Localization.Shared;
using Microsoft.CodeAnalysis;

namespace Editor.Localization.SourceGenerators
{
    public static class SourceGeneratorDiagnostics
    {
        public static readonly DiagnosticDescriptor CouldNotFindResourceDictionaries = new DiagnosticDescriptor(
            Constants.CouldNotFindResourceDictionariesId,
            "Could not find resource dictionaries",
            "Could not find resource dictionaries. There must be a `en.axaml` file under `Language` folder.",
            "Localization",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor LocalizationKeyUnused = new DiagnosticDescriptor(
            Constants.LocalizationKeyUnusedId,
            "Localization key is unused",
            $"Method `{Constants.ClassName}.{{0}}` is never used",
            "Localization",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor EnumFieldLocalizationKeyValueInvalid = new DiagnosticDescriptor(
            Constants.EnumFieldLocalizationKeyValueInvalidId,
            "Enum field localization key and value invalid",
            $"Enum field `{{0}}` does not have a valid localization key or value",
            "Localization",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );
    }
}
