using System;

namespace Code.Generation.Roslyn
{
    internal static class DateTimeMath
    {
        public static DateTime? Min(this DateTime? x, DateTime? y)
            => x == null && y != null
                ? y
                : x != null && y == null
                    ? x
                    : x > y
                        ? y
                        : x;

        public static DateTime? Max(this DateTime? x, DateTime? y) => Min(y, x);
    }
}
