using System;

namespace RobotTask
{
    internal static class StringExtensions
    {
        private static long GetPolynomialHash(this string s)
        {
            return s.GetPolynomialHash(0, s.Length);
        }

        private static long GetPolynomialHash(this string s, int startIndex, int endIndex)
        {
            var result = 0;
            for (var i = startIndex; i < endIndex; i++)
                result = (result << 1) + s[i];
            return result;
        }

        public static int RabinKarpIndexOf(this string str, string pattern)
        {
            Console.WriteLine(pattern.Length);
            var patternHash = pattern.GetPolynomialHash();
            var sum = str.GetPolynomialHash(0, pattern.Length);
            for (var i = 0; i < str.Length - pattern.Length + 1; i++)
            {
                if (i != 0)
                    sum = ((sum - (str[i - 1] << (pattern.Length - 1))) << 1)
                          + str[i + pattern.Length - 1];
                if (patternHash == sum && pattern.IsEqualToSubstring(str, i))
                    return i;
            }
            return -1;
        }

        private static bool IsEqualToSubstring(this string s, string source, int start)
        {
            if (s.Length > source.Length)
                return false;
            for (var i = 0; i < s.Length; i++)
                if (s[i] != source[start + i])
                    return false;
            return true;
        }
    }
}