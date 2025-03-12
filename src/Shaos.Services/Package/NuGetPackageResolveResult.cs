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

using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;

namespace Shaos.Services.Package
{
    /// <summary>
    /// The result of a package resolution request
    /// </summary>
    public record NuGetPackageResolveResult
    {
        public IEnumerable<SourcePackageDependencyInfo>? Dependencies { get; internal set; } = new List<SourcePackageDependencyInfo>();

        /// <summary>
        /// The <see cref="PackageIdentity"/> of the resolved package
        /// </summary>
        public PackageIdentity? Identity { get; internal set; }

        /// <summary>
        /// The <see cref="NuGetPackageResolveRequest"/>
        /// </summary>
        public required NuGetPackageResolveRequest Request { get; init; }

        /// <summary>
        /// The <see cref="ResolveStatus"/>
        /// </summary>
        public ResolveStatus Status { get; set; }
    }
}