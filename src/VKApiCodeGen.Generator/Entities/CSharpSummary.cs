namespace VKApiCodeGen.Generator.Entities
{
    public class CSharpSummary : ISyntaxEntity
    {
        public CSharpSummary(string text)
        {
            Text = text;
        }

        public string Text { get; set; }

        public void WriteSyntax(SyntaxBuilder builder)
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                return;
            }

            var dot = Text.EndsWith('.') ? string.Empty : ".";

            builder.Line("/// <summary>");
            builder.Line($"/// {Text}{dot}");
            builder.Line("/// </summary>");
        }
    }
}
