using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cofftea.IO
{
    public partial class CoffeeString : IDisposable, IEquatable<CoffeeString> //Output; Input - CoffeeString.Input.cs
    {
        struct Item
        {
            public string Text;
            public ConsoleColor Color;
            public int Delay;
            public bool Prefix;
            public ConsoleColor PrefixColor;
        }
        public static ConsoleColor DefaultColor { get; set; } = ConsoleColor.White;
        public static ConsoleColor DefaultPrefixColor { get; set; } = ConsoleColor.Blue;
        public static string Prefix { get; set; }
        public static bool InstantPrint { get; set; } = false;
        public static readonly CoffeeString Empty = new CoffeeString();

        private List<Item> Data;

        static object printLock = new object();

        public CoffeeString()
        {
            this.Data = new List<Item>();

        }
        public CoffeeString(CoffeeString cs)
        {
            this.Data = new List<Item>(cs.Data);
        }

        public void Print()
        {
            lock (printLock) {
                if (InstantPrint) {
                    PrintWithoutDelays();
                } else {
                    PrintWithDelays();
                }

                //Console.Write("\b");
                //if (Data.Count > 0 && !Data[Data.Count - 1].Text.EndsWith(Environment.NewLine)) {
                //    Console.WriteLine();
                //}
            }
        }
        void PrintWithoutDelays()
        {
            var color = Console.ForegroundColor;
            foreach (var item in this.Data) {
                if (item.Prefix) {
                    Console.ForegroundColor = item.PrefixColor;
                    Console.Write(Prefix);
                }
                Console.ForegroundColor = item.Color;
                Console.Write(item.Text);
            }
            Console.ForegroundColor = color;
        }
        void PrintWithDelays()
        {
            var color = Console.ForegroundColor;
            foreach (var item in this.Data) {
                if (item.Prefix) {
                    Console.ForegroundColor = item.PrefixColor;
                    Console.Write(Prefix);
                }
                Console.ForegroundColor = item.Color;
                if (item.Delay == 0) {
                    Console.Write(item.Text);
                } else {
                    int delay = item.Delay / (item.Text.Length + 1) + 1;
                    Thread.Sleep(delay);
                    foreach (char _char in item.Text) {
                        Console.Write(_char);
                        Thread.Sleep(delay);
                    }
                }
            }
            Console.ForegroundColor = color;
        }
        public void PrintEnumerated()
        {
            PrintEnumerated(0);
        }
        public void PrintEnumerated(int preSpacesCount, bool defaultColorOverride = false, bool hideLineNumber = false)
        {
            lock (printLock) {
                if (InstantPrint) {
                    PrintEnumeratedWithoutDelays(preSpacesCount, defaultColorOverride, hideLineNumber);
                } else {
                    PrintEnumeratedWithDelays(preSpacesCount, defaultColorOverride, hideLineNumber);
                }

                Console.Write("\b");
                if (Data.Count > 0 && !Data[Data.Count - 1].Text.EndsWith(Environment.NewLine)) {
                    Console.WriteLine();
                }
            }
        }
        void PrintEnumeratedWithoutDelays(int spacesCount, bool defaultColor, bool hideLineNumber)
        {
            var color = Console.ForegroundColor;
            int maxlen = FastMath.Log10(Count);
            int counter = 1;
            bool begin = true;
            foreach (var item in this.Data) {
                if (item.Prefix) {
                    Console.ForegroundColor = item.PrefixColor;
                    Console.Write(Prefix);
                }
                if (begin) {
                    var sb = new StringBuilder();
                    sb.Append(' ', spacesCount + maxlen - FastMath.Log10(counter));
                    Console.ForegroundColor = ConsoleColor.Red;
                    if (!hideLineNumber) {
                        Console.Write(sb.ToString() + counter + " ");
                    } else {
                        Console.Write(sb.ToString());
                    }
                    
                    begin = false;
                }
                if (defaultColor) Console.ForegroundColor = counter % 2 == 0 ? ConsoleColor.Gray : ConsoleColor.White;
                else Console.ForegroundColor = item.Color;
                
                Console.Write(item.Text);

                if (item.Text.EndsWith(Environment.NewLine)) {
                    counter++;
                    begin = true;
                }
            }
            Console.ForegroundColor = color;
        }
        void PrintEnumeratedWithDelays(int spacesCount, bool defaultColor, bool hideLineNumber)
        {
            var color = Console.ForegroundColor;
            foreach (var item in this.Data) {
                if (item.Prefix) {
                    Console.ForegroundColor = item.PrefixColor;
                    Console.Write(Prefix);
                }
                Console.ForegroundColor = item.Color;
                if (item.Delay == 0) {
                    Console.Write(item.Text);
                } else {
                    int delay = item.Delay / (item.Text.Length + 1) + 1;
                    Thread.Sleep(delay);
                    foreach (char _char in item.Text) {
                        Console.Write(_char);
                        Thread.Sleep(delay);
                    }
                }
            }
            Console.ForegroundColor = color;
        }
        #region static Console Write
        public static void WriteLine()
        {
            Console.WriteLine();
        }
        public static void WriteLine(string text)
        {
            WriteLine(text, DefaultColor, 0, false, DefaultPrefixColor);
        }
        public static void WriteLine(string text, ConsoleColor color)
        {
            WriteLine(text, color, 0, false, DefaultPrefixColor);
        }
        public static void WriteLine(string text, ConsoleColor color, int delay)
        {
            WriteLine(text, color, delay, false, DefaultPrefixColor);
        }
        public static void WriteLine(string text, ConsoleColor color, int delay, bool prefix)
        {
            WriteLine(text, color, delay, prefix, DefaultPrefixColor);
        }
        public static void WriteLine(string text, ConsoleColor color, int delay, bool prefix, ConsoleColor prefixColor)
        {
            Write(text + Environment.NewLine, color, delay, prefix, prefixColor);
        }
        public static void Write(string text)
        {
            Write(text, DefaultColor, 0, false, DefaultPrefixColor);
        }
        public static void Write(string text, ConsoleColor color)
        {
            Write(text, color, 0, false, DefaultPrefixColor);
        }
        public static void Write(string text, ConsoleColor color, int delay)
        {
            Write(text, color, delay, false, DefaultPrefixColor);
        }
        public static void Write(string text, ConsoleColor color, int delay, bool prefix)
        {
            Write(text, color, delay, prefix, DefaultPrefixColor);
        }
        public static void Write(string text, ConsoleColor color, int delay, bool prefix, ConsoleColor prefixColor)
        {
            lock (printLock) {
                var prev_color = Console.ForegroundColor;
                if (prefix) {
                    Console.ForegroundColor = prefixColor;
                    Console.Write(Prefix);
                }
                Console.ForegroundColor = color;
                if (delay > 0) {
                    for (int i = 0; i < text.Length; ++i) {
                        Console.Write(text[i]);
                        System.Threading.Thread.Sleep(delay);
                    }
                } else {
                    Console.Write(text);
                }
                Console.ForegroundColor = prev_color;
            }
        }
        #endregion
        #region Add functions
        public void AddDelay(int delay)
        {
            Add("", DefaultColor, delay, false, DefaultPrefixColor);
        }
        public void AddPrefix()
        {
            Add("", DefaultColor, 0, true, DefaultPrefixColor);
        }
        public void AddPrefix(ConsoleColor prefixColor)
        {
            Add("", DefaultColor, 0, true, prefixColor);
        }
        public void AddSpace()
        {
            Add(" ", DefaultColor, 0, false, DefaultPrefixColor);
        }
        public void AddLine()
        {
            AddLine("", DefaultColor, 0, false, DefaultPrefixColor);
        }
        public void AddLine(string text)
        {
            AddLine(text, DefaultColor, 0, false, DefaultPrefixColor);
        }
        public void AddLine(string text, ConsoleColor color)
        {
            AddLine(text, color, 0, false, DefaultPrefixColor);
        }
        public void AddLine(string text, ConsoleColor color, int delay)
        {
            AddLine(text, color, delay, false, DefaultPrefixColor);
        }
        public void AddLine(string text, ConsoleColor color, int delay, bool prefix)
        {
            AddLine(text, color, delay, prefix, DefaultPrefixColor);
        }
        public void AddLine(string text, ConsoleColor color, int delay, bool prefix, ConsoleColor prefixColor)
        {
            Add(text + Environment.NewLine, color, delay, prefix, prefixColor);
        }
        public void Add(string text)
        {
            Add(text, DefaultColor, 0, false, DefaultPrefixColor);
        }
        public void Add(string text, ConsoleColor color)
        {
            Add(text, color, 0, false, DefaultPrefixColor);
        }
        public void Add(string text, ConsoleColor color, int delay)
        {
            Add(text, color, delay, false, DefaultPrefixColor);
        }
        public void Add(string text, ConsoleColor color, int delay, bool prefix)
        {
            Add(text, color, delay, prefix, DefaultPrefixColor);
        }
        public void Add(string text, ConsoleColor color, int delay, bool prefix, ConsoleColor prefixColor)
        {
            Data.Add(new Item { Text = text, Color = color, Delay = delay, Prefix = prefix, PrefixColor = prefixColor });
        }
        public void Add(CoffeeString cstring)
        {
            this.Data.AddRange(cstring.Data);
        }

        #endregion
        public bool RemoveAt(int index)
        {
            if (!(index < this.Data.Count)) return false;
            this.Data.RemoveAt(index);
            return true;
        }
        public static string RedirectOutputToString(Action action)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            var std = Console.Out;
            Console.SetOut(writer);
            action();
            Console.SetOut(std);
            writer.Flush();

            using var reader = new StreamReader(stream);
            reader.BaseStream.Position = 0;
            return reader.ReadToEnd();
        }

        public bool IsEmpty => Data.Count <= 0;
        public int Count => Data.Count;
        public override string ToString()
        {
            return string.Join(" ", from item in Data select item.Text);
        }
        public string[] ToArray()
        {
            return (from item in Data select item.Text).ToArray();
        }
        public static bool operator ==(CoffeeString left, CoffeeString right)
        {
            if (left is null || right is null) return false;
            return left.Equals(right);
        }
        public static bool operator !=(CoffeeString left, CoffeeString right)
        {
            if (left is null || right is null) return true;
            return !left.Equals(right);
        }
        public static CoffeeString operator +(CoffeeString left, CoffeeString right) {
            var res = new CoffeeString();
            res.Data.AddRange(left.Data);
            res.Data.AddRange(right.Data);
            return res;
        }
        public override bool Equals(object obj)
        {
            var other = obj as CoffeeString;
            if (other == null) return false;
            return Equals(other);
        }
        public bool Equals(CoffeeString other)
        {
            if (other == null) return false;
            if (Data.Count != other.Data.Count) return false;
            for (int i = 0; i < Data.Count; ++i) {
                if (Data[i].Text != other.Data[i].Text) return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            return string.Join("", from item in Data select item.Text).GetHashCode();
        }

        bool _disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed) {
                if (disposing) {
                    Data.Clear();
                    Data = null;
                }
                _disposed = true;
            }
        }
    }
}
