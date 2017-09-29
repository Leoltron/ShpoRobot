using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using RobotTask;

namespace RobotTests
{
    class Program
    {
        static void Main(string[] args)
        {
            //Robot.UseAlternativeReplaceAt = true;
            //RunTimer();
            RunOptimisationTests();
        }

        private static void RunTimer()
        {
            const string path = @"C:\Users\Leoltron\Documents\Visual Studio 2017\Projects\Robot\RobotTests\timer.txt";
            new Robot().Evaluate(File.ReadAllLines(path).ToList());
        }

        private static void RunOptimisationTests()
        {
            var l = new List<string>();
            var lAlt = new List<string>();
            const string dirpath = @"C:\Users\Leoltron\Documents\Visual Studio 2017\Projects\Robot\RobotTests\ForOptimizations";
            foreach (var path in Directory.GetFiles(dirpath))
            {
                //Robot.UseAlternativeReplaceAt = false;

                var testName = path.Substring(path.LastIndexOf("\\") + 1) + ":";
                Console.WriteLine(testName);
                l.Add(testName);
                var sw = Stopwatch.StartNew();
                l.AddRange(new Robot().Evaluate(File.ReadAllLines(path).ToList(), new List<string>()));
                sw.Stop();
                var elapsedTimeString = "\tTime elapsed: " + sw.Elapsed;
                Console.WriteLine(elapsedTimeString);
                l.Add(elapsedTimeString);
                continue;
                //Robot.UseAlternativeReplaceAt = true;

                testName = path.Substring(path.LastIndexOf("\\") + 1) + " (Alt ReplaceAt) :";
                Console.WriteLine(testName);
                lAlt.Add(testName);
                sw = Stopwatch.StartNew();
                lAlt.AddRange(new Robot().Evaluate(File.ReadAllLines(path).ToList(), new List<string>()));
                sw.Stop();
                elapsedTimeString = "\tTime elapsed: " + sw.Elapsed;
                Console.WriteLine(elapsedTimeString);
                lAlt.Add(elapsedTimeString);
            }
            File.WriteAllLines("OptTestResults (Adaptive).txt", l.ToArray());
            //File.WriteAllLines("OptTestResults (Alt).txt", lAlt.ToArray());
        }
    }
}