using VKontakteApiCodeGen.CSharpCode;
using System;
using System.Collections.Generic;
using System.Linq;
using VKApiSchemaParser.Models;

namespace VKontakteApiCodeGen
{
    public class SourceFilesProcessor
    {
        private const string NamespaceName = "VKontakte.Net.Models";
        private const string EnumsFileName = "Enums";

        private static readonly IDictionary<string, string> TrickyTypesMap = new Dictionary<string, string>
        {
            { "BaseBoolInt", "bool?" },
            { "BaseOkResponse", "bool?" }
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

            if (obj.Type == ApiObjectType.Object)
            {
                sourceFile.Namespace.Classes.Add(obj.ToClass());
            }
            else if ((obj.Type == ApiObjectType.String || obj.Type == ApiObjectType.Integer) && obj.Enum != null)
            {
                sourceFile.Namespace.Enums.Add(obj.ToEnum());
            }
        }

        private string GetSourceFileNameForObject(ApiObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (obj.Type == ApiObjectType.String && obj.Enum != null)
            {
                return EnumsFileName;
            }

            var firstPart = obj.OriginalName.Split('_').First();

            return firstPart.First().ToString().ToUpper() + firstPart.Substring(1);
        }

        private CSharpSourceFile CreateSourceFile(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return new CSharpSourceFile
            {
                Name = $"gen\\{name}.cs",
                Usings = new[] { "System.Collections.Generic", "Newtonsoft.Json" },
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
