namespace VKApiCodeGen.Generator.Entities
{
    public class CSharpAttribute : ISyntaxEntity
    {
        public CSharpAttribute(string value)
        {
            Value = value;
        }

        public string Value { get; set; }

        public void WriteSyntax(SyntaxBuilder builder)
        {
            builder.Line($"[{Value}]");
        }
    }
}
