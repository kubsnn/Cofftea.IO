using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofftea.IO
{
    internal static class FastMath
    {
        private static int[] Data { get; }
        static FastMath()
        {
            Data = new int[] { 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000 };
        }
        public static int Log10(int n)
        {
            for (int i = 0; i < Data.Length; ++i) {
                if (Data[i] > n) return i;
            }
            return Data[Data.Length - 1];
        }

    }
}
