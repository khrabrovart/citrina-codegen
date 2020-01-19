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

        public CSharpEnum[] EnumParameters { get; set; }

        // If null then this method behaves as interface
        public CSharpMethodBody Body { get; set; }

        public static CSharpMethod[] Map(ApiMethod method, bool forInterface)
        {
            return method.Responses
                .Select(response =>
                {
                    var name = method.Name.ToBeautifiedName();
                    var parameters = new Dictionary<string, string>();
                    var enums = new List<CSharpEnum>();

                    foreach (var p in method.Parameters ?? Enumerable.Empty<ApiMethodParameter>())
                    {
                        var parameterName = p.Name.ToBeautifiedName(StringCase.Camel);

                        if (p.IsEnum())
                        {
                            
                            var typeName = $"{name}_{p.Name.ToBeautifiedName()}";

                            parameters.Add(parameterName, typeName);
                            enums.Add(CSharpEnum.FromObject(p, typeName));
                        }
                        // Check if enum is in array and it's not a reference (references have names)
                        else if (p.IsArray() && p.Items.IsEnum() && p.Items.Name == null)
                        {
                            var typeName = $"{name}{p.Name.ToBeautifiedName()}";

                            parameters.Add(parameterName, $"IEnumerable<{typeName}>");
                            enums.Add(CSharpEnum.FromObject(p.Items, typeName));
                        }
                        else
                        {
                            parameters.Add(parameterName, p.GetCSharpType());
                        }
                    }

                    return new CSharpMethod
                    {
                        Name = name,
                        Summary = new CSharpSummary(method.Description),
                        ReturnType = response.GetCSharpType(),
                        Parameters = parameters,
                        EnumParameters = enums.ToArray(),
                        Body = forInterface ? null : new CSharpMethodBody(method, response)
                    };
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
