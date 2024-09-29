using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace UltimateStorageSystem.Utilities.Extensions
{
    public static class StringExtensions
    {
        public static string ToSnakeCase(this string @string)
        {
            var output = new StringBuilder();
            foreach (Match match in new Regex("(?:([A-Z]?[a-z]+)[ _-]?)+").Matches(@string)) {
                (int i, string Value)[] matches = match.Groups.Values.Where(x => x.Value != @string).Select((x, i) => (i, x.Value)).ToArray();
                foreach ((int index, string groupValue) in matches) {
                    output.Append(groupValue.ToLower());
                    if (index < matches.Length) {
                        output.Append('_');
                    }
                }
            }
            return output.ToString();
        }
    }
}
