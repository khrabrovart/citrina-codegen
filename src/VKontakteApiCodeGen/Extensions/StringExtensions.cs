using System.Linq;

namespace VKontakteApiCodeGen.Extensions
{
    public static class StringExtensions
    {
        public static string ToBeautifiedName(this string name)
        {
            var isValidName = IsValidName(name);
            var beautifiedName = Beautify(name);

            return isValidName ? beautifiedName : '_' + beautifiedName;
        }

        public static bool IsValidName(this string name) => !char.IsNumber(name.First());

        private static string Beautify(string str)
        {
            var trimmed = str?.Trim();

            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            var splitted = trimmed.Split('_', ' ');

            if (splitted.Length == 1)
            {
                return trimmed[0].ToString().ToUpper() + trimmed.Substring(1);
            }

            return splitted.Aggregate(string.Empty, (r, p) => r + p[0].ToString().ToUpper() + p.Substring(1));
        }
    }
}
