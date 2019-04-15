using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CitrinaCodeGeneration
{
    public class CodeBuilder
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private int _indent = 0;

        public string Code => _sb.ToString();

        public void IncreaseIndent() => _indent++;

        public void DecreaseIndent() => _indent--;

        public void Line() => _sb.AppendLine();

        public void Line(string text) => _sb.AppendLine(Tabs(Math.Max(_indent, 0)) + text);

        public void Block(Action inBlock)
        {
            Line("{");
            IncreaseIndent();

            inBlock?.Invoke();

            DecreaseIndent();
            Line("}");
        }

        public void IterableBlock<T>(T[] collection, Action<T> action, bool separate)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            Block(() =>
            {
                if (collection == null)
                {
                    return;
                }

                for (int i = 0; i < collection.Length; i++)
                {
                    action(collection[i]);

                    if (separate && i < collection.Length - 1)
                    {
                        Line();
                    }
                }
            });
        }

        private static string Tabs(int count) => new string(' ', 4 * count);
    }
}
