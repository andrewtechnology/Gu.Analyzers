﻿namespace Gu.Analyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class GU0006UseNameof : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GU0006";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use nameof.",
            messageFormat: "Use nameof.",
            category: AnalyzerCategory.Correctness,
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Use nameof.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleArguments, SyntaxKind.Argument);
        }

        private static void HandleArguments(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is ArgumentSyntax argument &&
                argument.Expression is LiteralExpressionSyntax literal &&
                literal.IsKind(SyntaxKind.StringLiteralExpression) &&
                SyntaxFacts.IsValidIdentifier(literal.Token.ValueText))
            {
                var symbols = context.SemanticModel.LookupSymbols(argument.SpanStart, name: literal.Token.ValueText);
                if (symbols.TrySingle(x => x.Name == literal.Token.ValueText, out var symbol))
                {
                    if (symbol is IParameterSymbol ||
                        symbol is ILocalSymbol ||
                        symbol is IFieldSymbol ||
                        symbol is IEventSymbol ||
                        symbol is IPropertySymbol ||
                        symbol is IMethodSymbol)
                    {
                        if (symbol is ILocalSymbol local)
                        {
                            if (local.DeclaringSyntaxReferences.TrySingle(out var reference))
                            {
                                var statement = argument.FirstAncestor<StatementSyntax>();
                                if (statement.Span.Start < reference.Span.Start)
                                {
                                    return;
                                }
                            }
                            else
                            {
                                return;
                            }
                        }

                        if (symbol is IParameterSymbol ||
                            symbol is ILocalSymbol ||
                            symbol.IsStatic ||
                            context.ContainingSymbol.IsStatic)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptor, argument.GetLocation()));
                            return;
                        }

                        if (symbol.ContainingType == context.ContainingSymbol.ContainingType)
                        {
                            var properties = ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string>("member", symbol.Name) });
                            context.ReportDiagnostic(Diagnostic.Create(Descriptor, argument.GetLocation(), properties));
                        }
                    }
                }
            }
        }
    }
}