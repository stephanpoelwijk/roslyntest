using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace roslyntest.Collectors
{
    internal class ClassCollector : CSharpSyntaxWalker
    {
        internal readonly List<ClassDeclarationSyntax> classDeclarations = new List<ClassDeclarationSyntax>();

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            classDeclarations.Add(node);
        }
    }
}