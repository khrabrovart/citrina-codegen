using System.Collections.Generic;

namespace VKontakteApiCodeGen.CSharpCode
{
    public class CSharpSourceFile
    {
        public string Name { get; set; }

        public IEnumerable<string> Usings { get; set; }

        public CSharpNamespace Namespace { get; set; }
    }
}
