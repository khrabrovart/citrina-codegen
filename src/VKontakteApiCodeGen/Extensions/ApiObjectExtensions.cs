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
            string baseClass = null;
            IEnumerable<CSharpProperty> properties = null;

            if (obj.AllOf != null && obj.AllOf.First().OneOf == null)
            {
                baseClass = obj.AllOf.FirstOrDefault(o => o.Name != null)?.Name.ToBeautifiedName();
                properties = obj.AllOf
                    .Where(o => o.Name == null)
                    .SelectMany(o => o.Properties)
                    .Select(ToAutoProperty);
            }
            else
            {
                properties = obj.Properties?.Select(ToAutoProperty);
            }

            return new CSharpClass
            {
                Name = obj.Name.ToBeautifiedName(),
                Summary = obj.Description,
                Properties = properties?.ToArray() ?? Enumerable.Empty<CSharpProperty>(),
                BaseClass = baseClass
            };
        }

        public static CSharpEnum ToEnum(this ApiObject obj)
        {
            IEnumerable<CSharpEnumKey> keys;

            if (obj.EnumNames != null)
            {
                var isNumberEnum = obj.Enum.All(v => char.IsNumber(v[0]));

                if (isNumberEnum)
                {
                    keys = obj.EnumNames.Select(v => new CSharpEnumKey(v.ToBeautifiedName()));
                }
                else
                {
                    keys = obj.Enum.Zip(obj.EnumNames, (val, name) => new CSharpEnumKey(name.ToBeautifiedName(), val));
                }
            }
            else
            {
                keys = obj.Enum.Select(v => new CSharpEnumKey(v.ToBeautifiedName(), v));
            }

            return new CSharpEnum
            {
                Name = obj.Name.ToBeautifiedName(),
                Summary = obj.Description,
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

        public static string GetCSharpType(this ApiObject obj, bool nullablePrimitives = true)
        {
            // Handle primitive types
            switch (obj.Type)
            {
                case ApiObjectType.Integer:
                    return nullablePrimitives ? "int?" : "int";

                case ApiObjectType.Boolean:
                    return nullablePrimitives ? "bool?" : "bool";

                case ApiObjectType.Number:
                    return nullablePrimitives ? "double?" : "double";

                case ApiObjectType.Multiple:
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

                    return "IEnumerable<object>"; // Maybe throw an exception here later...?
            }

            // Handle references
            if (obj.Reference != null)
            {
                return TrickyTypesMap.TryGetValue(obj.Reference.Name, out var realType) ? realType : obj.Reference.Name.ToBeautifiedName();
            }

            return "object"; // Maybe throw an exception here later...?
        }
    }
}
