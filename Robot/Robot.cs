using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

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
        public List<StringBuilder> Stack = new List<StringBuilder>();
        public StringBuilder StackHead => Stack[StackHeadIndex];
        public int StackHeadIndex;

        public List<Action> ProgramActions;
        public int Pointer;

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
            ProgramActions = new List<Action>(commands.Count);
            LabelDictionary = new Dictionary<string, int>();
            StackHeadIndex = -1;
            ProgramActions = new CommandParser(this).ParseCommands(commands);

            for (Pointer = 0; Pointer < ProgramActions.Count; Pointer++)
            {
                ProgramActions[Pointer](); /*
                Console.WriteLine("DEBUG: Executed command: " + 
                    (commands[Pointer].Length > 30 ? commands[Pointer].Substring(0,30)+"..." : commands[Pointer]));
                Console.WriteLine("DEBUG: \tStack: ");
                for (int i = StackHeadIndex; i >= 0; i--)
                    Console.WriteLine("DEBUG: \t\t" + Stack[i]);*/
            }
        }

        public void Push(string s)
        {
            StackHeadIndex++;
            if (Stack.Count > StackHeadIndex)
            {
                Stack[StackHeadIndex].Clear();
                Stack[StackHeadIndex].Append(s);
            }
            else
                Stack.Add(new StringBuilder(s));
        }

        public void Pop()
        {
            StackHeadIndex--;
        }

        public string PopAndReturn()
        {
            var head = StackHead.ToString();
            Pop();
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

        public void CopyAndPush(int index)
        {
            Push(Stack[StackHeadIndex - index].ToString());
        }

        public void ReadAndPush()
        {
            Push(Read());
        }

        public void WriteStackHead()
        {
            Write(StackHead.ToString());
        }

        public void JumpToStackHeadLabel()
        {
            var label = StackHead.ToString();
            JumpToLabel(label);
            Pop();
        }

        private void JumpToLabel(string label)
        {
            if (!LabelDictionary.ContainsKey(label))
                throw new KeyNotFoundException($"Метки {label} не существует");
            Pointer = LabelDictionary[label];
        }

        public void ConcatTwoOnStackHead()
        {
            var a = StackHead;

            var bIndex = StackHeadIndex - 1;
            var b = Stack[bIndex];

            a.Append(b);

            Swap(0, 1);
            Pop();
        }

        public void ReplaceOne()
        {
            Swap(0, 3);
            var ret = PopAndReturn();
            var b = PopAndReturn();
            var c = PopAndReturn();
            var a = StackHead;

            var ind = a.ToString().IndexOf(b, StringComparison.Ordinal);
            if (ind == -1)
                JumpToLabel(ret);
            else
                a.Replace(b,c,ind,b.Length);

            //Console.WriteLine($"DEBUG: Replacing in {a} ({b} to {c})  Success: {isSuccessfull}, ret:{ret}");
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
                        if(!IsParserMethod(method))
                            throw new FormatException($"Методы с атрибутом {nameof(CommandParserMethodAttribute)} должны" +
                                                      $"принимать строку и возвращать Action, метод {method.Name} нарушает эти условия.");
                        parsers.Add(a.CommandName, s => method.Invoke(this, new object[] {s}) as Action);
                    }
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
            var commands = stringList
                .Where(commandString => !string.IsNullOrEmpty(commandString) && !char.IsWhiteSpace(commandString[0]))
                .ToList();
            var parsedCommands = new List<Action>(commands.Count);
            fixedJumps = new List<Tuple<int, string>>();
            for (currentIndex = 0; currentIndex < commands.Count; currentIndex++)
                parsedCommands.Add(Parse(commands[currentIndex]));

            foreach (var fixedJump in fixedJumps)
            {
                var index = fixedJump.Item1;
                var label = fixedJump.Item2;
                parsedCommands[index] = () => robot.Pointer = robot.LabelDictionary[label];
            }

            return parsedCommands;
        }

        private Action Parse(string line)
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
            return robot.Pop;
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