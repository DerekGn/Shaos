namespace Shaos.Extensions
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string value)
        {
            return char.ToLowerInvariant(value[0]) + value[1..];
        }

        public static string SanitizeFileName(this string value)
        {
#warning TODO
            return value;
        }
    }
}
