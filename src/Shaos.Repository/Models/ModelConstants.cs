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

namespace Shaos.Repository.Models
{
    /// <summary>
    /// Constants for model field sizes
    /// </summary>
    public static class ModelConstants
    {
        /// <summary>
        /// The maximum configuration type name field length
        /// </summary>
        public const int MaxConfigTypeLength = 100;

        /// <summary>
        /// The maximum description length
        /// </summary>
        public const int MaxDescriptionLength = 100;

        /// <summary>
        /// The maximum assembly file name length
        /// </summary>
        public const int MaxAssemblyFileNameLength = 40;

        /// <summary>
        /// The maximum file name field length
        /// </summary>
        public const int MaxFileNameLength = 40;

        /// <summary>
        /// The maximum assembly version field length
        /// </summary>
        public const int MaxAssemblyVersionLength = 10;

        /// <summary>
        /// The maximum directory field length
        /// </summary>
        public const int MaxDirectoryLength = 32;
    }
}