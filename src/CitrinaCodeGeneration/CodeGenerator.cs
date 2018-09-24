using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CitrinaCodeGeneration
{
    internal class CodeGenerator
    {
        private static readonly AdhocWorkspace workspace;
        private static readonly SyntaxGenerator generator;

        static CodeGenerator()
        {
            workspace = new AdhocWorkspace();
            generator = SyntaxGenerator.GetGenerator(workspace, LanguageNames.CSharp);
        }

        public SyntaxNode NewProperty(string name, string typeName)
        {
            return GenerateAutoProperty(name, typeName);
        }

        public SyntaxNode NewClass(string name, SyntaxNode baseType, params SyntaxNode[] members)
        {
            return generator.ClassDeclaration(
                name, typeParameters: null,
                accessibility: Accessibility.Public,
                baseType: null,
                members: members);
        }

        public SyntaxNode NewNamespace(string name, params SyntaxNode[] classNodes)
        {
            return generator.NamespaceDeclaration(name, classNodes);
        }

        public SyntaxNode NewNamespace(string name, SyntaxNode classNode)
        {
            return generator.NamespaceDeclaration(name, classNode);
        }

        public void NewSourceFile(string fileName, SyntaxNode namespaceNode, IEnumerable<string> usingNames)
        {
            var nodes = usingNames.Select(un => generator.NamespaceImportDeclaration(un)).ToList();
            nodes.Add(namespaceNode);
            generator.CompilationUnit(nodes).WriteFormattedSourceFile(fileName);
        }

        private static PropertyDeclarationSyntax GenerateAutoProperty(string name, string typeName)
        {
            return SF.PropertyDeclaration(SF.ParseTypeName(typeName), SF.Identifier(name))
                .AddModifiers(SF.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SF.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken)),
                    SF.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken)));
        }
    }
}
