using System.Collections.Generic;

namespace VKontakteApiCodeGen.CSharpCode
{
    public class CSharpClass
    {
        public string Name { get; set; }

        public string Summary { get; set; }

        public IEnumerable<CSharpProperty> Properties { get; set; }

        public string BaseClass { get; set; }
    }
}
