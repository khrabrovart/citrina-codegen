using System.Collections.Generic;

namespace CitrinaCodeGeneration.CSharpCode
{
    public class CSharpProperty
    {
        public string Name { get; set; }

        public string Summary { get; set; }

        public string Type { get; set; }

        public IEnumerable<string> Attributes { get; set; }
    }
}
