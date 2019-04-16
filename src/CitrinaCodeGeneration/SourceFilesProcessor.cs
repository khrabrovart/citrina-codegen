using CitrinaCodeGeneration.CSharpCode;
using System;
using System.Collections.Generic;
using System.Linq;
using VKApiSchemaParser.Models;

namespace CitrinaCodeGeneration
{
    public class SourceFilesProcessor
    {
        private const string NamespaceName = "VKontakte.Net.Models";

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

            if (_sourceFiles.TryGetValue(name, out var sourceFile))
            {
                sourceFile.Namespace.Classes.Add(ConvertToClass(obj));
            }
            else
            {
                var newSourceFile = CreateSourceFileWithObject(name, obj);
                _sourceFiles.Add(name, newSourceFile);
            }
        }

        private string GetSourceFileNameForObject(ApiObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var firstPart = obj.OriginalName.Split('_').First();

            return firstPart.First().ToString().ToUpper() + firstPart.Substring(1);
        }

        private CSharpSourceFile CreateSourceFileWithObject(string name, ApiObject obj)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return new CSharpSourceFile
            {
                Name = $"gen\\{name}.cs",
                Usings = PrepareUsings(),
                Namespace = new CSharpNamespace
                {
                    Name = NamespaceName,
                    Classes = new List<CSharpClass> { ConvertToClass(obj) }
                }
            };
        }

        private CSharpClass ConvertToClass(ApiObject obj)
        {
            return new CSharpClass
            {
                Name = obj.Name,
                Properties = obj.Properties?.Select(ConvertToProperty) ?? Enumerable.Empty<CSharpProperty>()
            };
        }

        private CSharpProperty ConvertToProperty(ApiObject obj)
        {
            var invalidName = char.IsNumber(obj.Name.First());

            return new CSharpProperty
            {
                Name = invalidName ? '_' + obj.Name : obj.Name,
                Summary = obj.Description,
                Type = GetType(obj),
                Attributes = invalidName ? new[] { $"JsonProperty(\"{obj.OriginalName}\")" } : null
            };
        }

        private string GetType(ApiObject obj)
        {
            // Handle primitive types
            switch (obj.Type)
            {
                case ApiObjectType.Integer:
                    return "int?";
                case ApiObjectType.Boolean:
                    return "bool?";
                case ApiObjectType.Number:
                    return "double?";
                case ApiObjectType.String:
                    return "string";
                case ApiObjectType.Array:
                    return "IEnumerable<object>";
            }

            // Handle references
            if (obj.Reference != null)
            {
                return TrickyTypesMap.TryGetValue(obj.Reference.Name, out var realType) ? realType : obj.Reference.Name;
            }

            return "object";
        }

        private IDictionary<string, Predicate<CSharpSourceFile>> PrepareUsings()
        {
            return new Dictionary<string, Predicate<CSharpSourceFile>>
            {
                {
                    "System.Collections.Generic", sf =>
                        sf.Namespace.Classes
                            .Any(cl => cl.Properties?.Any(p => p.Type.StartsWith("IEnumerable")) ?? false)
                },
                {
                    "Newtonsoft.Json", sf =>
                        sf.Namespace.Classes
                            .Any(cl => cl.Properties?.Any(p =>
                                           p.Attributes?.Any(a => a.StartsWith("JsonProperty")) ?? false) ?? false)
                }
            };
        }
    }
}
