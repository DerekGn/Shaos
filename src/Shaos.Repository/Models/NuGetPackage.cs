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
    /// A <see cref="NuGetPackage"/> for a <see cref="PlugIn"/>
    /// </summary>
    public class NuGetPackage : PlugInChildBase
    {
        /// <summary>
        /// The file name of the <see cref="NuGetPackage"/>
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
<<<<<<< HEAD
        /// The version of the  <see cref="NuGetPackage"/>
        /// </summary>
        public string Version { get; set; } = string.Empty;
=======
        /// The file path of the <see cref="NuGetPackage"/>
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// The version of the <see cref="NuGetPackage"/>
        /// </summary>
        public string Version {get; set;} = string.Empty;
>>>>>>> d0cd741a7f89a978c2d156ee706b7fdf88d35445
    }
}