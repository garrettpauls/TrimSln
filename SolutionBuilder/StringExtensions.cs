using System;

namespace SolutionBuilder
{
    public static class StringExtensions
    {
        public static bool Contains(this string haystack, string needle, StringComparison comparisonType)
            => haystack.IndexOf(needle, comparisonType) >= 0;
    }
}
