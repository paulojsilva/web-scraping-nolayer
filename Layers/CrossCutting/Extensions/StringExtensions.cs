namespace System
{
    public static class StringExtensions
    {
        public static bool ContainsInvariant(this string value, string search)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return value.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }
    }
}
