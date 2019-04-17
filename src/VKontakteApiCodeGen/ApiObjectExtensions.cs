using System.Collections.Generic;
using System.Linq;
using VKApiSchemaParser.Models;
using VKontakteApiCodeGen.CSharpCode;

namespace VKontakteApiCodeGen
{
    public static class ApiObjectExtensions
    {
        private static readonly IDictionary<string, string> TrickyTypesMap = new Dictionary<string, string>
        {
            { "BaseBoolInt", "bool?" },
            { "BaseOkResponse", "bool?" }
        };

        public static CSharpClass ToClass(this ApiObject obj)
        {
            return new CSharpClass
            {
                Name = obj.Name,
                Properties = obj.Properties?.Select(ToAutoProperty) ?? Enumerable.Empty<CSharpProperty>()
            };
        }

        public static CSharpEnum ToEnum(this ApiObject obj)
        {
            IList<KeyValuePair<string, string>> values;

            if (obj.EnumNames != null)
            {
                values = obj.Enum.Zip(obj.EnumNames, (val, name) => KeyValuePair.Create(name, val)).ToList();
            }
            else
            {
                values = new List<KeyValuePair<string, string>>();
                var index = 0;

                foreach (var val in obj.Enum)
                {
                    values.Add(new KeyValuePair<string, string>(val, index.ToString()));
                    index++;
                }
            }

            return new CSharpEnum
            {
                Name = obj.Name,
                Values = values
            };
        }

        public static CSharpProperty ToAutoProperty(this ApiObject obj)
        {
            var invalidName = char.IsNumber(obj.Name.First());

            return new CSharpProperty
            {
                Name = invalidName ? '_' + obj.Name : obj.Name,
                Summary = obj.Description,
                Type = GetCSharpType(obj),
                Attributes = invalidName ? new[] { $"JsonProperty(\"{obj.OriginalName}\")" } : null
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
                return TrickyTypesMap.TryGetValue(obj.Reference.Name, out var realType) ? realType : obj.Reference.Name;
            }

            return "object";
        }
    }
}
