using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Editor.Localization.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Editor.Localization.Analyzers.Localize
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OldGetTranslateAnalyzerCodeFixProvider)), Shared]
    public class OldGetTranslateAnalyzerCodeFixProvider : CodeFixProvider
    {
        #region CodeFixProvider

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
            Constants.OldLocalizationApiUsedId
        );

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: $"Replace with '{Constants.ClassName}.localization_key(...args)'",
                    createChangedDocument: _ => FixOldTranslationAsync(context, root, diagnostic),
                    equivalenceKey: Constants.OldLocalizationApiUsedId
                ),
                diagnostic
            );
        }

        #endregion

        #region Fix Methods

        private static async Task<Document> FixOldTranslationAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
        {
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            if (root is null) return context.Document;

            var invocationExpr = root
                .FindToken(diagnosticSpan.Start).Parent
                .AncestorsAndSelf()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault();

            if (invocationExpr is null) return context.Document;

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            if (semanticModel == null) return context.Document;

            var symbolInfo = semanticModel.GetSymbolInfo(invocationExpr, context.CancellationToken);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;

            // Case A: string.Format(..., Internationalization.GetTranslation("key"), ...)
            if (IsFormatStringCall(methodSymbol))
            {
                var argumentList = invocationExpr.ArgumentList.Arguments;
                for (var i = 0; i < argumentList.Count; i++)
                {
                    if (argumentList[i].Expression is InvocationExpressionSyntax innerInvocation)
                    {
                        var innerSymbol = semanticModel.GetSymbolInfo(innerInvocation, context.CancellationToken).Symbol as IMethodSymbol;
                        if (IsTranslateCall(innerSymbol))
                        {
                            var translationKey = GetFirstArgumentStringValue(innerInvocation);
                            if (translationKey == null) continue;

                            // New args are all arguments after the translation call in the format call
                            var newArguments = string.Join(
                                ", ",
                                argumentList.Skip(i + 1).Select(a => a.Expression.ToString())
                            );

                            var newInvocationExpr = string.IsNullOrWhiteSpace(newArguments)
                                ? SyntaxFactory.ParseExpression($"{Constants.ClassName}.{translationKey}()")
                                : SyntaxFactory.ParseExpression($"{Constants.ClassName}.{translationKey}({newArguments})");

                            var newRoot = root.ReplaceNode(invocationExpr, newInvocationExpr);
                            return context.Document.WithSyntaxRoot(newRoot);
                        }
                    }
                }
            }

            // Case B: Internationalization.GetTranslation("key", args...)
            else if (IsTranslateCall(methodSymbol))
            {
                var args = invocationExpr.ArgumentList.Arguments;
                var firstArgExpr = args.FirstOrDefault()?.Expression;
                if (!(firstArgExpr is LiteralExpressionSyntax literal) || !(literal.Token.Value is string translationKey))
                    return context.Document;

                var remainingArgs = args.Skip(1).Select(a => a.Expression.ToString());
                var remainingArgsJoined = string.Join(", ", remainingArgs);

                var replacement = string.IsNullOrWhiteSpace(remainingArgsJoined)
                    ? $"{Constants.ClassName}.{translationKey}()"
                    : $"{Constants.ClassName}.{translationKey}({remainingArgsJoined})";

                var newInvocationExpr2 = SyntaxFactory.ParseExpression(replacement);
                var newRoot2 = root.ReplaceNode(invocationExpr, newInvocationExpr2);
                return context.Document.WithSyntaxRoot(newRoot2);
            }

            // Fallback: leave unchanged if not recognized
            return context.Document;
        }

        #region Utils

        private static string GetFirstArgumentStringValue(InvocationExpressionSyntax invocationExpr)
        {
            if (invocationExpr.ArgumentList.Arguments.FirstOrDefault()?.Expression is LiteralExpressionSyntax syntax)
                return syntax.Token.ValueText;
            return null;
        }

        private static bool IsFormatStringCall(IMethodSymbol methodSymbol)
        {
            return methodSymbol?.Name == Constants.StringFormatMethodName &&
                   methodSymbol.ContainingType.ToDisplayString() == Constants.StringFormatTypeName;
        }

        private static bool IsTranslateCall(IMethodSymbol methodSymbol)
        {
            return methodSymbol?.Name == Constants.OldLocalizationMethodName &&
                   methodSymbol.ContainingType != null &&
                   Constants.OldLocalizationClasses.Contains(methodSymbol.ContainingType.Name);
        }

        #endregion

        #endregion
    }
}
