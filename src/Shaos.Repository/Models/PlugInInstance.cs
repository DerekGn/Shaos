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

using Microsoft.EntityFrameworkCore;
using Shaos.Repository.Models.Devices;
using Shaos.Sdk;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Shaos.Repository.Models
{
    /// <summary>
    /// Represents a configured instance of a <see cref="PlugIn"/>
    /// </summary>
    [Index(nameof(Name), IsUnique = true)]
    public class PlugInInstance : PlugInChildBase
    {
        /// <summary>
        /// The <see cref="IPlugIn"/> serialised configuration
        /// </summary>
        public string? Configuration { get; set; }

        /// <summary>
        /// The description of the <see cref="PlugInInstance"/>
        /// </summary>
        [MaxLength(100)]
        [Required]
        public required string Description { get; set; }

        /// <summary>
        /// Indicates if the <see cref="PlugInInstance"/> is enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// The name of this <see cref="PlugInInstance"/>
        /// </summary>
        [MaxLength(40)]
        [Required]
        public required string Name { get; set; }

        /// <summary>
        /// The set of <see cref="Device"/> instance created by this <see cref="PlugInInstance"/>
        /// </summary>
        public List<Device> Devices { get; } = [];

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append($"{nameof(Description)}: {Description ?? string.Empty} ");
            stringBuilder.Append($"{nameof(Enabled)}: {Enabled}");
            stringBuilder.Append($"{nameof(Name)}: {Name} ");

            return stringBuilder.ToString();
        }
    }
}