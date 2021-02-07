using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibrary
{
    public static class NumHelper
    {
        public static int GenerateNumberFromSeed(int seed, int upperLimit) {
            int result = seed % upperLimit;
            return result;
        }

        public static int RoundDownToNearest10(this int source) {
            int result = 10 * (source / 10);
            return result;
        }
    }
}
