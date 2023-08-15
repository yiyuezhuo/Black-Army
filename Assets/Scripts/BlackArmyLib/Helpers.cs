using System;

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
    }
}