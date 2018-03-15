namespace Gu.Analyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseParameterCodeFixProvider))]
    [Shared]
    internal class UseParameterCodeFixProvider : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(GU0014PreferParameter.DiagnosticId);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Properties.TryGetValue("Name", out var name) &&
                    syntaxRoot.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) is IdentifierNameSyntax identiferName)
                {
                    if (identiferName.Parent is MemberAccessExpressionSyntax memberAccess)
                    {
                        context.RegisterCodeFix(
                            "Prefer parameter.",
                            (editor, _) => editor.ReplaceNode(
                                memberAccess,
                                SyntaxFactory.IdentifierName(name)),
                            "Prefer parameter.",
                            diagnostic);
                    }
                    else
                    {
                        context.RegisterCodeFix(
                            "Prefer parameter.",
                            (editor, _) => editor.ReplaceNode(
                                identiferName,
                                identiferName.WithIdentifier(SyntaxFactory.Identifier(name))),
                            "Prefer parameter.",
                            diagnostic);
                    }
                }
            }
        }
    }
}