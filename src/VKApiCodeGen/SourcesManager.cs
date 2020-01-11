using System.Collections.Generic;
using VKApiCodeGen.Generator.Entities;

namespace VKApiCodeGen
{
    internal class SourcesManager
    {
        public SourcesManager()
        {
            SourceGroups = new Dictionary<string, List<CSharpSourceFile>>();
        }

        public IDictionary<string, List<CSharpSourceFile>> SourceGroups { get; }

        public void AddToSourceGroup(string groupName, CSharpSourceFile sourceFile)
        {
            if (sourceFile == null)
            {
                return;
            }

            if (!SourceGroups.ContainsKey(groupName))
            {
                SourceGroups.Add(groupName, new List<CSharpSourceFile>());
            }

            SourceGroups[groupName].Add(sourceFile);
        }
    }
}
