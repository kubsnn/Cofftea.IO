using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Cofftea.IO
{
    public partial class CoffeeString // Input; CoffeeString.cs - Output
    {
        public static Dictionary<string, List<string>> TempCommands { get; set; }
        static Dictionary<string, List<string>> autocompletions;
        static List<string> prevInput;
        static int prevInputPos;
        static StringBuilder input;
        static int pos { 
            get { 
                return (Console.CursorTop - beginTop) * Console.WindowWidth + Console.CursorLeft; 
            } 
            set {
                Console.CursorTop = beginTop + value / Console.WindowWidth;
                Console.CursorLeft = value % Console.WindowWidth;
            } 
        }
        static int top { get => Console.CursorTop; set => Console.CursorTop = value; }
        static int beginTop;
        static int prevPos;
        static int beginPos;
        static int repeatCount;
        static string lastCompleted;
        static CoffeeString()
        {
            autocompletions = new Dictionary<string, List<string>>();
            prevInput = new List<string>();
            TempCommands = new Dictionary<string, List<string>>();
        }
        public static void AddHintElement(string command, string arg2hint)
        {
            if (!autocompletions.ContainsKey(command)) autocompletions.Add(command, new List<string>());
            autocompletions[command.ToLower()].Add(arg2hint.ToLower());
        }
        public static void HintsClear(string command)
        {
            if (autocompletions.ContainsKey(command)) autocompletions[command].Clear();
        }
        private enum UpdateType
        {
            RefreshAll,
            RefreshInput,
            None
        }
        public static string ReadLine()
        {
            PrepareVariables();
            
            while (true) {
                var key = ReadKey();

                if (key.Key == ConsoleKey.Enter) break;

                var refresh = HandleKey(key);

                if (refresh == UpdateType.None) continue;

                ClearCurrentInput();
                PrintColoredInput();

                if (refresh == UpdateType.RefreshAll) prevInput[prevInput.Count - 1] = input.ToString();
            }

            Console.WriteLine();
            return input.ToString();
        }
        #region Handle input
        private static UpdateType HandleKey(ConsoleKeyInfo key)
        {
            bool ctrl = key.Modifiers.HasFlag(ConsoleModifiers.Control);
            bool shift = key.Modifiers.HasFlag(ConsoleModifiers.Shift);
            bool alt = key.Modifiers.HasFlag(ConsoleModifiers.Alt);

            if (key.Key == ConsoleKey.Tab) {
                TryAutocomplete(shift);
                prevPos = pos;
                return UpdateType.RefreshAll;
            }
            repeatCount = 0;
           

            if (key.Key == ConsoleKey.Backspace) {
                HandleBackspace(ctrl);
            } else if (key.Key == ConsoleKey.Delete) {
                HandleDelete(ctrl);
            } else if (key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.RightArrow) {
                CursorMove(key);
                return UpdateType.None;
            } else if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.DownArrow) {
                SwapWithPrevInput(key.Key);
                prevPos = pos;
                return UpdateType.RefreshInput;
            } else if (IsKeyAChar(key.KeyChar)) {
                input.Insert(pos - beginPos, key.KeyChar);
                pos++;
            } else {
                return UpdateType.None;
            }

            prevPos = pos;
            return UpdateType.RefreshAll;
        }

        private static ConsoleKeyInfo ReadKey()
        {
            pos = prevPos;
            Console.CursorVisible = true;

            var key = Console.ReadKey(true);
            Console.CursorVisible = false; //removes cursor flickering
            return key;
        }
        #endregion
        #region Autocomplete
        static void TryAutocomplete(bool shift)
        {
            string command = GetCommand().ToLower();

            if (pos - beginPos <= command.Length) {
                TryAutocompleteCommand(command, shift);
                return;
            }
            if (!autocompletions.ContainsKey(command)) return;
            

            if (repeatCount > 0) {
                ReAutocomplete(command, shift);
                return;
            }

            if (shift) return;

            string word = GetStringUnderCursor().ToLower();
            
            foreach (var item in autocompletions[command]) {
                if (item.StartsWith(word)) {
                    Autocomplete(item);
                    lastCompleted = word;
                    return;
                }
            }
        }
        static void ReAutocomplete(string command, bool shift)
        {
            int counter = -1;
            if (shift) repeatCount = Math.Max(repeatCount - 2, 0);
            foreach (var item in autocompletions[command]) {
                if (item.StartsWith(lastCompleted)) {
                    counter++;
                    if (counter == repeatCount) {
                        Autocomplete(item);
                        return;
                    }
                }
            }
        }
        static void TryAutocompleteCommand(string command, bool shift)
        {
            if (repeatCount > 0) {
                ReAutocompleteCommand(shift);
                return;
            }

            string word = GetStringUnderCursor().ToLower();

            foreach (var list in TempCommands.Values) {
                foreach (string s in list) {
                    if (s.StartsWith(command)) {
                        Autocomplete(s);
                        lastCompleted = word;
                        return;
                    }
                }   
            }
            if (command.Length == 0) return;
            foreach (string s in autocompletions.Keys) {
                if (s.StartsWith(command)) {
                    Autocomplete(s);
                    lastCompleted = word;
                    return;
                }
            }
        }
        static void ReAutocompleteCommand(bool shift)
        {
            int counter = -1;
            if (shift) repeatCount = Math.Max(repeatCount - 2, 0);

            foreach (var list in TempCommands.Values) {
                foreach (string item in list) {
                    if (item.StartsWith(lastCompleted)) {
                        counter++;
                        if (counter == repeatCount) {
                            Autocomplete(item);
                            return;
                        }
                    }
                }
            }

            if (lastCompleted.Length == 0) return;
            foreach (string item in autocompletions.Keys) {
                if (item.StartsWith(lastCompleted)) {
                    counter++;
                    if (counter == repeatCount) {
                        Autocomplete(item);
                        return;
                    }
                }
            }
        }
        static void Autocomplete(string s)
        {
            if (IsCursorBetweenQuotes()) {
                AutocompleteInQuotes(s);
                return;
            }
            int begin = RemoveStringUnderCursor(true);
            input.Insert(begin, s);
            pos = beginPos + begin + s.Length;

            repeatCount++;
        }
        static void AutocompleteInQuotes(string s)
        {
            int begin = RemoveStringUnderCursorInQuotes();
            input.Insert(begin, s);
            pos = beginPos + begin + s.Length;

            repeatCount++;
        }
        static bool IsCursorBetweenQuotes()
        {
            int ctr = 0;
            for (int i = 0; i < pos - beginPos - 1; ++i) {
                if (input[i] == '"') ctr++;
            }
            return ctr % 2 != 0;
        }
        static string GetCommand()
        {
            for (int i = 0; i < input.Length; ++i) {
                if (char.IsWhiteSpace(input[i])) return input.ToString().Substring(0, i);
            }
            return input.ToString();
        }
        static int RemoveStringUnderCursorInQuotes()
        {
            int begin = pos - 2 - beginPos;
            for (; begin >= 0; --begin) {
                if (input[begin] == '"') break;
            }
            int end = pos - beginPos;
            for (; end < input.Length; ++end) {
                if (input[end] == '"') break;
            }

            input.Remove(begin, end - begin);
            pos -= end - begin;
            return begin;
        }
        #endregion
        #region Arrow movement
        static void CursorMove(ConsoleKeyInfo key)
        {
            bool ctrl = key.Modifiers.HasFlag(ConsoleModifiers.Control);
            if (key.Key == ConsoleKey.LeftArrow) {
                if (ctrl) {
                    HandleCtrlLeftArrow();
                } else {
                    if (pos > beginPos) pos--;
                }
            } else {
                if (ctrl) {
                    HandleCtrlRightArrow();
                } else {
                    if (pos - beginPos < input.Length) pos++;
                }
            }
            prevPos = pos;
        }
        static void PrintUnderCursor(string s)
        {
            int left = Console.CursorLeft;
            Console.CursorTop++;
            Console.CursorLeft = 0;
            Console.Write(s);
            Console.CursorTop--;
            Console.CursorLeft = left;
        }
        static void HandleCtrlLeftArrow()
        {
            if (pos <= beginPos) return;
            int rbegin = pos - beginPos;
            while (rbegin > 0 && IsSpecialCharOrWhitespace(input[rbegin - 1])) rbegin--;
            while (rbegin > 0 && !IsSpecialCharOrWhitespace(input[rbegin - 1])) rbegin--;
            pos = rbegin + beginPos;
        }
        static void HandleCtrlRightArrow()
        {
            if (pos - beginPos >= input.Length) return;
            int begin = pos - beginPos;
            while (begin < input.Length && !IsSpecialCharOrWhitespace(input[begin])) begin++;
            while (begin < input.Length && IsSpecialCharOrWhitespace(input[begin])) begin++;
            pos = begin + beginPos;
        }
        #endregion
        #region Arrow Up/Down
        private static void SwapWithPrevInput(ConsoleKey key)
        {
            if (key == ConsoleKey.UpArrow) {
                if (prevInputPos <= 0) return;

                input.Clear();
                input = new StringBuilder(prevInput[--prevInputPos]);
            } else {
                if (prevInputPos >= prevInput.Count - 1) return;

                input.Clear();
                input = new StringBuilder(prevInput[++prevInputPos]);
            }
            pos = input.Length + beginPos;
        }
        #endregion
        #region Backspace/Delete
        static void HandleBackspace(bool ctrlPressed)
        {
            int relPos = pos - beginPos;
            if (input.Length <= 0 || relPos <= 0) return;
            if (ctrlPressed) {
                int rbegin = relPos - 1;
                while (rbegin > 0 && IsSpecialCharOrWhitespace(input[rbegin])) rbegin--;
                input.Remove(rbegin, relPos - rbegin);
                pos -= relPos - rbegin;
                int begin = RemoveStringUnderCursorLeft();
                pos = begin + beginPos;
            } else {
                input.Remove(relPos - 1, 1);
                pos--;
            }
        }
        static void HandleDelete(bool ctrPressed)
        {
            int relPos = pos - beginPos;
            if (input.Length <= 0 || relPos >= input.Length) return;
            
            if (ctrPressed) {
                int end = relPos;
                while (end < input.Length && IsSpecialCharOrWhitespace(input[end])) end++;
                if (end > 0) input.Remove(end - 1, end - relPos);
                _ = RemoveStringUnderCursorRight();
            } else {
                input.Remove(relPos, 1);
            }
        }
        //returns position to begin index, relative to beginPos
        static int RemoveStringUnderCursor(bool onlyWhitespace = false)
        {
            RemoveStringUnderCursorLeft(onlyWhitespace);
            return RemoveStringUnderCursorRight(onlyWhitespace);
        }
        static int RemoveStringUnderCursorLeft(bool onlyWhitespace = false)
        {
            int begin = pos - 1 - beginPos;
            for (; begin >= 0; --begin) {
                if (onlyWhitespace) { 
                    if (char.IsWhiteSpace(input[begin])) break; 
                } else { 
                    if (IsSpecialCharOrWhitespace(input[begin])) break; 
                }
            }
            int end = pos - beginPos;
            input.Remove(begin + 1, end - begin - 1);
            pos -= end - begin - 1;
            return begin + 1;
        }
        static int RemoveStringUnderCursorRight(bool onlyWhitespace = false)
        {
            int begin = pos - 1 - beginPos;
            int end = pos - beginPos;
            for (; end < input.Length; ++end) {
                if (onlyWhitespace) {
                    if (char.IsWhiteSpace(input[end])) break;
                } else {
                    if (IsSpecialCharOrWhitespace(input[end])) break;
                }
            }
            input.Remove(begin + 1, end - begin - 1);

            return begin + 1;
        }
        #endregion
        #region remaining functions
        private static void PrepareVariables()
        {
            input = new StringBuilder();
            beginPos = Console.CursorLeft;
            prevPos = beginPos;
            beginTop = Console.CursorTop;
            repeatCount = 0;
            lastCompleted = string.Empty;
            prevInput.Remove("");
            prevInputPos = prevInput.Count;
            prevInput.Add("");
        }
        private static string GetStringUnderCursor()
        {
            var sb = new StringBuilder();
            int end = pos - beginPos;
            for (; end < input.Length; ++end) {
                if (char.IsWhiteSpace(input[end])) break;
            }
            int begin = pos - 1 - beginPos;
            for (; begin >= 0; --begin) {
                if (char.IsWhiteSpace(input[begin])) break;
            }
            for (int i = begin + 1; i < end; ++i) {
                sb.Append(input[i]);
            }
            return sb.ToString();
        }
        private static void ClearCurrentInput()
        {
            int linesCount = (input.Length + beginPos) / Console.WindowWidth + 1;
            Console.CursorTop = beginTop + linesCount - 1;
            while (linesCount > 1) {
                Console.CursorLeft = 0;
                Console.Write(new string(' ', Console.WindowWidth));
                Console.CursorTop--;
                linesCount--;
            }

            Console.CursorLeft = beginPos;
            Console.Write(new string(' ', Console.WindowWidth - beginPos));
            Console.CursorLeft = beginPos;
            Console.CursorTop = beginTop;
        }
        private static bool IsKeyAChar(char key)
        {
            return 32 <= (byte)key && (byte)key <= 127;
        }
        private static bool IsSpecialCharOrWhitespace(char key)
        {
            return key != 45 && (char.IsWhiteSpace(key) || 33 <= (byte)key && (byte)key <= 47);
        }
        #endregion
        #region Input coloring
        private static void PrintColoredInput()
        {
            if (input.Length <= 0) return;

            var cs = new CoffeeString();
            int i = 0;
            var commandColor = input[0] == '.' ? ConsoleColor.Cyan : ConsoleColor.Yellow;
            for (; i < input.Length; ++i) {
                if (char.IsWhiteSpace(input[i])) break;
            }
            cs.Add(input.ToString(0, i), commandColor);

            for (; i < input.Length; i++) {
                if (!char.IsWhiteSpace(input[i])) {
                    i = TryColorArg(i, cs);
                } else {
                    cs.Add(input[i] + "");
                }
            }
            bool flag = CoffeeString.InstantPrint;
            InstantPrint = true;

            cs.Print();

            InstantPrint = flag;
        }
        private static int TryColorArg(int i, CoffeeString cs)
        {
            if (i == 0) return 0;
            if (i >= input.Length) return i;

            if (i < input.Length - 1 && input[i] == '-' && input[i + 1] != ' ') {
                int begin = i;
                for (; i < input.Length; ++i) {
                    if (char.IsWhiteSpace(input[i])) break;
                }
                cs.Add(input.ToString(begin, i - begin), ConsoleColor.DarkGray);
            } else {
                int begin = i;
                for (; i < input.Length; ++i) {
                    if (char.IsWhiteSpace(input[i])) break;
                }
                cs.Add(input.ToString(begin, i - begin), ConsoleColor.Gray);
            }
            return i - 1;
        }
        #endregion
    }
}