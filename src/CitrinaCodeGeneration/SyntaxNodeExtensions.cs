using System.IO;
using Microsoft.CodeAnalysis;

namespace CitrinaCodeGeneration
{
    internal static class SyntaxNodeExtensions
    {
        public static SyntaxNode GetFormattedCode(this SyntaxNode node)
        {
            return node.NormalizeWhitespace();
        }

        public static SyntaxNode GetCompactCode(this SyntaxNode node)
        {
            return node.NormalizeWhitespace(indentation: string.Empty, eol: string.Empty);
        }

        public static void WriteFormattedSourceFile(this SyntaxNode node, string filePath)
        {
            File.WriteAllText(filePath, node.GetFormattedCode().ToString());
        }

        public static void WriteCompactSourceFile(this SyntaxNode node, string filePath)
        {
            File.WriteAllText(filePath, node.GetCompactCode().ToString());
        }
    }
}
