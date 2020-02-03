using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibrary.Helpers
{
    public static class Extensions
    {
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
