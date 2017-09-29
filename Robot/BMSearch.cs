using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exercises
{
    static class BMSearch
    {
        public static int BMIndexOf(this string str, string pattern)
        {
            return bmSearch(str, pattern);
        }

        private static int[] PrefixFunction(string str)
        {
            var pi = new int[str.Length];
            pi[0] = 0;
            var k = 0;
            for (var i = 1; i < str.Length; i++)
            {
                while (k > 0 && str[k] != str[i])
                    k = pi[k - 1];
                if (str[k] == str[i])
                    k++;
                pi[i] = k;
            }
            return pi;
        }

        private static int[] SuffixFunction(string str)
        {
            return PrefixFunction(reverseString(str));
        }

        private static string reverseString(string str)
        {
            var sb = new StringBuilder();
            for (var i = str.Length - 1; i >= 0; i--)
                sb.Append(str[i]);
            return sb.ToString();
        }

        public static int bmSearch(string str, string pattern)
        {
            var suffixTable = buildSuffixTable(pattern);
            suffixTable = suffixTable.Reverse().ToArray();
            var symbolsTable = BuildStopSymbolsTable(pattern);

            if (str.Length < pattern.Length)
                return -1;

            var patternLen = pattern.Length;
            for (var i = patternLen - 1; i < str.Length;)
            {
                var countResult = CountEqualSymbols(str, pattern, i);

                if (countResult.Item1 == patternLen)
                    return (i - patternLen + 1);
                var suffixShift = suffixTable[countResult.Item1];
                var stopSymbolShift = countResult.Item2 + 1
                                      - ElementAtKeyOrDefault(
                                          symbolsTable,
                                          str[countResult.Item3], 0);
                i += Math.Max(suffixShift, stopSymbolShift);
            }
            return -1;
        }

        private static Tuple<int, int, int> CountEqualSymbols(string str, string pattern, int startIndex)
        {
            var patternLen = pattern.Length;
            var equalSymbolsCount = 0;

            int stopPatternIndex = 0;
            int stringIndex = startIndex;
            for (;
                stringIndex > startIndex - patternLen;
                stringIndex--)
            {
                stopPatternIndex = patternLen - 1 - equalSymbolsCount;
                if (str[stringIndex] == pattern[stopPatternIndex])
                    equalSymbolsCount++;
                else
                    break;
            }
            return Tuple.Create(
                equalSymbolsCount,
                stopPatternIndex,
                stringIndex
            );
        }

        private static int[] buildSuffixTable(string pattern)
        {
            var m = pattern.Length;
            var suffixShiftTable = new int[m + 1];

            var pi = new int[m + 1];
            PrefixFunction(pattern).CopyTo(pi, 1);
            var pi1 = new int[m + 1];
            SuffixFunction(pattern).CopyTo(pi1, 1);

            int j;
            for (j = 0; j <= m; j++)
                suffixShiftTable[j] = m - pi[m];
            for (var i = 1; i <= m; i++)
            {
                j = m - pi1[i];
                suffixShiftTable[j] = Math.Min(suffixShiftTable[j], i - pi1[i]);
            }
            return suffixShiftTable;
        }

        private static Dictionary<char, int> BuildStopSymbolsTable(string pattern)
        {
            var table = new Dictionary<char, int>();
            for (var i = pattern.Length - 2; i >= 0; i--)
            {
                var ch = pattern[i];
                if (!table.ContainsKey(ch))
                    table[ch] = i + 1;
            }
            return table;
        }

        private static V ElementAtKeyOrDefault<K, V>(this IReadOnlyDictionary<K, V> dict, K key, V defaultValue)
        {
            return dict.ContainsKey(key) ? dict[key] : defaultValue;
        }
    }
}