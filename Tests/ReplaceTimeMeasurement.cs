using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Tests
{
    class ReplaceTimeMeasurement
    {
        public static void MeasureReplaceTime(int repeatsAmount)
        {
            MeasureStringReplaceTime(repeatsAmount);
            MeasureStringBuilderReplaceTime(repeatsAmount);
            MeasureStringBuilderCustomReplaceTime(repeatsAmount);
            MeasureStringBuilderCustomNoRecreatingReplaceTime(repeatsAmount);
            MeasureStringBuilderToStringReplaceTime(repeatsAmount);
            MeasureStringBuilderToStringNoRecreatingReplaceTime(repeatsAmount);
            MeasureStringBuilderToStringAltNoRecreatingReplaceTime(repeatsAmount);
        }

        private static void MeasureStringBuilderReplaceTime(int amount)
        {
            var a = new StringBuilder("utdfg;kjd;kfg;sdojfHelloWorldafgsdhndggwegwrf");
            var sw = new Stopwatch();
            var stringToReplace = new StringBuilder("HelloWorld");
            sw.Start();
            for (int i = 0; i < amount; i++)
            {
                a.Replace(stringToReplace.ToString(), "HelloWorldHelloWorld", a.ToString().IndexOf(stringToReplace.ToString(), StringComparison.Ordinal), stringToReplace.Length);
            }

            sw.Stop();
            Console.WriteLine($"StringBuilder own replace x{amount}: {sw.Elapsed}");
        }

        private static void MeasureStringReplaceTime(int amount)
        {
            var a = "utdfg;kjd;kfg;sdojfHelloWorldafgsdhndggwegwrf";
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < amount; i++)
                a = new Regex("HelloWorld").Replace(a, "HelloWorldHelloWorld", 1);
            sw.Stop();
            Console.WriteLine($"string regEx replace x{amount}: {sw.Elapsed}");
        }

        private static void MeasureStringAdaptedReplaceTime(int amount)
        {
            var a = "utdfg;kjd;kfg;sdojfHelloWorldafgsdhndggwegwrf";
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < amount; i++)
            {
                var match = new Regex("HelloWorld").Match(a);
            }
            sw.Stop();
            Console.WriteLine($"string regEx replace x{amount}: {sw.Elapsed}");
        }

        private static void MeasureStringBuilderCustomReplaceTime(int amount)
        {
            var a = new StringBuilder("utdfg;kjd;kfg;sdojfHelloWorldafgsdhndggwegwrf");
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < amount; i++)
            {
                var replaceWhat = new StringBuilder("HelloWorld");
                var replacement = new StringBuilder("HelloWorldHelloWorld");
                a.ReplaceOnce(replaceWhat, replacement);
            }

            sw.Stop();
            Console.WriteLine($"StringBuilder custom replace x{amount}: {sw.Elapsed}");
        }

        private static void MeasureStringBuilderCustomNoRecreatingReplaceTime(int amount)
        {
            var a = new StringBuilder("utdfg;kjd;kfg;sdojfHelloWorldafgsdhndggwegwrf");
            var sw = new Stopwatch();
            sw.Start();
            var replaceWhat = new StringBuilder("HelloWorld");
            var replacement = new StringBuilder("HelloWorldHelloWorld");
            for (int i = 0; i < amount; i++)
            {
                a.ReplaceOnce(replaceWhat, replacement);
            }

            sw.Stop();
            Console.WriteLine($"StringBuilder custom replace (no recreating) x{amount}: {sw.Elapsed}");
        }

        private static void MeasureStringBuilderToStringReplaceTime(int amount)
        {
            var a = new StringBuilder("utdfg;kjd;kfg;sdojfHelloWorldafgsdhndggwegwrf");
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < amount; i++)
            {
                var replaceWhat = new StringBuilder("HelloWorld");
                var replacement = new StringBuilder("HelloWorldHelloWorld");
                a = new StringBuilder(
                    new Regex(replaceWhat.ToString()).Replace(a.ToString(), replacement.ToString(), 1));
            }

            sw.Stop();
            Console.WriteLine($"StringBuilder ToString() replace (no recreating) x{amount}: {sw.Elapsed}");
        }

        private static void MeasureStringBuilderToStringNoRecreatingReplaceTime(int amount)
        {
            var a = new StringBuilder("utdfg;kjd;kfg;sdojfHelloWorldafgsdhndggwegwrf");
            var sw = new Stopwatch();
            sw.Start();
            var replaceWhat = new StringBuilder("HelloWorld");
            var replacement = new StringBuilder("HelloWorldHelloWorld");
            for (int i = 0; i < amount; i++)
            {
                a = new StringBuilder(
                    new Regex(replaceWhat.ToString()).Replace(a.ToString(), replacement.ToString(), 1));
            }

            sw.Stop();
            Console.WriteLine($"StringBuilder ToString() RegEx replace x{amount}: {sw.Elapsed}");
        }

        public static string ReplaceFirstOccurrence(string source, string find, string replace)
        {
            var place = source.IndexOf(find, StringComparison.Ordinal);
            var result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }

        private static void MeasureStringBuilderToStringAltNoRecreatingReplaceTime(int amount)
        {
            var a = new StringBuilder("utdfg;kjd;kfg;sdojfHelloWorldafgsdhndggwegwrf");
            var sw = new Stopwatch();
            sw.Start();
            var replaceWhat = new StringBuilder("HelloWorld");
            var replacement = new StringBuilder("HelloWorldHelloWorld");
            for (int i = 0; i < amount; i++)
                a = new StringBuilder(ReplaceFirstOccurrence(a.ToString(), replaceWhat.ToString(),
                    replacement.ToString()));

            sw.Stop();
            Console.WriteLine($"StringBuilder ToString() alt replace x{amount}: {sw.Elapsed}");
        }
    }
}