namespace Shaos.Services.Extensions
{
    public static class StringExtensions
    {
        public static string ComputeHash(this string toHash)
        {
            // using var hash = System.Security.Cryptography.SHA256.Create();

            // using var stream = new MemoryStream()
            // {
            //     hash.ComputeHashAsync()
            // 	using (var stream = System.IO.File.OpenRead(filename))
            // 	{
            // 		var hash = hasher.ComputeHash(stream);
            // 		return BitConverter.ToString(hash).Replace("-", "");
            // 	}
            // }

            return string.Empty;
        }
    }
}