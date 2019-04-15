using System.Linq;
using System.Threading.Tasks;
using CitrinaCodeGeneration.CSharpCode;
using VKApiSchemaParser;
using VKApiSchemaParser.Models;

namespace CitrinaCodeGeneration
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var vkApiSchema = await VKApiSchema.ParseAsync();

            var codeGen = new SimpleCodeGenerator();

            var sourceFile = new CSharpSourceFile
            {
                Name = "test.cs",
                Usings = new[] {"System", "System.Text"},
                Namespace = new CSharpNamespace
                {
                    Name = "TestNameSpace",
                    Classes = vkApiSchema.Objects.Take(10).Select(o => o.Value).Select(o => new CSharpClass
                    {
                        Name = o.Name,
                        Properties = o.Properties?.Select(p => new CSharpProperty
                        {
                            Name = p.Name,
                            Type = PrepareType(p.Type)
                        })
                    })
                }
            };

            await codeGen.CreateSourceFileAsync(sourceFile);
        }

        private static string PrepareName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            if (char.IsNumber(name.First()))
            {
                return '_' + name;
            }

            return name;
        }

        private static string PrepareType(ApiObjectType type)
        {
            switch (type)
            {
                case ApiObjectType.Integer:
                    return "int";
                case ApiObjectType.String:
                    return "string";
                case ApiObjectType.Array:
                    return "IEnumerable<object>";
                default:
                    return "object";
            }
        }
    }
}
