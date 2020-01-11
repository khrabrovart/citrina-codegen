using System;
using System.Linq;

namespace VKApiCodeGen.Extensions
{
    public enum StringCase
    {
        Camel,
        Pascal
    }

    public static class StringExtensions
    {
        private static readonly string[] Keywords = new[] { "long", "object", "out", "private", "ref" };

        public static string ToBeautifiedName(this string name, StringCase stringCase = StringCase.Pascal)
        {  
            var beautifiedName = Beautify(name, stringCase);

            if (name.StartsWithNumber())
            {
                return '_' + beautifiedName;
            }

            if (name.IsKeyword())
            {
                return '@' + beautifiedName;
            }

            return beautifiedName;
        }

        public static bool StartsWithNumber(this string str) => str.Length > 0 && char.IsNumber(str[0]);

        public static bool IsKeyword(this string str) => Keywords.Contains(str);

        private static string ToPascalCase(string[] strs) => strs.Aggregate(string.Empty, (a, b) => a + b[0].ToString().ToUpper() + b.Substring(1));

        private static string ToCamelCase(string[] strs) => strs[0].ToLower() + ToPascalCase(strs[1..]);

        private static string Beautify(string str, StringCase strCase)
        {
            var trimmed = str?.Trim();

            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            var splitted = trimmed.Split('_', ' ');

            switch (strCase)
            {
                case StringCase.Pascal:
                    return ToPascalCase(splitted);
                case StringCase.Camel:
                    return ToCamelCase(splitted);
                default:
                    throw new ArgumentException(nameof(strCase));
            }
        }
    }
}
