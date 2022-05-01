using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cofftea.IO
{
    public class Command : ICloneable
    {
        public string Base { get; set; }
        public List<string> Keys { get; private set; }
        public List<string> Values { get; private set; }
        public List<string> Args { get; private set; }

        public bool Error { get; set; } = false;
        public string RawLine { get; private set; }

        public Command()
        {
            Base = string.Empty;
            Keys = new List<string>();
            Values = new List<string>();
        }

        public Command(string cmd, IList<string> keys, IList<string> values, IList<string> args)
        {
            Base = cmd;
            Keys = (List<string>)keys;
            Values = (List<string>)values;
            Args = (List<string>)args;
            UpdateStringArgs();
        }

        public Command(Command cmd)
        {
            Base = (string)cmd.Base.Clone();
            Keys = (List<string>)cmd.Keys.Clone();
            Values = (List<string>)cmd.Values.Clone();
        }
        
        public void AddArg(string s)
        {   
            Args.Add(s);
            UpdateStringArgs();
        }
        public void ArgsToUpper()
        {
            Args = (from arg in Args
                    select arg.ToUpper()
                    ).ToList();
        }
        public object Clone()
        {
            var cmd = new Command();
            cmd.Base = (string)Base.Clone();
            cmd.Keys = (List<string>)Keys.Clone();
            return cmd;
        }

        //private members
        private void UpdateStringArgs()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < Keys.Count; ++i) {
                bool flag = Keys[i].Contains(' ');
                if (flag) sb.Append('"');
                sb.Append(Keys[i]);
                if (flag) sb.Append('"');
                sb.Append("=");
                bool flag2 = Values[i].Contains(' ');
                if (flag2) sb.Append('"'); ;
                sb.Append(Values[i]);
                if (flag2) sb.Append('"');
                sb.Append(' ');
            }
            RawLine = sb.ToString();
        }
    }
}
