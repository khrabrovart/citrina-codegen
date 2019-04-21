using System.Collections.Generic;

namespace VKontakteApiCodeGen.CSharpCode
{
    public class CSharpEnum
    {
        public string Name { get; set; }

        public string Summary { get; set; }

        public IEnumerable<CSharpEnumKey> Keys { get; set; }
    }
}
