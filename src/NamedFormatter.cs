using System;
using System.Text.RegularExpressions;

namespace GetBackToWork
{
    /*
     * Caveat emptor! Doesn't escape braces correctly!
     */
    static class NamedFormatter
    {
        private const string REGEX = "{[a-zA-Z0-9_]+(:[^}]+)?}";

        public static string Format(string template, object o)
        {
            Type type = o.GetType();
            MatchCollection matches = Regex.Matches(template, REGEX);

            string result = template;
            object[] data = new object[matches.Count];

            for (int i = 0; i < matches.Count; i ++)
            {
                string propertyName = Regex.Match(matches[i].Value, "[a-zA-Z0-9_]+").Value;
                result = result.Replace(matches[i].Value, matches[i].Value.Replace(propertyName, i.ToString()));
                data[i] = type.GetProperty(propertyName).GetValue(o, null);
            }

            result = String.Format(result, data);
            return result;
        }
    }
}
