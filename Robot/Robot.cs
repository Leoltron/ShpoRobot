using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using exercises;
using NUnit.Framework.Constraints;

namespace RobotTask
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class CommandParserMethodAttribute : Attribute
    {
        public string CommandName { get; }

        public CommandParserMethodAttribute(string commandName)
        {
            CommandName = commandName;
        }
    }

    public class Robot : IRobot
    {
        public Action<string> Write { get; private set; }
        public Func<string> Read { get; private set; }

        public Dictionary<string, int> LabelDictionary;
        public List<string> Stack = new List<string>();

        public string StackHead => Stack[StackHeadIndex];
        public int StackHeadIndex => Stack.Count - 1;

        public List<Action> ProgramInstructions;
        public int InstructionPointer;

        public List<string> Evaluate(List<string> commands, IEnumerable<string> input)
        {
            var enumerator = input.GetEnumerator();
            Read = () =>
            {
                enumerator.MoveNext();
                return enumerator.Current;
            };

            var output = new List<string>();
            Write = s => output.Add(s);

            EvaluateInternal(commands);
            enumerator.Dispose();
            return output;
        }

        public void Evaluate(List<string> commands)
        {
            Read = Console.ReadLine;
            Write = Console.WriteLine;

            EvaluateInternal(commands);
        }

        private void EvaluateInternal(List<string> commands)
        {
            ProgramInstructions = new List<Action>(commands.Count);
            LabelDictionary = new Dictionary<string, int>();
            ProgramInstructions = new CommandParser(this).ParseCommands(commands);

            for (InstructionPointer = 0; InstructionPointer < ProgramInstructions.Count; InstructionPointer++)
            {
                ProgramInstructions[InstructionPointer](); /*
                Console.WriteLine("DEBUG: Executed command: " + 
                    (commands[InstructionPointer].Length > 30 ? commands[InstructionPointer].Substring(0,30)+"..." : commands[InstructionPointer]));
                Console.WriteLine("DEBUG: \tStack: ");
                for (int i = StackHeadIndex; i >= 0; i--)
                    Console.WriteLine("DEBUG: \t\t" + Stack[i]);*/
            }
        }

        public void Push(string s)
        {
            Stack.Add(s);
        }

        public string Pop()
        {
            var head = StackHead;
            Stack.RemoveAt(StackHeadIndex);
            return head;
        }

        public void Swap(int indexA, int indexB)
        {
            var listIndexA = StackHeadIndex - indexA;
            var listIndexB = StackHeadIndex - indexB;

            var reserved = Stack[listIndexA];
            Stack[listIndexA] = Stack[listIndexB];
            Stack[listIndexB] = reserved;
        }

        public void CopyAndPush(int index) => Push(Stack[StackHeadIndex - index]);

        public void ReadAndPush() => Push(Read());

        public void WriteStackHead() => Write(StackHead);

        public void JumpToStackHeadLabel()
        {
            var label = StackHead;
            JumpToLabel(label);
            Pop();
        }

        private void JumpToLabel(string label)
        {
            if (!LabelDictionary.ContainsKey(label))
                throw new KeyNotFoundException($"Метки {label} не существует");
            InstructionPointer = LabelDictionary[label];
        }

        public void ConcatTwoOnStackHead()
        {
            var a = Pop();
            var b = Pop();

            Push(string.Concat(a, b));
        }

        public void ReplaceOne()
        {
            var str = Pop();                //a
            var pattern = Pop();            //b
            var replacement = Pop();        //c
            var failureReturnLabel = Pop(); //ret

            var ind = str.IndexOf(pattern);
            if (ind == -1)
                JumpToLabel(failureReturnLabel);
            else
                str =str.Remove(ind, pattern.Length).Insert(ind, replacement);
            Push(str);

            //Console.WriteLine($"DEBUG: Replacing in {a.Length} ({b.Length} to {c.Length})  Success: {ind}, ret:{ret}");
        }

        private static string ReplaceAt(string str, int start, int length, string replacement)
        {
            var sb = new StringBuilder(str);
            sb.Remove(start, length);
            sb.Insert(start, replacement);
            return sb.ToString();
        }
    }

    internal class CommandParser
    {
        private static readonly Action emptyAction = () => { };

        private readonly Robot robot;
        private readonly Dictionary<string, Func<string, Action>> parsers;

        private List<Tuple<int, string>> fixedJumps;
        private int currentIndex;

        public CommandParser(Robot robot)
        {
            this.robot = robot;
            parsers = new Dictionary<string, Func<string, Action>>();

            foreach (var method in typeof(CommandParser).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            foreach (var attribute in method.GetCustomAttributes(typeof(CommandParserMethodAttribute), false))
                if (attribute is CommandParserMethodAttribute a)
                {
                    CheckParserMethod(method);
                    parsers.Add(a.CommandName, s => method.Invoke(this, new object[] {s}) as Action);
                }
        }

        private static void CheckParserMethod(MethodInfo method)
        {
            if (!IsParserMethod(method))
                throw new FormatException($"Методы с атрибутом {nameof(CommandParserMethodAttribute)} должны" +
                                          $"принимать строку и возвращать Action, метод {method.Name} нарушает эти условия.");
        }

        private static bool IsParserMethod(MethodInfo method)
        {
            var parameters = method.GetParameters();
            return parameters.Length == 1
                   && parameters[0].ParameterType == typeof(string)
                   && method.ReturnType == typeof(Action);
        }

        public List<Action> ParseCommands(List<string> stringList)
        {
            var commands = FilterCommands(stringList);
            var parsedCommands = ParseFilteredCommands(commands);
            DecodeFixedJumps(parsedCommands);

            return parsedCommands;
        }

        private static List<string> FilterCommands(List<string> rawCommandStrings)
        {
            return rawCommandStrings
                .Where(s => !string.IsNullOrEmpty(s) && !char.IsWhiteSpace(s[0]))
                .ToList();
        }

        private List<Action> ParseFilteredCommands(List<string> commands)
        {
            fixedJumps = new List<Tuple<int, string>>();
            var parsedCommands = new List<Action>(commands.Count);
            for (currentIndex = 0; currentIndex < commands.Count; currentIndex++)
                parsedCommands.Add(ParseCommandString(commands[currentIndex]));
            return parsedCommands;
        }

        private void DecodeFixedJumps(IList<Action> parsedCommands)
        {
            foreach (var fixedJump in fixedJumps)
            {
                var index = fixedJump.Item1;
                var label = fixedJump.Item2;
                parsedCommands[index] = () => robot.InstructionPointer = robot.LabelDictionary[label];
            }
        }

        private Action ParseCommandString(string line)
        {
            var splittedLine = line.Split(new[] {' '}, 2);
            var command = splittedLine[0];
            var argString = splittedLine.Length == 1 ? string.Empty : splittedLine[1];

            if (!parsers.ContainsKey(command)) throw new ArgumentException($"Команда {command} не найдена");

            return parsers[command](argString);
        }

        [CommandParserMethod("POP")]
        private Action ParsePop(string argString)
        {
            AssertCommandArgsEmpty(argString, "POP");
            return () => robot.Pop();
        }

        private static readonly Regex pushArgumentRegEx = new Regex("'((''|[^'])*)'");

        [CommandParserMethod("PUSH")]
        private Action ParsePush(string argString)
        {
            var match = pushArgumentRegEx.Match(argString);
            CheckPushMatch(argString, match);
            var stringToPush = match.Groups[1].Value.Replace("''", "'");
            return () => robot.Push(stringToPush);
        }

        private static void CheckPushMatch(string argString, Match match)
        {
            if (argString == null) throw new ArgumentNullException(nameof(argString));
            if (match == null) throw new ArgumentNullException(nameof(match));

            if (!match.Success)
                throw new FormatException("Строка должна быть заключена в одиночные кавычки!");

            if (match.Length != argString.Length)
                throw new FormatException("Внутри строки не должно быть одиночных кавычек!");
        }

        [CommandParserMethod("WRITE")]
        private Action ParseWrite(string argString)
        {
            AssertCommandArgsEmpty(argString, "WRITE");
            return robot.WriteStackHead;
        }

        [CommandParserMethod("READ")]
        private Action ParseRead(string argString)
        {
            AssertCommandArgsEmpty(argString, "READ");
            return robot.ReadAndPush;
        }

        private static readonly Regex swapArgumentRegEx = new Regex(@"^([\d]+) ([\d]+)$");

        [CommandParserMethod("SWAP")]
        private Action ParseSwap(string argString)
        {
            var match = swapArgumentRegEx.Match(argString);
            if (!match.Success)
                throw new FormatException();

            var a = int.Parse(match.Groups[1].Value);
            var b = int.Parse(match.Groups[2].Value);

            return () => robot.Swap(a - 1, b - 1);
        }

        private static readonly Regex copyArgumentRegEx = new Regex(@"^([\d]+)$");

        [CommandParserMethod("COPY")]
        private Action ParseCopy(string argString)
        {
            var match = copyArgumentRegEx.Match(argString);
            if (!match.Success)
                throw new FormatException();

            var index = int.Parse(match.Groups[1].Value);
            return () => robot.CopyAndPush(index - 1);
        }

        [CommandParserMethod("LABEL")]
        private Action ParseLabel(string argString)
        {
            if (string.IsNullOrEmpty(argString))
                throw new FormatException("Аргумент команды LABEL не может быть пустым!");

            robot.LabelDictionary.Add(argString, currentIndex);
            return emptyAction;
        }

        [CommandParserMethod("JMP")]
        private Action ParseJump(string argString)
        {
            if (string.IsNullOrEmpty(argString))
                return robot.JumpToStackHeadLabel;
            fixedJumps.Add(Tuple.Create(currentIndex, argString));
            return emptyAction;
        }

        [CommandParserMethod("CONCAT")]
        private Action ParseConcat(string argString)
        {
            AssertCommandArgsEmpty(argString, "CONCAT");
            return robot.ConcatTwoOnStackHead;
        }

        [CommandParserMethod("REPLACEONE")]
        private Action ParseReplaceOne(string argString)
        {
            AssertCommandArgsEmpty(argString, "REPLACEONE");
            return robot.ReplaceOne;
        }

        private static void AssertCommandArgsEmpty(string argString, string commandName)
        {
            if (!string.IsNullOrEmpty(argString))
                throw new ArgumentException($"Команда {commandName} не принимает аргументов");
        }
    }
}