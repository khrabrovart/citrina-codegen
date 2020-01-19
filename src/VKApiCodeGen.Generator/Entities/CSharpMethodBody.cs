using System;
using System.Linq;
using VKApiCodeGen.Extensions;
using VKApiSchemaParser.Models;

namespace VKApiCodeGen.Generator.Entities
{
    public class CSharpMethodBody : ISyntaxEntity
    {
        public CSharpMethodBody(ApiMethod method, ApiObject response)
        {
            Method = method;
            Response = response;
        }

        public ApiMethod Method { get; set; }

        public ApiObject Response { get; set; }

        public void WriteSyntax(SyntaxBuilder builder)
        {
            builder.Line("var request = new Dictionary<string, string>");
            builder.Block(() =>
            {
                var parameters = Method.Parameters?.ToArray() ?? Array.Empty<ApiMethodParameter>();

                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];

                    var mappingString = GetParameterMappingString(
                        parameter.Name.ToBeautifiedName(StringCase.Camel),
                        parameter.GetCSharpType());

                    builder.Line($"[\"{parameter.Name}\"] = {mappingString},");
                }
            }, insertSemicolon: true);

            builder.Line();
            // добавить sccessToken
            builder.Line($"return RequestManager.CreateRequestAsync<{Response.GetCSharpType()}>(\"{Method.FullName}\", null, request);");
        }

        private static string GetParameterMappingString(string parameterName, string parameterType)
        {
            if (parameterType == "bool?")
            {
                return $"RequestHelpers.ParseBoolean({parameterName})";
            }

            if (parameterType.EndsWith('?'))
            {
                return $"{parameterName}?.ToString()";
            }

            if (parameterType.StartsWith("IEnumerable"))
            {
                return $"RequestHelpers.ParseEnumerable({parameterName})";
            }

            return parameterName;
        }
    }
}
