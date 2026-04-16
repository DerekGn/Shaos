namespace Shaos.Extensions
{
    public static class BoolExtensions
    {
        public static string ToChecked(this bool value)
        {
            return value ? "checked" : string.Empty;
        }
    }
}
