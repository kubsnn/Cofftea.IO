using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cofftea.IO
{
    public class Input : IDisposable
    {
        private List<int> data;
        private bool Error;
        public delegate void Handler(Command cmd);
        private Handler handler;
        public static string Prefix { get; set; } = ">";
        public static ConsoleColor PrefixColor { get; set; } = ConsoleColor.Gray;
        
        public Input(Handler handler)
        {
            this.handler = handler;
        }
        public Command GetCommand(string line)
        {
            Error = false;
            data = (from c in line
                    select (int)c
                    ).ToList();

            data.Append(-1);

            var keys = new List<string>();
            var values = new List<string>();
            var args = new List<string>();

            for (int i = 0; i < data.Count; ++i) {
                string key = ReadArg(ref i);
                SkipWhitespaces(ref i);
                if (i < data.Count - 1 && (char)data[i + 1] == '=') {
                    i += 2;
                    SkipWhitespaces(ref i);
                    i++;
                    string value = ReadArg(ref i);
                    SkipWhitespaces(ref i);
                    keys.Add(key);
                    values.Add(value);
                } else {
                    args.Add(key);
                }
            }

            ClearTemporaryVars();

            string _base = "INSERT";
            if (args.Count > 0) {
                _base = args[0];
                args.RemoveAt(0);
            }

            return new Command(_base, keys, values, args) { Error = this.Error };
        }
        string ReadArg(ref int i)
        {
            bool spaces = false;
            char end_char = (char)data[i];
            if (end_char == '"' || end_char == '\'' || end_char == '%') {
                spaces = true;
                i++;
            }

            var sb = new StringBuilder();
            for (; i < data.Count; ++i)
            {
                if (data[i] == -1) {
                    if (spaces) Error = true;
                    break;
                }
                char c = (char)data[i];
                if (spaces) {
                    if (c != end_char) sb.Append(c);
                    else break;
                } else {
                    if (c == '=') { i--; break; } 
                    if (c != ' ' && c != '\t') sb.Append(c);
                    else break;
                }
            }
            i++;
            if (end_char == '%') {
                return GetCommandResult(GetCommand(sb.ToString()));
            }
            return sb.ToString();
        }
        string GetCommandResult(Command cmd)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            var std = Console.Out;
            Console.SetOut(writer);
            handler(cmd);
            Console.SetOut(std);
            writer.Flush();

            var reader = new StreamReader(stream);
            reader.BaseStream.Position = 0;
            string output = reader.ReadToEnd().Replace("\r", "");
            return output.EndsWith('\n') ? output[0..(output.Length - 1)] : output;
        }
        void SkipWhitespaces(ref int i)
        {
            while (i < data.Count && (data[i] == -1 || char.IsWhiteSpace((char)data[i]))) i++;
            i--;
        }
        void ClearTemporaryVars()
        {
            data.Clear();
        }
        public void Dispose()
        {
            ClearTemporaryVars();
            GC.SuppressFinalize(this);
        }
    }
}
