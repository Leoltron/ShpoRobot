using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tests
{
    public static class StringBuilderExtensions
    {
        public static void AppendByChar(this StringBuilder builder, StringBuilder another)
        {
            for (int i = 0; i < another.Length; i++)
                builder.Append(another[i]);
        }

        public static int IndexOf(this StringBuilder sb, StringBuilder s)
        {
            if (sb == null || s == null)
                throw new ArgumentNullException();

            for (var i = 0; i < sb.Length; i++)
            {
                int j;
                for (j = 0; j < s.Length && i + j < sb.Length && sb[i + j] == s[j]; j++) ;
                if (j == s.Length)
                    return i;
            }

            return -1;
        }

        public static bool ReplaceOnce(this StringBuilder sb, StringBuilder replaceWhat, StringBuilder replacement)
        {
            var s = sb.ToString();
            var index = s.IndexOf(replaceWhat.ToString(), StringComparison.Ordinal);

            if (index == -1) return false;

            var r = s.Remove(index, replaceWhat.Length).Insert(index, replacement.ToString());
            sb.Clear();
            sb.Append(r);

            return true;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();
            var sb = new StringBuilder();
            for (int i = 0; i < 1000000; i++)
            {
                sb.Append(sb.ToString());
            }
            sw.Stop();
        }


        static void MeasureStringAppendTime(int amount)
        {
            var sw = new Stopwatch();
            sw.Start();
            var a = "Hello";
            var b = "Hello";
            for (int i = 0; i < amount; i++)
            {
                b = a + b;
            }
            sw.Stop();
            Console.WriteLine($"string + string concat x{amount}: {sw.Elapsed}");
        }

        static void MeasureStringBuilderAppendTime(int amount)
        {
            var sw = new Stopwatch();
            sw.Start();
            var a = new StringBuilder("Hello");
            var b = new StringBuilder("Hello");
            for (int i = 0; i < amount; i++)
            {
                a.Append(b);
            }
            sw.Stop();
            Console.WriteLine("StringBuilder + StringBuilder.ToString() append x" + amount + ": " + sw.Elapsed);
        }

        static void MeasureStringBuilderAppendByCharTime(int amount)
        {
            var sw = new Stopwatch();
            sw.Start();
            var a = new StringBuilder("Hello");
            var b = new StringBuilder("Hello");
            for (int i = 0; i < amount; i++)
            {
                a.AppendByChar(b);
            }
            sw.Stop();
            Console.WriteLine("StringBuilder + StringBuilder append x" + amount + ": " + sw.Elapsed);
        }


        static void Sum(IEnumerable<int> a)
        {
            var s = 0;
            Console.WriteLine("Start");
            var e = a.GetEnumerator();
            s += e.Current;
            e.MoveNext();
            s += e.Current;
            Console.WriteLine(s);
        }

        static IEnumerable<int> F()
        {
            Console.WriteLine("F() called");
            yield return 1;
            yield return 2;
            yield return 3;
            yield return 4;
            yield return 5;
            yield return 6;
            yield return 7;
            yield return 8;
            yield return 9;
            yield return 0;
        }
    }
}