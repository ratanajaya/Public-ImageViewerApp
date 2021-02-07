using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SharedLibrary
{
    public static class StringHelper
    {
        public static string UriFriendlyBase64Decode(this string cleanedBase64) {
            var regularBase64 = cleanedBase64.Replace('-', '+').Replace('_', '/');
            var base64Bytes = Convert.FromBase64String(regularBase64);
            var plainText = Encoding.UTF8.GetString(base64Bytes);
            return plainText;
        }

        public static string UriFriendlyBase64Encode(this string plainText) {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var regularBase64 = Convert.ToBase64String(plainTextBytes);
            var cleanedBase64 = regularBase64.Replace('+', '-').Replace('/', '_');
            return cleanedBase64;
        }

        #region Collection
        public static bool Contains(this string source, string toCheck, StringComparison comp) {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static bool ContainsAny(this string haystack, params string[] needles) {
            foreach (string needle in needles) {
                if (haystack.ContainsIgnoreCase(needle)) {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsContains(this string[] haystack, string needle) {
            foreach (string s in haystack) {
                if (s.ToLower().Contains(needle.ToLower())) {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainsContains(this List<string> haystack, string needle) {
            foreach(string s in haystack) {
                if(s.ToLower().Contains(needle.ToLower())) {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainsIgnoreCase(this string haystack, string needle) {
            if (haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0) {
                return true;
            }
            return false;
        }

        public static bool Contains(this IEnumerable<string> source, string toCheck, StringComparison comp) {
            foreach (string s in source) {
                if (s.Contains(toCheck, comp)) {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainsExact(this IEnumerable<string> source, string toCheck, StringComparison comp) {
            foreach (string s in source) {
                if(s.Equals(toCheck,comp)) {
                    return true;
                }
            }
            return false;
        }

        public static IOrderedEnumerable<T> OrderByAlphaNumeric<T>(this IEnumerable<T> source, Func<T, string> selector) {
            int max = source
                .SelectMany(i => Regex.Matches(selector(i), @"\d+").Cast<Match>().Select(m => (int?)m.Value.Length))
                .Max() ?? 0;

            return source.OrderBy(i => Regex.Replace(selector(i), @"\d+", m => m.Value.PadLeft(max, '0')));
        }
        #endregion

        #region CloudAPI
        public static string RemoveNonLetterDigit(this string source) {
            string result = new string(source.Where(x => char.IsLetterOrDigit(x)).ToArray());

            return result;
        }

        public static string MapToUpperAlphanumericString(this string source) {
            string result = "";
            foreach(char c in source) {
                result += c.MapToUpperAlphanumericChar();
            }
            return result;
        }

        //Take any Unicode character and return A-Z0-9 character
        public static char MapToUpperAlphanumericChar(this char source) {
            char result;
            //WARNING: Shotgun surgery with MapToUpperAlphanumericChar(this int source)
            const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            if(AllowedChars.Contains(source.ToString().ToUpper())) {
                result = source.ToString().ToUpper().First();
            }
            else {
                int charCode = source;
                int charMapIndex = charCode % AllowedChars.Length;

                result = AllowedChars[charMapIndex];
            }

            return result;
        }

        //WARNING: Shotgun surgery with MapToUpperAlphanumericChar
        public static char MapToUpperAlphanumericChar(this int source) {
            //WARNING: Shotgun surgery with MapToUpperAlphanumericChar(this char source)
            const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            int charMapIndex = source % AllowedChars.Length;

            char result = AllowedChars[charMapIndex];

            return result;
        }

        public static string TakeUppercaseAlphanumeric(this string source, int length) {
            source = source.RemoveNonLetterDigit().MapToUpperAlphanumericString();

            var sb = new StringBuilder("");
            for(int i = 0; i < length; i++) {
                sb.Append("0");
            }

            int maxIteration = source.Length >= length ? length : source.Length;
            for(int i = 0; i < maxIteration; i++) {
                sb[i] = source[i];
            }

            string result = sb.ToString();
            return result;
        }

        #endregion

        #region ViewerApp
        public static string ActionType(this string action) {
            return action.Substring(0, action.IndexOf(':'));
        }

        public static string ActionQuery(this string action) {
            return action.Substring(action.IndexOf(':') + 1, action.Length - (action.IndexOf(':') + 1));
        }
        #endregion
    }
}
