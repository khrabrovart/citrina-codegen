using System.Collections.Generic;
using System.Linq;
using VKApiSchemaParser.Models;

namespace VKApiCodeGen.Extensions
{
    public static class ApiMethodParameterExtensions
    {
        private static readonly IEnumerable<ApiObjectType> EnumObjectTypes = new[] { ApiObjectType.String, ApiObjectType.Integer };

        public static bool IsEnum(this ApiMethodParameter parameter) => EnumObjectTypes.Contains(parameter.Type) && parameter.Enum != null;

        public static bool IsArray(this ApiMethodParameter parameter) => parameter.Type == ApiObjectType.Array && parameter.Items != null;

        public static string GetCSharpType(this ApiMethodParameter parameter)
        {
            // Handle primitive types
            switch (parameter.Type)
            {
                case ApiObjectType.Integer:
                    return "int?";

                case ApiObjectType.Boolean:
                    return "bool?";

                case ApiObjectType.Number:
                    return "double?";

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
