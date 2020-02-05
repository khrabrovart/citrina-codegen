using System;
using System.Collections.Generic;
using System.Linq;
using VKApiCodeGen.Extensions;
using VKApiSchemaParser.Models;

namespace VKApiCodeGen.Generator.Entities
{
    public class CSharpMethod : ISyntaxEntity
    {
        public static readonly IDictionary<string, CSharpEnum> MethodEnumParameters = new Dictionary<string, CSharpEnum>();

        private static readonly IDictionary<ApiAccessTokenType, string> AccessTokenTypesMap = new Dictionary<ApiAccessTokenType, string>
        {
            { ApiAccessTokenType.Service, "ServiceAccessToken" },
            { ApiAccessTokenType.User, "UserAccessToken" },
            { ApiAccessTokenType.Group, "GroupAccessToken" },
            { ApiAccessTokenType.Open, null },
            { ApiAccessTokenType.Undefined, null }
        };

        public string Name { get; set; }

        public CSharpSummary Summary { get; set; }

        public string ReturnType { get; set; }

        public IDictionary<string, string> Parameters { get; set; }

        // If null then this method behaves as interface
        public CSharpMethodBody Body { get; set; }

        public static CSharpMethod[] FromMethod(ApiMethod method, bool forInterface)
        {
            var outputMethods = new List<CSharpMethod>();

            foreach (var response in method.Responses)
            {
                if (!method.AccessTokenTypes.Any())
                {
                    throw new Exception("Access token types not found.");
                }

                var tokenTypes = method.AccessTokenTypes.ToArray();

                for (int i = 0; i < tokenTypes.Length; i++)
                {
                    var responseName = response.Name.EndsWith("Response") ? response.Name[0..^8].ToBeautifiedName() : null;
                    var methodName = method.Name.ToBeautifiedName();
                    var methodNameByResponse = responseName == null ? methodName : $"{methodName}_{responseName}";

                    var parameters = new Dictionary<string, string>();
                    var accessTokenType = GetAccessTokenTypeName(tokenTypes[i]);

                    if (accessTokenType != null)
                    {
                        parameters.Add("accessToken", accessTokenType);
                    }

                    foreach (var p in method.Parameters ?? Enumerable.Empty<ApiMethodParameter>())
                    {
                        var parameterName = p.Name.ToBeautifiedName(StringCase.Camel);

                        if (p.IsEnum())
                        {
                            var newEnum = CSharpEnum.FromObject(p);
                            newEnum.Name = $"{method.Category.ToBeautifiedName()}_{methodName}_{parameterName}";

                            MethodEnumParameters.TryAdd(newEnum.Name, newEnum);
                            parameters.Add(parameterName, newEnum.Name);
                        }
                        // Check if enum is in array and it's not a reference (references have names)
                        else if (p.IsArray() && p.Items.IsEnum() && p.Items.Name == null)
                        {
                            var newEnum = CSharpEnum.FromObject(p.Items);
                            newEnum.Name = $"{method.Category.ToBeautifiedName()}_{methodName}_{parameterName}";

                            MethodEnumParameters.TryAdd(newEnum.Name, newEnum);
                            parameters.Add(parameterName, $"IEnumerable<{newEnum.Name}>");
                        }
                        else
                        {
                            parameters.Add(parameterName, p.GetCSharpType());
                        }
                    }

                    var newMethod = new CSharpMethod
                    {
                        Name = methodNameByResponse,
                        Summary = new CSharpSummary(method.Description),
                        ReturnType = response.GetCSharpType(),
                        Parameters = parameters,
                        Body = forInterface ? null : new CSharpMethodBody(method, response)
                    };

                    outputMethods.Add(newMethod);
                }
            }

            return outputMethods.ToArray();
        }

        public void WriteSyntax(SyntaxBuilder builder)
        {
            if (Summary != null)
            {
                Summary.WriteSyntax(builder);
            }

            var parameters = Parameters.Select(p => 
            {
                var parameterString = $"{p.Value} {p.Key}";

                if (p.Key != "accessToken")
                {
                    parameterString += " = null";
                }

                return parameterString;
            });

            var parametersString = Parameters != null ? string.Join(", ", parameters) : string.Empty;
            var methodDeclaration = $"Task<ApiRequest<{ReturnType}>> {Name}({parametersString})";

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

        private static string GetAccessTokenTypeName(ApiAccessTokenType accessTokenType)
        {
            return AccessTokenTypesMap.TryGetValue(accessTokenType, out var typeName)
                ? typeName
                : throw new Exception("Access token type is invalid");
        }
    }
}
