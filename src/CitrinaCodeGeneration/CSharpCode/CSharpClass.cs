using System.Collections.Generic;

namespace CitrinaCodeGeneration.CSharpCode
{
    public class CSharpClass
    {
        public string Name { get; set; }

        public IEnumerable<CSharpProperty> Properties { get; set; }
    }
}
