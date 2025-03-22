
namespace Shaos.Services.UnitTests.Extensions
{
    internal static class StringExtensions
    {
        public static void CreateFolder(this string value) {
            
            if (value != null && !Directory.Exists(value))
            {
                Directory.CreateDirectory(value);
            }
        }
    }
}
