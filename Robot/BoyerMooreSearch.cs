using System;
using System.Collections.Generic;
using System.Linq;

namespace exercises
{
    internal static class BoyerMooreSearch
    {
        public static int BoyerMooreIndexOf(this string str, string pattern)
        {
            if (str.Length < pattern.Length)
                return -1;

            var suffixTable = BuildSuffixTable(pattern).Reverse().ToArray();

            var stopSymbolsTable = BuildStopSymbolsTable(pattern);

            for (var i = pattern.Length - 1; i < str.Length;)
            {
                var countResult = CountEqualSymbolsFromEnd(str, pattern, i);

                if (countResult.Item1 == pattern.Length)
                    return i - pattern.Length + 1;

                var suffixShift = suffixTable[countResult.Item1];
                var stopSymbolShift = countResult.Item2 + 1 -
                                      ElementAtOrDefault(stopSymbolsTable, str[countResult.Item3], 0);

                i += Math.Max(suffixShift, stopSymbolShift);
            }
            return -1;
        }

        private static int[] GetPrefixFunction(string str)
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

        private static int[] GetSuffixFunction(string str)
        {
            return GetPrefixFunction(ReverseString(str));
        }

        private static string ReverseString(string str)
        {
            return new string(str.ToCharArray().Reverse().ToArray());
        }

        private static Tuple<int, int, int> CountEqualSymbolsFromEnd(string str, string pattern, int endIndex)
        {
            var patternLen = pattern.Length;
            var equalSymbolsCount = 0;

            var stopPatternIndex = 0;
            var i = endIndex;
            var startIndex = endIndex - patternLen;

            for (; i > startIndex; i--)
            {
                stopPatternIndex = patternLen - 1 - equalSymbolsCount;
                if (str[i] == pattern[stopPatternIndex])
                    equalSymbolsCount++;
                else
                    break;
            }
            return Tuple.Create(equalSymbolsCount, stopPatternIndex, i);
        }

        private static int[] BuildSuffixTable(string pattern)
        {
            var patternLength = pattern.Length;
            var suffixShiftTable = new int[patternLength + 1];

            var prefFunc = new int[patternLength + 1];
            GetPrefixFunction(pattern).CopyTo(prefFunc, 1);
            var suffixFunc = new int[patternLength + 1];
            GetSuffixFunction(pattern).CopyTo(suffixFunc, 1);

            int j;
            for (j = 0; j <= patternLength; j++)
                suffixShiftTable[j] = patternLength - prefFunc[patternLength];
            for (var i = 1; i <= patternLength; i++)
            {
                j = patternLength - suffixFunc[i];
                suffixShiftTable[j] = Math.Min(suffixShiftTable[j], i - suffixFunc[i]);
            }
            return suffixShiftTable;
        }

        private static Dictionary<char, int> BuildStopSymbolsTable(string pattern)
        {
            var table = new Dictionary<char, int>();
            for (var i = pattern.Length - 2; i >= 0; i--)
            {
                var charAtI = pattern[i];
                if (!table.ContainsKey(charAtI))
                    table[charAtI] = i + 1;
            }
            return table;
        }

        private static TValue ElementAtOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict,
            TKey key, TValue defaultValue)
        {
            return dict.ContainsKey(key) ? dict[key] : defaultValue;
        }
    }
}