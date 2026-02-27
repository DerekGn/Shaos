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
    /// A <see cref="PlugInInformation"/> for a <see cref="PlugIn"/>
    /// </summary>
    public class PlugInInformation : BaseEntity
    {
        /// <summary>
        /// The fully qualified path to the PlugIn assembly
        /// </summary>
        [MaxLength(40)]
        [Required]
        public required string AssemblyFileName { get; set; }

        /// <summary>
        /// The version of the <see cref="PlugInInformation"/>
        /// </summary>
        [MaxLength(10)]
        [Required]
        public required string AssemblyVersion { get; set; }

        /// <summary>
        /// The folder where the PlugIn was extracted too.
        /// </summary>
        [MaxLength(32)]
        [Required]
        public required string Directory { get; set; }

        /// <summary>
        /// Indicates if the <see cref="PlugInInformation"/> <see cref="IPlugIn"/> has a configuration constructor argument
        /// </summary>
        [Required]
        public required bool HasConfiguration { get; set; }

        /// <summary>
        /// Indicates if the <see cref="PlugInInformation"/> <see cref="IPlugIn"/> has a logger constructor argument
        /// </summary>
        [Required]
        public required bool HasLogger { get; set; }

        /// <summary>
        /// The file name of the PlugIn package
        /// </summary>
        [MaxLength(40)]
        [Required]
        public required string PackageFileName { get; set; }

        /// <summary>
        /// The parent <see cref="IPlugIn"/>
        /// </summary>
        public PlugIn PlugIn { get; set; } = null!;

        /// <summary>
        /// The parent <see cref="PlugIn"/> identifier
        /// </summary>
        public int PlugInId { get; set; }

        /// <summary>
        /// The type name of the type that implements the <see cref="IPlugIn"/> interface
        /// </summary>
        [MaxLength(100)]
        [Required]
        public required string TypeName { get; set; }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append($"{nameof(AssemblyFileName)}: {AssemblyFileName} ");
            stringBuilder.Append($"{nameof(AssemblyVersion)}: {AssemblyVersion} ");
            stringBuilder.Append($"{nameof(Directory)}: {Directory} ");
            stringBuilder.Append($"{nameof(HasConfiguration)}: {HasConfiguration} ");
            stringBuilder.Append($"{nameof(HasLogger)}: {HasLogger}");
            stringBuilder.Append($"{nameof(PackageFileName)}: {PackageFileName}");
            stringBuilder.Append($"{nameof(TypeName)}: {TypeName}");

            return stringBuilder.ToString();
        }
    }
}