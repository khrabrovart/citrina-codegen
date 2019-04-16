using System;
using System.Collections.Generic;

namespace CitrinaCodeGeneration.CSharpCode
{
    public class CSharpSourceFile
    {
        public string Name { get; set; }

        public IDictionary<string, Predicate<CSharpSourceFile>> Usings { get; set; }

        public CSharpNamespace Namespace { get; set; }
    }
}
