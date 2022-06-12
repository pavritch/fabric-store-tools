using System;

namespace Utilities
{
    public static class RandomInteger
    {
        private static readonly Random _rand = new Random();
        public static int Between(int min, int max) { return _rand.Next(min, max + 1); }
        public static bool Bool() { return Convert.ToBoolean(_rand.Next(0, 2)); }
    }
}