using System.Text;

namespace CitrinaCodeGeneration
{
    public class CodeBuilder
    {
        private readonly StringBuilder _sb;

        public CodeBuilder()
        {
            _sb = new StringBuilder();
        }

        public void Line(string text, int indent = 0)
        {
            _sb.AppendLine(Tabs(indent) + text);
        }

        public string Code => _sb.ToString();

        private static string Tabs(int count) => new string(' ', 4 * count);
    }
}
