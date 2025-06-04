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

using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Shaos.Services.Runtime.Host
{
    public class InstanceConfiguration
    {
        public InstanceConfiguration(bool requiresConfiguration,
                                     string? configuration = default)
        {
            RequiresConfiguration = requiresConfiguration;
            Configuration = configuration;
        }

        /// <summary>
        /// The PlugIn instance configuration
        /// </summary>
        /// <remarks>
        /// The PlugIn configuration JSON string
        /// </remarks>
        public string? Configuration { get; }

        /// <summary>
        /// Indicates if this instance has configuration settings
        /// </summary>
        public bool RequiresConfiguration { get; }

        /// <summary>
        /// Indicates if this instance is configured
        /// </summary>
        public bool IsConfigured => !string.IsNullOrWhiteSpace(Configuration);

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{nameof(Configuration)}: {(Configuration ?? "Empty")} ");
            stringBuilder.AppendLine($"{nameof(IsConfigured)}: {IsConfigured}");
            stringBuilder.AppendLine($"{nameof(RequiresConfiguration)}: {RequiresConfiguration} ");

            return stringBuilder.ToString();
        }
    }
}