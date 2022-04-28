using System;

namespace Cofftea.IO
{
    public static class Messages
    {
        readonly static CoffeeString UnknownCmd;
        public static CoffeeString UnknownCommand(string cmd)
        {
            var s = new CoffeeString();
            s.Add("'" + cmd + "'", ConsoleColor.Red, 10, true);
            s += UnknownCmd;
            return s;
        }
        public static CoffeeString WrongArgNumber(int given, int required)
        {
            var s = new CoffeeString();
            s.Add("Number of arguments is not matching ", ConsoleColor.Red, 50, true);
            s.AddLine("(" + given + ", required: " + required + ")", ConsoleColor.Red, 50);
            return s;
        }
        static Messages()
        {
            var s = new CoffeeString();
            s.AddLine(" is not recognized as an internal or external program", ConsoleColor.Red, 120);
            UnknownCmd = s;
        }
    }
}
