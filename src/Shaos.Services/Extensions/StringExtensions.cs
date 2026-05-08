/*
* MIT License
*
* Copyright (c) 2025 Derek Goslin https://github.com/DerekGn
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

namespace Shaos.Services.Extensions
{
    /// <summary>
    /// A set of string extension methods
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Create a directory for the specified value
        /// </summary>
        /// <param name="value">The directory to create</param>
        /// <returns><see langword="true"/> if the path was created else <see langword="false"/> if the directory already existed</returns>
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

        /// <summary>
        /// Deletes all files and subdirectories from a directory
        /// </summary>
        /// <param name="value">The path to the directory</param>
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

        /// <summary>
        /// Indicates if a string is null, empty or whitespace
        /// </summary>
        /// <param name="value">The string value to check</param>
        /// <returns><see langword="true"/> if the string is null, empty or whitespace</returns>
        public static bool IsEmptyOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Sanitize a string value
        /// </summary>
        /// <param name="value">The value to Sanitize</param>
        /// <returns>The sanitized string value</returns>
        public static string Sanitize(this string value)
        {
            return value.Replace(Environment.NewLine, string.Empty);
        }
    }
}