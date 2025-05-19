namespace Shaos.Services.Extensions
{
    internal static class StringExtensions
    {
        public static bool CreateDirectory(this string value)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(value);

            var result = Directory.Exists(value);

            if (!result)
            {
                Directory.CreateDirectory(value);
            }

            return result;
        }

        public static void EmptyDirectory(this string value)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(value);

            if (Directory.Exists(value))
            {

                DirectoryInfo directoryInfo = new DirectoryInfo(value);

                foreach (FileInfo file in directoryInfo.EnumerateFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in directoryInfo.EnumerateDirectories())
                {
                    dir.Delete(true);
                }
            }
        }
    }
}