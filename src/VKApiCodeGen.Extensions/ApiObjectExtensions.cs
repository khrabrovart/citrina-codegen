using System.Collections.Generic;
using System.Linq;
using VKApiSchemaParser.Models;

namespace VKApiCodeGen.Extensions
{
    public static class ApiObjectExtensions
    {
        private static readonly IDictionary<string, string> TrickyTypesMap = new Dictionary<string, string>
        {
            { "base_bool_int", "bool?" },
            { "base_ok_response", "bool?" }
        };

        private static readonly IEnumerable<ApiObjectType> EnumObjectTypes = new[] { ApiObjectType.String, ApiObjectType.Integer };
        private static readonly IEnumerable<ApiObjectType> ClassObjectTypes = new[] { ApiObjectType.Object };

        public static bool IsEnum(this ApiObject obj) => EnumObjectTypes.Contains(obj.Type) && obj.Enum != null;

        public static bool IsClass(this ApiObject obj) => ClassObjectTypes.Contains(obj.Type);

        public static string GetCSharpType(this ApiObject obj, bool preferNullable = true)
        {
            // Handle primitive types
            switch (obj.Type)
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

                    if (obj.Items != null)
                    {
                        var genericType = string.IsNullOrWhiteSpace(obj.Items.Name) ? 
                            obj.Items.GetCSharpType(false) : 
                            obj.Items.Name.ToBeautifiedName();

                        return $"IEnumerable<{genericType}>";
                    }

                    return "IEnumerable<object>"; // TODO: Maybe throw an exception here later...?
            }

            // Handle references
            if (obj.Reference != null)
            {
                if (TrickyTypesMap.TryGetValue(obj.Reference.Name, out var realType))
                {
                    return realType;
                }

                if (!obj.Reference.IsClass())
                {
                    return obj.Reference.GetCSharpType(true);
                }
                
                return obj.Reference.Name.ToBeautifiedName();
            }

            return "object"; // TODO: Maybe throw an exception here later...?
        }
    }
}
