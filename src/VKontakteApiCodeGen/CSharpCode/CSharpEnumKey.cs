namespace VKontakteApiCodeGen.CSharpCode
{
    public class CSharpEnumKey
    {
        public CSharpEnumKey(string name) : this(name, null)
        {
        }

        public CSharpEnumKey(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}
