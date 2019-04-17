using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VKontakteApiCodeGen.CSharpCode;

namespace VKontakteApiCodeGen
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
            _codeBuilder.Clear();

            if (string.IsNullOrWhiteSpace(sourceFile.Name))
            {
                throw new ArgumentNullException(nameof(sourceFile.Name));
            }

            if (!sourceFile.Name.EndsWith(".cs"))
            {
                sourceFile.Name += ".cs";
            }

            foreach (var u in sourceFile.Usings)
            {
                if (u.Value?.Invoke(sourceFile) ?? true)
                {
                    _codeBuilder.Line($"using {u.Key};");
                }
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
