using System.IO;
using System.Text;
using System.Threading.Tasks;
using CitrinaCodeGeneration.CSharpCode;

namespace CitrinaCodeGeneration
{
    public class SimpleCodeGenerator
    {
        private readonly StringBuilder _codeBuilder;

        public SimpleCodeGenerator()
        {
            _codeBuilder = new StringBuilder();
        }

        public async Task CreateSourceFileAsync(CSharpSourceFile sourceFile)
        {
            foreach (var usingName in sourceFile.Usings)
            {
                AddUsing(usingName);
            }

            AddEmptyLine();
            AddNamespace(sourceFile.Namespace);

            await File.WriteAllTextAsync(sourceFile.Name, Code);
        }

        private void AddEmptyLine()
        {
            _codeBuilder.AppendLine();
        }

        private void AddUsing(string usingName)
        {
            _codeBuilder.AppendLine($"using {usingName};");
        }

        private void AddNamespace(CSharpNamespace ns)
        {
            if (ns == null)
            {
                return;
            }

            _codeBuilder.AppendLine($"namespace {ns.Name}");
            _codeBuilder.AppendLine("{");

            if (ns.Classes != null)
            {
                foreach (var cl in ns.Classes)
                {
                    AddClass(cl);
                }
            }

            _codeBuilder.AppendLine("}");
        }

        private void AddClass(CSharpClass cl)
        {
            if (cl == null)
            {
                return;
            }

            _codeBuilder.AppendLine($"{Tabs(1)}public class {cl.Name}");
            _codeBuilder.AppendLine($"{Tabs(1)}{{");

            if (cl.Properties != null)
            {
                foreach (var property in cl.Properties)
                {
                    AppProperty(property);
                }
            }

            _codeBuilder.AppendLine($"{Tabs(1)}}}");
        }

        private void AppProperty(CSharpProperty property)
        {
            if (property == null)
            {
                return;
            }

            _codeBuilder.AppendLine($"{Tabs(2)}public {property.Type} {property.Name} {{ get; set; }} ");
        }

        private string Code => _codeBuilder.ToString();

        private static string Tabs(int count) => new string(' ', 4 * count);
    }
}
