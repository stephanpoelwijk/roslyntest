using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using roslyntest.Collectors;
using System;
using System.IO;
using System.Linq;

namespace roslyntest
{
    class Program
    {
        static void Main(string[] args)
        {

            var classContents = File.ReadAllText(@"C:\Users\spoel\source\playground\roslyntest\Models\SomeDomainObject.cs");
            var tree = CSharpSyntaxTree.ParseText(classContents);
            var sourceRoot = (CompilationUnitSyntax)tree.GetRoot();

            var classCollector = new ClassCollector();
            classCollector.Visit(sourceRoot);

            foreach (var classDeclaration in classCollector.classDeclarations)
            {
                var namespaceDeclarationSyntax = classDeclaration.Parent as NamespaceDeclarationSyntax;
                if (namespaceDeclarationSyntax == null)
                {
                    continue;
                }

                var testDataBuilderTypeName = $"{classDeclaration.Identifier.Text}TestDataBuilder";

                Console.WriteLine($"Generating {testDataBuilderTypeName}");

                var testDataBuilderClass = SyntaxFactory.ClassDeclaration(testDataBuilderTypeName);
                testDataBuilderClass = testDataBuilderClass.AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

                foreach (ConstructorDeclarationSyntax constructorDeclaration in classDeclaration.ChildNodes().Where(c => c.GetType() == typeof(ConstructorDeclarationSyntax)))
                {
                    testDataBuilderClass = AddPrivateFields(testDataBuilderClass, constructorDeclaration);
                    testDataBuilderClass = AddBuildMethod(classDeclaration.Identifier.Text, testDataBuilderClass, constructorDeclaration);
                    testDataBuilderClass = AddBuilderMethods(testDataBuilderTypeName, testDataBuilderClass, constructorDeclaration);
                }

                sourceRoot = sourceRoot.ReplaceNode(namespaceDeclarationSyntax, namespaceDeclarationSyntax.AddMembers(testDataBuilderClass));
            }

            Console.WriteLine(sourceRoot.NormalizeWhitespace().ToFullString());
        }

        private static string GetPrivateFieldName(SyntaxToken identifier)
        {
            return $"_{identifier.Text}";
        }

        private static string GetBuilderMethodName(SyntaxToken identifier)
        {
            return $"With{char.ToUpper(identifier.Text[0])}{identifier.Text.Substring(1)}";
        }

        private static ClassDeclarationSyntax AddBuildMethod(string returnType, ClassDeclarationSyntax testDataBuilderClass, ConstructorDeclarationSyntax constructorDeclaration)
        {
            var argumentList = SyntaxFactory.ArgumentList();
            foreach (var parameter in constructorDeclaration.ParameterList.Parameters)
            {
                argumentList = argumentList.AddArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(GetPrivateFieldName(parameter.Identifier))));
            }

            var newExpression = SyntaxFactory
                .ObjectCreationExpression(SyntaxFactory.ParseTypeName(returnType))
                .WithArgumentList(argumentList);

            var returnStatement = SyntaxFactory.ReturnStatement(newExpression);

            var buildMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(returnType), "Build")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(SyntaxFactory.Block(returnStatement));

            testDataBuilderClass = testDataBuilderClass.AddMembers(buildMethod);

            return testDataBuilderClass;
        }

        private static ClassDeclarationSyntax AddPrivateFields(ClassDeclarationSyntax classDeclaration, ConstructorDeclarationSyntax constructorDeclarationSyntax)
        {
            foreach (var parameter in constructorDeclarationSyntax.ParameterList.Parameters)
            {
                var variableDeclaration = SyntaxFactory.VariableDeclaration(parameter.Type)
                    .AddVariables(SyntaxFactory.VariableDeclarator(GetPrivateFieldName(parameter.Identifier)));

                var fieldDeclaration = SyntaxFactory.FieldDeclaration(variableDeclaration)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

                classDeclaration = classDeclaration.AddMembers(fieldDeclaration);
            }

            return classDeclaration;
        }

        private static ClassDeclarationSyntax AddBuilderMethods(string testDataBuilderTypeName, ClassDeclarationSyntax classDeclaration, ConstructorDeclarationSyntax constructorDeclarationSyntax)
        {
            foreach (var parameter in constructorDeclarationSyntax.ParameterList.Parameters)
            {
                var methodParameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameter.Identifier.Text)).WithType(parameter.Type);

                var methodBody = SyntaxFactory.ParseStatement($"{GetPrivateFieldName(parameter.Identifier)} = {parameter.Identifier.Text};\r\nreturn this;");

                var methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(testDataBuilderTypeName), GetBuilderMethodName(parameter.Identifier))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddParameterListParameters(parameter)
                    .WithBody(SyntaxFactory.Block(methodBody));

                classDeclaration = classDeclaration.AddMembers(methodDeclaration);
            }

            return classDeclaration;
        }
    }
}
