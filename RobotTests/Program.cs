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
            RunTimer();
            //RunOptimisationTests();
        }

        private static void RunTimer()
        {
            const string path = @"C:\Users\Leoltron\Documents\Visual Studio 2017\Projects\Robot\RobotTests\timer.txt";
            new Robot().Evaluate(File.ReadAllLines(path).ToList());
        }

        private static void RunOptimisationTests()
        {
            var l = new List<string>();
            var dirpath = @"C:\Users\Leoltron\Documents\Visual Studio 2017\Projects\Robot\RobotTests\ForOptimizations";
            foreach (var path in Directory.GetFiles(dirpath))
            {
                var testName = path.Substring(path.LastIndexOf("\\") + 1) + ":";
                Console.WriteLine(testName);
                l.Add(testName);
                var sw = Stopwatch.StartNew();
                l.AddRange(new Robot().Evaluate(File.ReadAllLines(path).ToList(), new List<string>()));
                sw.Stop();
                var elapsedTimeString = "\tTime elapsed: " + sw.Elapsed;
                Console.WriteLine(elapsedTimeString);
                l.Add(elapsedTimeString);
            }
            File.WriteAllLines("OptimisationTestResults.txt", l.ToArray());
        }
    }
}