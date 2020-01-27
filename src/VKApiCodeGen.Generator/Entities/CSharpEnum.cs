using System;
using System.Collections.Generic;
using System.Linq;
using VKApiCodeGen.Extensions;
using VKApiSchemaParser.Models;

namespace VKApiCodeGen.Generator.Entities
{
    public class CSharpEnum : ISyntaxEntity
    {
        public string Name { get; set; }

        public CSharpSummary Summary { get; set; }

        public IDictionary<string, string> Keys { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is CSharpEnum other))
            {
                return false;
            }

            var thisKeys = Keys.Select(k => k.Key).ToArray();
            var otherKeys = other.Keys.Select(k => k.Key).ToArray();

            if (thisKeys.Length != otherKeys.Length)
            {
                return false;
            }

            for (int i = 0; i < thisKeys.Length; i++)
            {
                if (thisKeys[i] != otherKeys[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hash = Keys.Keys.Aggregate(0, (r, i) => r += i.GetHashCode());
            return Math.Abs(hash);
        }

        public static CSharpEnum FromObject(IApiEnumEntity obj)
        {
            IDictionary<string, string> keys;

            if (obj.EnumNames != null)
            {
                var isNumberEnum = obj.Enum.All(v => char.IsNumber(v[0]));

                if (isNumberEnum)
                {
                    keys = obj.EnumNames.ToDictionary(n => n.ToBeautifiedName(), n => (string)null);
                }
                else
                {
                    keys = obj.Enum
                        .Zip(obj.EnumNames, (val, name) => new KeyValuePair<string, string>(name.ToBeautifiedName(), val))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }
            else
            {
                keys = obj.Enum.ToDictionary(v => v.ToBeautifiedName());
            }

            return new CSharpEnum
            {
                Name = obj.Name?.ToBeautifiedName(),
                Summary = new CSharpSummary(obj.Description),
                Keys = keys
            };
        }

        public void WriteSyntax(SyntaxBuilder builder)
        {
            if (Summary != null)
            {
                Summary.WriteSyntax(builder);
            }

            builder.Line($"public enum {Name}");
            builder.Block(() =>
            {
                foreach (var key in Keys)
                {
                    if (key.Value != null && !int.TryParse(key.Value, out var intValue))
                    {
                        new CSharpAttribute($"EnumMember(Value = \"{key.Value}\")").WriteSyntax(builder);
                    }

                    builder.Line($"{key.Key},");
                }
            });
        }
    }
}
