using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Editor.Localization.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Editor.Localization.SourceGenerators.Localize
{
    [Generator]
    public partial class EnumSourceGenerator : IIncrementalGenerator
    {
        #region Fields

        private static readonly Version PackageVersion = typeof(EnumSourceGenerator).Assembly.GetName().Version;

        private static readonly ImmutableArray<EnumField> _emptyEnumFields = ImmutableArray<EnumField>.Empty;

        #endregion

        #region Incremental Generator

        /// <summary>
        /// Initializes the generator and registers source output based on enum declarations.
        /// </summary>
        /// <param name="context">The initialization context.</param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var enumDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (s, _) => s is EnumDeclarationSyntax,
                    transform: (ctx, _) => (EnumDeclarationSyntax)ctx.Node)
                .Where(ed => ed.AttributeLists.Count > 0)
                .Collect();

            var compilation = context.CompilationProvider;

            var configOptions = context.AnalyzerConfigOptionsProvider;

            var compilationEnums = enumDeclarations.Combine(configOptions).Combine(compilation);

            context.RegisterSourceOutput(compilationEnums, Execute);
        }

        /// <summary>
        /// Executes the generation of enum data classes based on the provided data.
        /// </summary>
        /// <param name="spc">The source production context.</param>
        /// <param name="data">The provided data.</param>
        private void Execute(SourceProductionContext spc,
            ((ImmutableArray<EnumDeclarationSyntax> EnumsDeclarations,
            AnalyzerConfigOptionsProvider ConfigOptionsProvider),
            Compilation Compilation) data)
        {
            var compilation = data.Compilation;
            var configOptions = data.Item1.ConfigOptionsProvider;
            var enumsDeclarations = data.Item1.EnumsDeclarations;

            var assemblyNamespace = compilation.AssemblyName ?? Constants.DefaultNamespace;

            foreach (var enumDeclaration in enumsDeclarations.Distinct())
            {
                var semanticModel = compilation.GetSemanticModel(enumDeclaration.SyntaxTree);
                var enumSymbol = semanticModel.GetDeclaredSymbol(enumDeclaration) as INamedTypeSymbol;

                // Check if the enum has the EnumLocalize attribute
                if (enumSymbol?.GetAttributes().Any(ad =>
                    ad.AttributeClass?.Name == Constants.EnumLocalizeAttributeName) ?? false)
                {
                    GenerateSource(spc, enumSymbol, assemblyNamespace);
                }
            }
        }

        #endregion

        #region Get Enum Fields

        private static ImmutableArray<EnumField> GetEnumFields(SourceProductionContext spc, INamedTypeSymbol enumSymbol, string enumFullName)
        {
            // Iterate through enum members and get enum fields
            var enumFields = new List<EnumField>();
            var enumError = false;
            foreach (var member in enumSymbol.GetMembers().Where(m => m.Kind == SymbolKind.Field))
            {
                if (member is IFieldSymbol fieldSymbol)
                {
                    var enumFieldName = fieldSymbol.Name;

                    // Check if the field has the EnumLocalizeKey attribute
                    var keyAttr = fieldSymbol.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass?.Name == Constants.EnumLocalizeKeyAttributeName);
                    var keyAttrExist = keyAttr != null;

                    // Check if the field has the EnumLocalizeValue attribute
                    var valueAttr = fieldSymbol.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass?.Name == Constants.EnumLocalizeValueAttributeName);
                    var valueAttrExist = valueAttr != null;

                    // Get the key and value from the attributes
                    var key = keyAttr?.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? string.Empty;
                    var value = valueAttr?.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? string.Empty;

                    // Users may use "  " as a key, so we need to check if the key is not empty and not whitespace
                    if (keyAttrExist && !string.IsNullOrWhiteSpace(key))
                    {
                        // If localization key exists and is valid, use it
                        enumFields.Add(new EnumField(enumFieldName, key, valueAttrExist ? value : null));
                    }
                    else if (valueAttrExist)
                    {
                        // If localization value exists, use it
                        enumFields.Add(new EnumField(enumFieldName, value));
                    }
                    else
                    {
                        // If localization key and value are not provided, do not generate the field and report a diagnostic
                        spc.ReportDiagnostic(Diagnostic.Create(
                            SourceGeneratorDiagnostics.EnumFieldLocalizationKeyValueInvalid,
                            Location.None,
                            $"{enumFullName}.{enumFieldName}"));
                        enumError = true;
                    }
                }
            }

            // If there was an error, do not generate the class
            if (enumError) return _emptyEnumFields;

            return enumFields.ToImmutableArray();
        }

        #endregion

        #region Generate Source

        private void GenerateSource(
            SourceProductionContext spc,
            INamedTypeSymbol enumSymbol,
            string assemblyNamespace)
        {
            var enumFullName = enumSymbol.ToDisplayString(new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted, // Remove global:: symbol
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
            var enumDataClassName = $"{enumSymbol.Name}{Constants.EnumLocalizeClassSuffix}";
            var enumName = enumSymbol.Name;
            var enumNamespace = enumSymbol.ContainingNamespace.ToDisplayString();
            var tabString = Helper.Spacing(1);

            var sourceBuilder = new StringBuilder();

            // Generate header
            GeneratedHeaderFromPath(sourceBuilder, enumFullName);
            sourceBuilder.AppendLine();

            // Generate nullable enable
            sourceBuilder.AppendLine("#nullable enable");
            sourceBuilder.AppendLine();

            // Generate namespace
            sourceBuilder.AppendLine($"namespace {enumNamespace};");
            sourceBuilder.AppendLine();

            // Suppress warning for old localization API usage
            sourceBuilder.AppendLine(Constants.SuppressWarning);
            sourceBuilder.AppendLine();

            // Generate class
            sourceBuilder.AppendLine($"/// <summary>");
            sourceBuilder.AppendLine($"/// Data class for <see cref=\"{enumFullName}\"/>");
            sourceBuilder.AppendLine($"/// </summary>");
            sourceBuilder.AppendLine($"[System.CodeDom.Compiler.GeneratedCode(\"{nameof(EnumSourceGenerator)}\", \"{PackageVersion}\")]");
            sourceBuilder.AppendLine($"public class {enumDataClassName} : global::System.ComponentModel.INotifyPropertyChanged");
            sourceBuilder.AppendLine("{");

            // Generate properties
            sourceBuilder.AppendLine($"{tabString}/// <summary>");
            sourceBuilder.AppendLine($"{tabString}/// The value of the enum");
            sourceBuilder.AppendLine($"{tabString}/// </summary>");
            sourceBuilder.AppendLine($"{tabString}public {enumName} Value {{ get; private init; }}");
            sourceBuilder.AppendLine();

            sourceBuilder.AppendLine($"{tabString}private string? _display;");
            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine($"{tabString}/// <summary>");
            sourceBuilder.AppendLine($"{tabString}/// The display text of the enum value");
            sourceBuilder.AppendLine($"{tabString}/// </summary>");
            sourceBuilder.AppendLine($"{tabString}public string? Display");
            sourceBuilder.AppendLine($"{tabString}{{");
            sourceBuilder.AppendLine($"{tabString}{tabString}get => _display;");
            sourceBuilder.AppendLine($"{tabString}{tabString}set");
            sourceBuilder.AppendLine($"{tabString}{tabString}{{");
            sourceBuilder.AppendLine($"{tabString}{tabString}{tabString}if (_display != value)");
            sourceBuilder.AppendLine($"{tabString}{tabString}{tabString}{{");
            sourceBuilder.AppendLine($"{tabString}{tabString}{tabString}{tabString}_display = value;");
            sourceBuilder.AppendLine($"{tabString}{tabString}{tabString}{tabString}OnPropertyChanged(nameof(Display));");
            sourceBuilder.AppendLine($"{tabString}{tabString}{tabString}}}");
            sourceBuilder.AppendLine($"{tabString}{tabString}}}");
            sourceBuilder.AppendLine($"{tabString}}}");
            sourceBuilder.AppendLine();

            sourceBuilder.AppendLine($"{tabString}/// <summary>");
            sourceBuilder.AppendLine($"{tabString}/// The localization key of the enum value");
            sourceBuilder.AppendLine($"{tabString}/// </summary>");
            sourceBuilder.AppendLine($"{tabString}public string? LocalizationKey {{ get; set; }}");
            sourceBuilder.AppendLine();

            sourceBuilder.AppendLine($"{tabString}/// <summary>");
            sourceBuilder.AppendLine($"{tabString}/// The localization value of the enum value");
            sourceBuilder.AppendLine($"{tabString}/// </summary>");
            sourceBuilder.AppendLine($"{tabString}public string? LocalizationValue {{ get; set; }}");
            sourceBuilder.AppendLine();

            // Use instance from App.Internationalization
            var getTranslation = $"{assemblyNamespace}.{Constants.Internationalization}.GetTranslation";

            // Generate GetValues method
            sourceBuilder.AppendLine($"{tabString}/// <summary>");
            sourceBuilder.AppendLine($"{tabString}/// Get all values of <see cref=\"{enumFullName}\"/>");
            sourceBuilder.AppendLine($"{tabString}/// </summary>");
            sourceBuilder.AppendLine($"{tabString}public static global::System.Collections.Generic.List<{enumDataClassName}> GetValues()");
            sourceBuilder.AppendLine($"{tabString}{{");
            sourceBuilder.AppendLine($"{tabString}{tabString}return new global::System.Collections.Generic.List<{enumDataClassName}>");
            sourceBuilder.AppendLine($"{tabString}{tabString}{{");
            var enumFields = GetEnumFields(spc, enumSymbol, enumFullName);
            if (enumFields.Length == 0) return;
            foreach (var enumField in enumFields)
            {
                GenerateEnumField(sourceBuilder, getTranslation, enumField, enumName, tabString);
            }
            sourceBuilder.AppendLine($"{tabString}{tabString}}};");
            sourceBuilder.AppendLine($"{tabString}}}");
            sourceBuilder.AppendLine();

            // Generate UpdateLabels method
            GenerateUpdateLabelsMethod(sourceBuilder, getTranslation, enumDataClassName, tabString);
            sourceBuilder.AppendLine();

            // Generate INotifyPropertyChanged implementation
            sourceBuilder.AppendLine($"{tabString}/// <inheritdoc />");
            sourceBuilder.AppendLine($"{tabString}public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;");
            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine($"{tabString}protected void OnPropertyChanged([global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)");
            sourceBuilder.AppendLine($"{tabString}{{");
            sourceBuilder.AppendLine($"{tabString}{tabString}PropertyChanged?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(propertyName));");
            sourceBuilder.AppendLine($"{tabString}}}");

            sourceBuilder.AppendLine($"}}");

            // Add source to context
            spc.AddSource($"{Constants.ClassName}.{assemblyNamespace}.{enumNamespace}.{enumDataClassName}.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        private static void GeneratedHeaderFromPath(StringBuilder sb, string enumFullName)
        {
            if (string.IsNullOrEmpty(enumFullName))
            {
                sb.AppendLine("/// <auto-generated/>");
            }
            else
            {
                sb.AppendLine("/// <auto-generated>")
                    .AppendLine($"/// From: {enumFullName}")
                    .AppendLine("/// </auto-generated>");
            }
        }

        private static void GenerateEnumField(
            StringBuilder sb,
            string getTranslation,
            EnumField enumField,
            string enumName,
            string tabString)
        {
            sb.AppendLine($"{tabString}{tabString}{tabString}new()");
            sb.AppendLine($"{tabString}{tabString}{tabString}{{");
            sb.AppendLine($"{tabString}{tabString}{tabString}{tabString}Value = {enumName}.{enumField.EnumFieldName},");
            if (enumField.UseLocalizationKey)
            {
                sb.AppendLine($"{tabString}{tabString}{tabString}{tabString}Display = {getTranslation}(\"{enumField.LocalizationKey}\"),");
                sb.AppendLine($"{tabString}{tabString}{tabString}{tabString}LocalizationKey = \"{enumField.LocalizationKey}\",");
            }
            else
            {
                sb.AppendLine($"{tabString}{tabString}{tabString}{tabString}Display = \"{enumField.LocalizationValue}\",");
            }
            if (enumField.LocalizationValue != null)
            {
                sb.AppendLine($"{tabString}{tabString}{tabString}{tabString}LocalizationValue = \"{enumField.LocalizationValue}\",");
            }
            sb.AppendLine($"{tabString}{tabString}{tabString}}},");
        }

        private static void GenerateUpdateLabelsMethod(
            StringBuilder sb,
            string getTranslation,
            string enumDataClassName,
            string tabString)
        {
            sb.AppendLine($"{tabString}/// <summary>");
            sb.AppendLine($"{tabString}/// Update the labels of the enum values when culture info changes.");
            sb.AppendLine($"{tabString}/// </summary>");
            sb.AppendLine($"{tabString}public static void UpdateLabels(global::System.Collections.Generic.List<{enumDataClassName}> options)");
            sb.AppendLine($"{tabString}{{");
            sb.AppendLine($"{tabString}{tabString}foreach (var item in options)");
            sb.AppendLine($"{tabString}{tabString}{{");
            // Users may use "  " as a key, so we need to check if the key is not empty and not whitespace
            sb.AppendLine($"{tabString}{tabString}{tabString}if (!string.IsNullOrWhiteSpace(item.LocalizationKey))");
            sb.AppendLine($"{tabString}{tabString}{tabString}{{");
            sb.AppendLine($"{tabString}{tabString}{tabString}{tabString}item.Display = {getTranslation}(item.LocalizationKey);");
            sb.AppendLine($"{tabString}{tabString}{tabString}}}");
            sb.AppendLine($"{tabString}{tabString}}}");
            sb.AppendLine($"{tabString}}}");
        }

        #endregion

        #region Classes

        public class EnumField
        {
            public string EnumFieldName { get; set; }
            public string LocalizationKey { get; set; }
            public string LocalizationValue { get; set; }

            // Users may use "  " as a key, so we need to check if the key is not empty and not whitespace
            public bool UseLocalizationKey => !string.IsNullOrWhiteSpace(LocalizationKey);

            public EnumField(string enumFieldName, string localizationValue) : this(enumFieldName, null, localizationValue)
            {
            }

            public EnumField(string enumFieldName, string localizationKey, string localizationValue)
            {
                EnumFieldName = enumFieldName;
                LocalizationKey = localizationKey;
                LocalizationValue = localizationValue;
            }
        }

        #endregion
    }
}
