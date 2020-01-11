using System.Collections.Generic;
using System.Linq;
using VKApiCodeGen.Extensions;
using VKApiSchemaParser.Models;

namespace VKApiCodeGen.Generator.Entities
{
    public class CSharpMethod : ISyntaxEntity
    {
        public string Name { get; set; }

        public CSharpSummary Summary { get; set; }

        public string ReturnType { get; set; }

        public IDictionary<string, string> Parameters { get; set; }

        // If null then this method behaves as interface
        public CSharpMethodBody Body { get; set; }

        public static CSharpMethod[] Map(ApiMethod method, bool forInterface)
        {
            return method.Responses
                .Select(response => new CSharpMethod
                {
                    Name = method.Name.ToBeautifiedName(),
                    Summary = new CSharpSummary(method.Description),
                    ReturnType = response.GetCSharpType(),
                    Parameters = method.Parameters?.ToDictionary(p => p.Name.ToBeautifiedName(StringCase.Camel), p => p.GetCSharpType(true)),
                    Body = forInterface ? null : new CSharpMethodBody(method, response)
                })
                .ToArray();
        }

        public void WriteSyntax(SyntaxBuilder builder)
        {
            if (Summary != null)
            {
                Summary.WriteSyntax(builder);
            }

            var paramsString = Parameters != null 
                ? string.Join(", ", Parameters.Select(p => $"{p.Value} {p.Key} = null")) 
                : string.Empty;

            var methodDeclaration = $"Task<ApiRequest<{ReturnType}>> {Name}({paramsString})";

            if (Body == null)
            {
                builder.Line(methodDeclaration + ';');
            }
            else
            {
                builder.Line("public " + methodDeclaration);
                builder.Block(() => Body.WriteSyntax(builder));
            }
        }
    }
}
