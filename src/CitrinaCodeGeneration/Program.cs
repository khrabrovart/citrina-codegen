using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using VKApiSchemaParser;
using VKApiSchemaParser.Models;

namespace CitrinaCodeGeneration
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var vkApiSchema = new VKApiSchema();
            var classes = vkApiSchema.GetObjectsAsync().Result;
            var sc = classes.Objects.Take(5);

            var cg = new CodeGenerator();
            var gc = new List<SyntaxNode>();

            foreach (var c in sc)
            {
                var properties = c.Value.Properties?.Select(p => cg.NewProperty(PrepareName(p.Name), PrepareType(p.Type)));
                gc.Add(cg.NewClass(PrepareName(c.Value.Name), null, properties?.ToArray()));
            }

            var newNamespace = cg.NewNamespace("TestNamespace", gc.ToArray());
            cg.NewSourceFile("test.cs", newNamespace, new[] { "System", "System.Text" });
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
