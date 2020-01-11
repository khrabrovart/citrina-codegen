using System;
using System.Collections.Generic;
using System.Linq;
using VKApiCodeGen.Extensions;
using VKApiSchemaParser.Models;

namespace VKApiCodeGen.Generator.Entities
{
    public class CSharpSourceFile : ISyntaxEntity
    {
        private const string NamespaceName = "CitrinaVK.Models";

        private static readonly IEnumerable<string> DefaultUsings = new[]
        {
            "System.Collections.Generic", "Newtonsoft.Json"
        };

        public string Name { get; set; }

        public string Namespace { get; set; }

        public IEnumerable<string> Usings { get; set; }

        public CSharpClass Class { get; set; }

        public CSharpEnum Enum { get; set; }

        public CSharpInterface Interface { get; set; }

        public static CSharpSourceFile FromObject(ApiObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var sourceFile = new CSharpSourceFile
            {
                Name = obj.Name.ToBeautifiedName(),
                Usings = DefaultUsings,
                Namespace = NamespaceName
            };

            if (obj.IsEnum())
            {
                sourceFile.Enum = CSharpEnum.Map(obj);
            }
            else if (obj.IsClass())
            {
                sourceFile.Class = CSharpClass.Map(obj);
            }
            else
            {
                return null;
            }

            return sourceFile;
        }

        public static CSharpSourceFile FromMethods(ApiMethod[] methods, bool asInterface)
        {
            var name = methods[0].Category.ToBeautifiedName();
            var sourceFile = new CSharpSourceFile
            {
                Name = asInterface ? 'I' + name : name,
                Usings = DefaultUsings,
                Namespace = NamespaceName
            };

            if (asInterface)
            {
                sourceFile.Interface = CSharpInterface.FromMethods(methods);
            }
            else
            {
                sourceFile.Class = CSharpClass.FromMethods(methods);
            }

            return sourceFile;
        }

        public string GetSourceCode()
        {
            var builder = new SyntaxBuilder();
            WriteSyntax(builder);
            return builder.SourceCode;
        }

        public void WriteSyntax(SyntaxBuilder builder)
        {
            builder.Clear();

            if (Usings != null && Usings.Any())
            {
                foreach (var u in Usings)
                {
                    builder.Line($"using {u};");
                }

                builder.Line();
            }

            builder.Line($"namespace {Namespace}");

            builder.Block(() =>
            {
                if (Class != null)
                {
                    Class.WriteSyntax(builder);
                }
                
                if (Enum != null)
                {
                    Enum.WriteSyntax(builder);
                }
                
                if (Interface != null)
                {
                    Interface.WriteSyntax(builder);
                }
            });
        }
    }
}
