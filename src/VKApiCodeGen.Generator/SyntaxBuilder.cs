using System;
using System.Text;

namespace VKApiCodeGen.Generator
{
    public class SyntaxBuilder
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private int _indent;

        public string SourceCode => _sb.ToString();

        public void Clear() => _sb.Clear();

        public void Line() => _sb.AppendLine();

        public void Line(string text) => _sb.AppendLine(Tabs(Math.Max(_indent, 0)) + text);

        public void Block(Action inBlock, bool insertSemicolon = false)
        {
            Line("{");
            IncreaseIndent();

            inBlock?.Invoke();

            DecreaseIndent();
            Line(insertSemicolon ? "};" : "}");
        }

        private void IncreaseIndent() => _indent++;

        private void DecreaseIndent() => _indent--;

        private static string Tabs(int count) => new string(' ', 4 * count);
    }
}
