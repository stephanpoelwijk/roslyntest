using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace roslyntest.Collectors
{
    internal class ConstructorCollector : CSharpSyntaxWalker
    {
        internal readonly List<ConstructorDeclarationSyntax> constructorDeclarations = new List<ConstructorDeclarationSyntax>();

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            constructorDeclarations.Add(node);
        }
    }
}