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

using NuGet.Protocol.Core.Types;

namespace Shaos.Services.Package
{
    public interface INuGetPackageSourceService
    {
        /// <summary>
        /// Resolves a <see cref="NuGetSpecification"/> to a NuGet and its dependent NuGet packages
        /// </summary>
        /// <param name="nuGetSpecification">The <see cref="NuGetSpecification"/> to resolve the NuGet package reference</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel this operation</param>
        /// <returns>The <see cref="NuGetSpecificationResolveResult"/> for the resolved package</returns>
        Task<NuGetSpecificationResolveResult> ResolveNuGetSpecificationAsync(
            NuGetSpecification nuGetSpecification,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Download a set of package dependencies
        /// </summary>
        /// <param name="packageDependency">The <see cref="SourcePackageDependencyInfo"/> to download</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel this operation</param>
        /// <returns>The <see cref="NuGetPackageDownloadResult"/></returns>
        Task<NuGetPackageDownloadResult> DownloadPackageDependenciesAsync(
            SourcePackageDependencyInfo packageDependency,
            CancellationToken cancellationToken = default);
    }
}
