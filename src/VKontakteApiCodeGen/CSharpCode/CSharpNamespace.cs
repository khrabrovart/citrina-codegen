using System.Collections.Generic;

namespace VKontakteApiCodeGen.CSharpCode
{
    public class CSharpNamespace
    {
        public string Name { get; set; }

        public IList<CSharpClass> Classes { get; set; }
    }
}
