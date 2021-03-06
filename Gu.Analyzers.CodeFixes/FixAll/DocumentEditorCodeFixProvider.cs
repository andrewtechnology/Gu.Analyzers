namespace Gu.Analyzers
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CodeFixes;

    internal abstract class DocumentEditorCodeFixProvider : CodeFixProvider
    {
        public sealed override FixAllProvider GetFixAllProvider() => this.FixAllProvider();

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context) => this.RegisterCodeFixesAsync(new DocumentEditorCodeFixContext(context));

        protected abstract Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context);

        protected virtual DocumentEditorFixAllProvider FixAllProvider() => DocumentEditorFixAllProvider.Default;
    }
}
