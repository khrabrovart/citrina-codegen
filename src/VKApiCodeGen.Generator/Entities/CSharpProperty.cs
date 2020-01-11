using System.Collections.Generic;
using VKApiCodeGen.Extensions;
using VKApiSchemaParser.Models;

namespace VKApiCodeGen.Generator.Entities
{
    public class CSharpProperty : ISyntaxEntity
    {
        public string Name { get; set; }

        public CSharpSummary Summary { get; set; }

        public string Type { get; set; }

        public IEnumerable<string> Attributes { get; set; }

        public static CSharpProperty Map(ApiObject obj)
        {
            return new CSharpProperty
            {
                Name = obj.Name.ToBeautifiedName(),
                Summary = new CSharpSummary(obj.Description),
                Type = obj.GetCSharpType(preferNullable: true),
                Attributes = obj.Name.StartsWithNumber() ? new[] { GetJsonPropertyAttribute(obj.Name) } : null
            };
        }

        public void WriteSyntax(SyntaxBuilder builder)
        {
            if (Summary != null)
            {
                Summary.WriteSyntax(builder);
            }

            if (Attributes != null)
            {
                foreach (var attr in Attributes)
                {
                    new CSharpAttribute(attr).WriteSyntax(builder);
                }
            }

            builder.Line($"public {Type} {Name} {{ get; set; }} ");
        }

        private static string GetJsonPropertyAttribute(string name) => $"JsonProperty(\"{name}\")";
    }
}
