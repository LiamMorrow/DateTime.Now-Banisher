using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System.Text.RegularExpressions;

namespace DateTimeNow.Now.Banisher
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisableDateTimeNowCodeFixProvider)), Shared]
    public class DisableDateTimeOffsetNowCodeFixProvider : CodeFixProvider
    {
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.DateTimeOffsetAnalyzerTitle), Resources.ResourceManager, typeof(Resources));

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DisableDateTimeNowAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            
            return Task.Run(()=>context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title.ToString(),
                    createChangedDocument: c => ReplaceWithUtcNowAsync(context.Document, diagnosticSpan, c),
                    equivalenceKey: Title.ToString()),
                diagnostic));
        }

        private async Task<Document> ReplaceWithUtcNowAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            var text = await document.GetTextAsync();
            var repl = "DateTimeOffset.UtcNow";
            if (Regex.Replace(text.GetSubText(span).ToString(),@"\s+",string.Empty) == "System.DateTimeOffset.Now")
                repl = "System.DateTimeOffset.UtcNow";
            var newtext = text.Replace(span, repl);
            return document.WithText(newtext);
        }
    }
}