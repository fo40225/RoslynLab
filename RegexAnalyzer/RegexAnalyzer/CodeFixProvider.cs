﻿using System;
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
using Microsoft.CodeAnalysis.Formatting;

namespace RegexAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RegexAnalyzerCodeFixProvider)), Shared]
    public class RegexAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Fix regex";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(RegexAnalyzerAnalyzer.DiagnosticId);
            }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var invocationExpr = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => FixRegexAsync(context.Document, invocationExpr, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> FixRegexAsync(Document document, InvocationExpressionSyntax invocationExpr, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;
            var memberSymbol = semanticModel.GetSymbolInfo(memberAccessExpr).Symbol as IMethodSymbol;
            var argumentList = invocationExpr.ArgumentList as ArgumentListSyntax;
            var regexLiteral = argumentList.Arguments[1].Expression as LiteralExpressionSyntax;
            var regexOpt = semanticModel.GetConstantValue(regexLiteral);
            var regex = regexOpt.Value as string;

            var newLiteral = SyntaxFactory.ParseExpression("\"valid regex\"")
                                          .WithLeadingTrivia(regexLiteral.GetLeadingTrivia())
                                          .WithTrailingTrivia(regexLiteral.GetTrailingTrivia())
                                          .WithAdditionalAnnotations(Formatter.Annotation);

            var root = await document.GetSyntaxRootAsync();

            var newRoot = root.ReplaceNode(regexLiteral, newLiteral);

            var newDocument = document.WithSyntaxRoot(newRoot);

            return newDocument;
        }
    }
}