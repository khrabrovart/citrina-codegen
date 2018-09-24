using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CitrinaCodeGeneration
{
    public class Program
    {
        private const string TestFieldName = "_testField";
        private const string TestPropertyName = "TestField";

        private const string OtherFieldName = "_otherField";
        private const string OtherPropertyName = "OtherField";

        public static void Main(string[] args)
        {
            var workspace = new AdhocWorkspace();
            var generator = SyntaxGenerator.GetGenerator(workspace, LanguageNames.CSharp);

            var usingDirectives = generator.NamespaceImportDeclaration("System");

            var lastNameField = generator.FieldDeclaration(TestFieldName,
                generator.TypeExpression(SpecialType.System_String),
                Accessibility.Private);

            var firstNameField = generator.FieldDeclaration(OtherFieldName,
                generator.TypeExpression(SpecialType.System_String),
                Accessibility.Private);



            var lastNameProperty = generator.PropertyDeclaration(TestPropertyName,
                generator.TypeExpression(SpecialType.System_String), Accessibility.Public,
                getAccessorStatements: new SyntaxNode[]
                { generator.ReturnStatement(generator.IdentifierName(TestFieldName)) },
                setAccessorStatements: new SyntaxNode[]
                { generator.AssignmentStatement(generator.IdentifierName(TestFieldName),
                generator.IdentifierName("value"))});

            var firstNameProperty = generator.PropertyDeclaration(OtherPropertyName,
                generator.TypeExpression(SpecialType.System_String),
                Accessibility.Public,
                getAccessorStatements: new SyntaxNode[]
                { generator.ReturnStatement(generator.IdentifierName(OtherFieldName)) },
                setAccessorStatements: new SyntaxNode[]
                { generator.AssignmentStatement(generator.IdentifierName(OtherFieldName),
                generator.IdentifierName("value")) });

            var cloneMethodBody = generator.ReturnStatement(generator.
                InvocationExpression(generator.IdentifierName("MemberwiseClone")));

            var cloneMethoDeclaration = generator.MethodDeclaration("Clone", null,
                null, null,
                Accessibility.Public,
                DeclarationModifiers.Virtual,
                new SyntaxNode[] { cloneMethodBody });

            var ICloneableInterfaceType = generator.IdentifierName("ICloneable");

            var cloneMethodWithInterfaceType = generator
                .AsPublicInterfaceImplementation(
                    cloneMethoDeclaration,
                    ICloneableInterfaceType);

            var constructorParameters = new SyntaxNode[]
            {
                generator.ParameterDeclaration("testValue",
                generator.TypeExpression(SpecialType.System_String)),
                generator.ParameterDeclaration("otherValue",
                generator.TypeExpression(SpecialType.System_String))
            };

            var constructorBody = new SyntaxNode[]
            {
                generator.AssignmentStatement(generator.IdentifierName(TestFieldName),
                generator.IdentifierName("testValue")),
                generator.AssignmentStatement(generator.IdentifierName(OtherFieldName),
                generator.IdentifierName("otherValue"))
            };

            var constructor = generator.ConstructorDeclaration("Person",
                constructorParameters, Accessibility.Public,
                statements: constructorBody);

            var newAutoProperty = GenerateAutoProperty("Test", "int?");

            var members = new SyntaxNode[]
            {
                lastNameField,
                firstNameField,
                lastNameProperty,
                firstNameProperty,
                newAutoProperty,
                cloneMethodWithInterfaceType,
                constructor
            };

            var classDefinition = generator.ClassDeclaration(
                "Person", typeParameters: null,
                accessibility: Accessibility.Public,
                modifiers: DeclarationModifiers.Abstract,
                baseType: null,
                interfaceTypes: new SyntaxNode[] { ICloneableInterfaceType },
                members: members);

            var namespaceDeclaration = generator.NamespaceDeclaration("MyTypes", classDefinition);

            var newNode = generator.CompilationUnit(usingDirectives, namespaceDeclaration)
                .NormalizeWhitespace();
                //.NormalizeWhitespace(indentation: string.Empty, eol: string.Empty);

            Console.WriteLine(newNode.GetText());
            Console.ReadKey();
        }

        public static PropertyDeclarationSyntax GenerateAutoProperty(string name, string typeName)
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
