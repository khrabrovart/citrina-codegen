using System.Collections.Generic;

namespace VKontakteApiCodeGen.CSharpCode
{
    public class CSharpClass
    {
        public string Name { get; set; }

        public IEnumerable<CSharpProperty> Properties { get; set; }
    }
}
