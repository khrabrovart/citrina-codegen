using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CitrinaCodeGeneration.CSharpCode;

namespace CitrinaCodeGeneration
{
    public class SimpleCodeGenerator
    {
        private readonly CodeBuilder _codeBuilder;

        public SimpleCodeGenerator()
        {
            _codeBuilder = new CodeBuilder();
        }

        public async Task CreateSourceFileAsync(CSharpSourceFile sourceFile)
        {
            if (string.IsNullOrWhiteSpace(sourceFile.Name))
            {
                throw new ArgumentNullException(nameof(sourceFile.Name));
            }

            if (!sourceFile.Name.EndsWith(".cs"))
            {
                sourceFile.Name += ".cs";
            }

            foreach (var usingName in sourceFile.Usings)
            {
                _codeBuilder.Line($"using {usingName};");
            }

            _codeBuilder.Line();

            if (sourceFile.Namespace != null)
            {
                AddNamespace(sourceFile.Namespace);
            }

            await File.WriteAllTextAsync(sourceFile.Name, _codeBuilder.Code);
        }

        private void AddNamespace(CSharpNamespace ns)
        {
            _codeBuilder.Line($"namespace {ns.Name}");
            _codeBuilder.IterableBlock(ns.Classes?.ToArray(), AddClass, true);
        }

        private void AddClass(CSharpClass cl)
        {
            _codeBuilder.Line($"public class {cl.Name}");
            _codeBuilder.IterableBlock(cl.Properties?.ToArray(), AddProperty, true);
        }

        private void AddProperty(CSharpProperty property)
        {
            // Temporarily disable descriptions
            //if (!string.IsNullOrWhiteSpace(property.Summary))
            //{
            //    AddSummary(property.Summary);
            //}

            if (property.Attributes != null)
            {
                foreach (var attr in property.Attributes)
                {
                    AddAttribute(attr);
                }
            }

            _codeBuilder.Line($"public {property.Type} {property.Name} {{ get; set; }} ");
        }

        private void AddAttribute(string attribute)
        {
            _codeBuilder.Line($"[{attribute}]");
        }

        private void AddSummary(string summary)
        {
            _codeBuilder.Line("/// <summary>");
            _codeBuilder.Line($"/// {summary}");
            _codeBuilder.Line("/// </summary>");
        }
    }
}
