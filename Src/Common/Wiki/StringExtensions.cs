namespace WikiExportParser.Wiki
{
    using System;

    internal static class StringExtensions
    {
        public static bool StartsWithOrdinal(this string source, string value)
        {
            return source.StartsWith(value, StringComparison.Ordinal);
        }

        public static bool EqualsOrdinal(this string @this, string value)
        {
            return @this.Equals(value, StringComparison.Ordinal);
        }

        public static string EmptyAsNull(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return value;
        }
    }
}
