using System.Collections.Generic;

namespace CitrinaCodeGeneration.CSharpCode
{
    public class CSharpNamespace
    {
        public string Name { get; set; }

        public IList<CSharpClass> Classes { get; set; }
    }
}
