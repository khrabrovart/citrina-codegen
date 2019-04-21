using System.Linq;
using VKontakteApiCodeGen.CSharpCode;

namespace VKontakteApiCodeGen
{
    public class SyntaxGenerator
    {
        private readonly CodeBuilder _codeBuilder = new CodeBuilder();

        public string GenerateSourceFileSyntax(CSharpSourceFile sourceFile)
        {
            _codeBuilder.Clear();

            if (sourceFile.Usings != null && sourceFile.Usings.Any())
            {
                foreach (var u in sourceFile.Usings)
                {
                    _codeBuilder.Line($"using {u};");
                }

                _codeBuilder.Line();
            }

            if (sourceFile.Namespace != null)
            {
                AddNamespace(sourceFile.Namespace);
            }

            return _codeBuilder.Code;
        }

        private void AddNamespace(CSharpNamespace ns)
        {
            _codeBuilder.Line($"namespace {ns.Name}");

            if (ns.Classes != null && ns.Classes.Any())
            {
                _codeBuilder.IterableBlock(ns.Classes.ToArray(), AddClass);
            }
            else if (ns.Enums != null && ns.Enums.Any())
            {
                _codeBuilder.IterableBlock(ns.Enums.ToArray(), AddEnum);
            }
        }

        private void AddClass(CSharpClass cl)
        {
            // Temporarily disable descriptions
            //if (!string.IsNullOrWhiteSpace(cl.Summary))
            //{
            //    AddSummary(cl.Summary);
            //}

            var baseClassSyntax = cl.BaseClass != null ? $" : {cl.BaseClass}" : null;

            _codeBuilder.Line($"public class {cl.Name}{baseClassSyntax}");
            _codeBuilder.IterableBlock(cl.Properties?.ToArray(), AddProperty);
        }

        private void AddEnum(CSharpEnum en)
        {
            // Temporarily disable descriptions
            //if (!string.IsNullOrWhiteSpace(en.Summary))
            //{
            //    AddSummary(en.Summary);
            //}

            _codeBuilder.Line($"public enum {en.Name}");
            _codeBuilder.IterableBlock(en.Keys.ToArray(), key => 
            {
                if (key.Value != null && !int.TryParse(key.Value, out var intValue))
                {
                    AddAttribute($"EnumMember(Value = \"{key.Value}\")");
                }

                _codeBuilder.Line($"{key.Name},");
            });
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
