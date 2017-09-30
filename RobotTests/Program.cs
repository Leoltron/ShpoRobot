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
            const string path = @"C:\Users\Леонид\Source\Repos\ShpoRobot\RobotTests\timer.txt";
            new Robot().Evaluate(File.ReadAllLines(path).ToList());
        }

        private static void RunOptimisationTests()
        {
            for (int i = 1; i < 4; i++)
            {
                var l = new List<string>();
                var lAltR = new List<string>();
                var lAltI = new List<string>();
                var lAltC = new List<string>();
                const string dirpath = @"C:\Users\Леонид\Source\Repos\ShpoRobot\RobotTests\ForOptimizations";
                foreach (var path in Directory.GetFiles(dirpath))
                {
                    string testName;
                    string elapsedTimeString;
                    Stopwatch sw;
                    /*
                    Robot.AltReplace = false;
                    Robot.AltIndexOf = false;

                    var testName = path.Substring(path.LastIndexOf("\\") + 1) + ":";
                    Console.WriteLine(testName);
                    l.Add(testName);
                    var sw = Stopwatch.StartNew();
                    l.AddRange(new Robot().Evaluate(File.ReadAllLines(path).ToList(), new List<string>()));
                    sw.Stop();
                    var elapsedTimeString = "\tTime elapsed: " + sw.Elapsed;
                    Console.WriteLine(elapsedTimeString);
                    l.Add(elapsedTimeString);

                    Robot.AltReplace = true;
                    Robot.AltIndexOf = false;

                    testName = path.Substring(path.LastIndexOf("\\") + 1) + " (Alt Replace) :";
                    Console.WriteLine(testName);
                    lAltR.Add(testName);
                    sw = Stopwatch.StartNew();
                    lAltR.AddRange(new Robot().Evaluate(File.ReadAllLines(path).ToList(), new List<string>()));
                    sw.Stop();
                    elapsedTimeString = "\tTime elapsed: " + sw.Elapsed;
                    Console.WriteLine(elapsedTimeString);
                    lAltR.Add(elapsedTimeString);
                    
                    Robot.AltReplace = false;
                    Robot.AltIndexOf = true;

                    testName = path.Substring(path.LastIndexOf("\\") + 1) + " (Alt IndexOf) :";
                    Console.WriteLine(testName);
                    lAltI.Add(testName);
                    sw = Stopwatch.StartNew();
                    lAltI.AddRange(new Robot().Evaluate(File.ReadAllLines(path).ToList(), new List<string>()));
                    sw.Stop();
                    elapsedTimeString = "\tTime elapsed: " + sw.Elapsed;
                    Console.WriteLine(elapsedTimeString);
                    lAltI.Add(elapsedTimeString);

                    Robot.AltReplace = true;
                    Robot.AltIndexOf = true;
                    */
                    for (int j = 0; j < 5; j++)
                    {
                        
                    testName = path.Substring(path.LastIndexOf("\\") + 1) + " (Alt Both) :";
                    Console.WriteLine(testName);
                    lAltC.Add(testName);
                    sw = Stopwatch.StartNew();
                    lAltC.AddRange(new Robot().Evaluate(File.ReadAllLines(path).ToList(), new List<string>()));
                    sw.Stop();
                    elapsedTimeString = "\tTime elapsed: " + sw.Elapsed;
                    Console.WriteLine(elapsedTimeString);
                    lAltC.Add(elapsedTimeString);
                    }
                }
               // File.WriteAllLines(i + "OptTestResults.txt", l.ToArray());
                //File.WriteAllLines(i + "OptTestResults (Alt Replace).txt", lAltR.ToArray());
                //File.WriteAllLines(i + "OptTestResults (Alt IndexOf).txt", lAltI.ToArray());
                File.WriteAllLines(i + "OptTestResults (Alt Both).txt", lAltC.ToArray());
            }
        }
    }
}