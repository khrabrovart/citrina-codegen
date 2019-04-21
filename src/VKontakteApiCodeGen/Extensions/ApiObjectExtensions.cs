using System.Collections.Generic;
using System.Linq;
using VKApiSchemaParser.Models;
using VKontakteApiCodeGen.CSharpCode;

namespace VKontakteApiCodeGen.Extensions
{
    public static class ApiObjectExtensions
    {
        private static readonly IDictionary<string, string> TrickyTypesMap = new Dictionary<string, string>
        {
            { "base_bool_int", "bool?" },
            { "base_ok_response", "bool?" }
        };

        private static readonly IEnumerable<ApiObjectType> EnumTypes = new[] { ApiObjectType.String, ApiObjectType.Integer };

        public static bool IsEnum(this ApiObject obj) => EnumTypes.Contains(obj.Type) && obj.Enum != null;

        public static CSharpClass ToClass(this ApiObject obj)
        {
            return new CSharpClass
            {
                Name = obj.Name.ToBeautifiedName(),
                Properties = obj.Properties?.Select(ToAutoProperty) ?? Enumerable.Empty<CSharpProperty>()
            };
        }

        public static CSharpEnum ToEnum(this ApiObject obj)
        {
            IList<CSharpEnumKey> keys;

            var isNumberEnum = char.IsNumber(obj.Enum.First()[0]);

            if (obj.EnumNames != null)
            {
                if (isNumberEnum)
                {
                    keys = obj.EnumNames.Select(n => new CSharpEnumKey(n.ToBeautifiedName())).ToList();
                }
                else
                {
                    keys = obj.Enum
                        .Zip(obj.EnumNames, (val, name) => new CSharpEnumKey(name.ToBeautifiedName(), val))
                        .ToList();
                }
            }
            else
            {
                keys = new List<CSharpEnumKey>();

                foreach (var key in obj.Enum)
                {
                    keys.Add(new CSharpEnumKey(key.ToBeautifiedName(), key));
                }
            }

            return new CSharpEnum
            {
                Name = obj.Name.ToBeautifiedName(),
                Keys = keys
            };
        }

        public static CSharpProperty ToAutoProperty(this ApiObject obj)
        {
            return new CSharpProperty
            {
                Name = obj.Name.ToBeautifiedName(),
                Summary = obj.Description,
                Type = GetCSharpType(obj),
                Attributes = obj.Name.IsValidName() ? null : new[] { $"JsonProperty(\"{obj.Name}\")" }
            };
        }

        public static string GetCSharpType(this ApiObject obj)
        {
            // Handle primitive types
            switch (obj.Type)
            {
                case ApiObjectType.Integer:
                    return "int?";
                case ApiObjectType.Boolean:
                    return "bool?";
                case ApiObjectType.Number:
                    return "double?";
                case ApiObjectType.String:
                    return "string";
                case ApiObjectType.Array:
                    return "IEnumerable<object>";
            }

            // Handle references
            if (obj.Reference != null)
            {
                return TrickyTypesMap.TryGetValue(obj.Reference.Name, out var realType) ? realType : obj.Reference.Name.ToBeautifiedName();
            }

            return "object";
        }
    }
}
