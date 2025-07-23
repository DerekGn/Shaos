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

using Shaos.Sdk;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Shaos.Repository.Models
{
    /// <summary>
    /// A <see cref="Package"/> for a <see cref="PlugIn"/>
    /// </summary>
    public class Package
    {
        /// <summary>
        /// The fully qualified path to the PlugIn assembly
        /// </summary>
        [DisplayFormat(NullDisplayText = "No Package Uploaded")]
        public string AssemblyFile { get; set; } = string.Empty;

        /// <summary>
        /// The version of the <see cref="Package"/>
        /// </summary>
        [DisplayFormat(NullDisplayText = "No Package Uploaded")]
        public string AssemblyVersion { get; set; } = string.Empty;

        /// <summary>
        /// The file name of the <see cref="Package"/>
        /// </summary>
        [DisplayFormat(NullDisplayText = "No Package Uploaded")]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the <see cref="Package"/> <see cref="IPlugIn"/> has a configuration constructor argument
        /// </summary>
        [DisplayFormat(NullDisplayText = "No Package Uploaded")]
        public bool HasConfiguration { get; set; }

        /// <summary>
        /// Indicates if the <see cref="Package"/> <see cref="IPlugIn"/> has a logger constructor argument
        /// </summary>
        [DisplayFormat(NullDisplayText = "No Package Uploaded")]
        public bool HasLogger { get; set; }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append($"{nameof(AssemblyFile)}: {AssemblyFile} ");
            stringBuilder.Append($"{nameof(AssemblyVersion)}: {AssemblyVersion}");
            stringBuilder.Append($"{nameof(FileName)}: {FileName}");
            stringBuilder.Append($"{nameof(HasConfiguration)}: {HasConfiguration}");
            stringBuilder.Append($"{nameof(HasLogger)}: {HasLogger}");

            return stringBuilder.ToString();
        }
    }
}