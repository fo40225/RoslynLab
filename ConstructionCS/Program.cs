using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ConstructionCS
{
    class Program
    {
        static void Main(string[] args)
        {
            NameSyntax name = IdentifierName("System");
            name = QualifiedName(name, IdentifierName("Collections"));
            name = QualifiedName(name, IdentifierName("Generic"));

            SyntaxTree tree = CSharpSyntaxTree.ParseText(
@"using System;
using System.Collections;
using System.Linq;
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
            var oldUsing = root.Usings[1];
            var newUsing = oldUsing.WithName(name);
            root = root.ReplaceNode(oldUsing, newUsing);
            Console.WriteLine(root.ToString());
        }
    }
}
