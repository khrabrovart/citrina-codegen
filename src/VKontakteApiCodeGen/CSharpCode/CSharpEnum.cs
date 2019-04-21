using System.Collections.Generic;

namespace VKontakteApiCodeGen.CSharpCode
{
    public class CSharpEnum
    {
        public string Name { get; set; }

        public IList<CSharpEnumKey> Keys { get; set; }
    }
}
