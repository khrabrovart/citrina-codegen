using VKontakteApiCodeGen.CSharpCode;
using System;
using System.Collections.Generic;
using System.Linq;
using VKApiSchemaParser.Models;
using VKontakteApiCodeGen.Extensions;

namespace VKontakteApiCodeGen
{
    public class SourceFilesProcessor
    {
        private const string NamespaceName = "VKontakte.Net.Models";
        private const string EnumsFileName = "Enums";

        private static readonly IEnumerable<string> DefaultUsings = new[]
        {
            "System.Collections.Generic", "Newtonsoft.Json"
        };

        private readonly IDictionary<string, CSharpSourceFile> _sourceFiles = new Dictionary<string, CSharpSourceFile>();

        public IEnumerable<CSharpSourceFile> SourceFiles => _sourceFiles.Values;

        public void AddToSourceFile(ApiObject obj)
        {
            var name = GetSourceFileNameForObject(obj);
            CSharpSourceFile sourceFile;

            if (!_sourceFiles.TryGetValue(name, out sourceFile))
            {
                sourceFile = CreateSourceFile(name);
                _sourceFiles.Add(name, sourceFile);
            }

            if (obj.IsEnum())
            {
                sourceFile.Namespace.Enums.Add(obj.ToEnum());
            }
            else
            {
                sourceFile.Namespace.Classes.Add(obj.ToClass());
            }
        }

        private static string GetSourceFileNameForObject(ApiObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (obj.IsEnum())
            {
                return EnumsFileName;
            }

            var firstPart = obj.Name.Split('_').First();
            return firstPart.First().ToString().ToUpper() + firstPart.Substring(1);
        }

        private static CSharpSourceFile CreateSourceFile(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return new CSharpSourceFile
            {
                Name = $"gen\\{name}.cs",
                Usings = DefaultUsings,
                Namespace = new CSharpNamespace
                {
                    Name = NamespaceName,
                    Classes = new List<CSharpClass>(),
                    Enums = new List<CSharpEnum>()
                }
            };
        }
    }
}
