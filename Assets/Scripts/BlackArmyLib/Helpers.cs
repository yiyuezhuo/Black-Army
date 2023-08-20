using System;
using System.Collections.Generic;

namespace YYZ
{
    public static class Helpers
    {
        static Random rng = new();
        public static float NextFloat() => (float)rng.NextDouble();
        public static int RandomRound(float x)
        {
            var r = NextFloat() < (x % 1) ? 1 : 0;
            return (int)MathF.Floor(x) + r;
        }

        public static T MaxBy<T>(IEnumerable<T> collection, Func<T, int> f)
        {
            var max = int.MinValue;
            var maxEl = default(T);
            foreach (var el in collection)
            {
                var x = f(el);
                if (x > max)
                {
                    max = x;
                    maxEl = el;
                }
            }
            return maxEl;
        }
    }
}