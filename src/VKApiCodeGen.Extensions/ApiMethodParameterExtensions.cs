using System.Collections.Generic;
using VKApiSchemaParser.Models;

namespace VKApiCodeGen.Extensions
{
    public static class ApiMethodParameterExtensions
    {
        private static readonly IDictionary<string, string> TrickyTypesMap = new Dictionary<string, string>
        {
            { "base_bool_int", "bool?" },
            { "base_ok_response", "bool?" }
        };

        private static readonly IEnumerable<ApiObjectType> EnumObjectTypes = new[] { ApiObjectType.String, ApiObjectType.Integer };
        private static readonly IEnumerable<ApiObjectType> ClassObjectTypes = new[] { ApiObjectType.Object };

        public static string GetCSharpType(this ApiMethodParameter parameter, bool preferNullable = true)
        {
            // Handle primitive types
            switch (parameter.Type)
            {
                case ApiObjectType.Integer:
                    return preferNullable ? "int?" : "int";

                case ApiObjectType.Boolean:
                    return preferNullable ? "bool?" : "bool";

                case ApiObjectType.Number:
                    return preferNullable ? "double?" : "double";

                case ApiObjectType.Multiple: // TODO: Think about this type
                case ApiObjectType.String:
                    return "string";

                case ApiObjectType.Array:

                    if (parameter.Items != null)
                    {
                        var genericType = string.IsNullOrWhiteSpace(parameter.Items.Name) ?
                            parameter.Items.GetCSharpType(false) :
                            parameter.Items.Name.ToBeautifiedName();

                        return $"IEnumerable<{genericType}>";
                    }

                    return "IEnumerable<object>"; // TODO: Maybe throw an exception here later...?
            }

            return "object"; // TODO: Maybe throw an exception here later...?
        }
    }
}
