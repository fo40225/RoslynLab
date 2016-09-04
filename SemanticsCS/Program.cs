using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SemanticsCS
{
    class Program
    {
        static void Main(string[] args)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(
@"using System;
using System.Collections.Generic;
using System.Text;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello, World!"");
        }
    }
}");

            var root = (CompilationUnitSyntax)tree.GetRoot();
            var compilation = CSharpCompilation.Create("HelloWorld")
                                               .AddReferences(
                                                    MetadataReference.CreateFromFile(
                                                        typeof(object).Assembly.Location))
                                               .AddSyntaxTrees(tree);

            var model = compilation.GetSemanticModel(tree);
            var nameInfo = model.GetSymbolInfo(root.Usings[0].Name);
            var systemSymbol = (INamespaceSymbol)nameInfo.Symbol;

            foreach (var ns in systemSymbol.GetNamespaceMembers())
            {
                Console.WriteLine(ns.Name);
            }

            var helloWorldString = root.DescendantNodes()
                                       .OfType<LiteralExpressionSyntax>()
                                       .First();
            var literalInfo = model.GetTypeInfo(helloWorldString);

            var stringTypeSymbol = (INamedTypeSymbol)literalInfo.Type;

            Console.Clear();

            var methodNames = stringTypeSymbol.GetMembers()
                                              .OfType<IMethodSymbol>()
                                              .Where(m => m.ReturnType.Equals(stringTypeSymbol)
                                                          &&
                                                          m.DeclaredAccessibility == Accessibility.Public)
                                              .Select(m => m.Name)
                                              .Distinct();
            foreach (var name in methodNames)
            {
                Console.WriteLine(name);
            }
        }
    }
}