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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Shaos.Services.Package;
using Shaos.Services.Processing;
using Shaos.Services.Shared.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests.Processing
{
    public class NuGetProcessingServiceTests : BaseTests
    {
        private readonly Mock<INuGetPackageSourceService> _mockNuGetPackageSource;
        private readonly NuGetProcessingService _nuGetProcessingService;

        public NuGetProcessingServiceTests(ITestOutputHelper output) : base(output)
        {
            _mockNuGetPackageSource = new Mock<INuGetPackageSourceService>();

            _nuGetProcessingService = new NuGetProcessingService(
                Factory!.CreateLogger<NuGetProcessingService>(),
                _mockNuGetPackageSource.Object);
        }

        [Fact]
        public async Task TestDownloadNuGetNotDownloadedAsync()
        {
            var specification = new NuGetSpecification()
            {
                Id = "PACKAGE",
                PreRelease = false,
                Version = "1.0.0",
            };

            var version = new NuGetVersion(specification.Version);

            var resoveResult = new NuGetSpecificationResolveResult(true);

            var packageDependancies = new List<PackageDependency>()
            {
                new PackageDependency("Dependency")
            };

            resoveResult.Dependencies.Add(
                new SourcePackageDependencyInfo(
                    specification.Id,
                    version,
                    packageDependancies,
                    true,
                    new SourceRepository(new PackageSource("source"),
                    new List<INuGetResourceProvider>())));

            resoveResult.Identity = new PackageIdentity(
                specification.Id,
                version);

            _mockNuGetPackageSource.Setup(_ => _.ResolveNuGetSpecificationAsync(
                It.IsAny<NuGetSpecification>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(resoveResult);

            _mockNuGetPackageSource.Setup(_ => _.DownloadPackageDependenciesAsync(
                It.IsAny<SourcePackageDependencyInfo>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PackageDownload(new PackageSpecification("id", "version"))
                {
                    Status = DownloadStatus.NotFound
                });

            var result = await _nuGetProcessingService.DownloadNuGetAsync(specification);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotEmpty(result.PackageDownloads);
            Assert.Equal(DownloadStatus.NotFound, result.PackageDownloads.First().Status);
        }

        [Fact]
        public async Task TestDownloadNuGetNotResolvedAsync()
        {
            var specification = new NuGetSpecification()
            {
                Id = "PACKAGE",
                PreRelease = false,
                Version = "1.0.0",
            };

            var resoveResult = new NuGetSpecificationResolveResult(false);

            _mockNuGetPackageSource.Setup(_ => _.ResolveNuGetSpecificationAsync(
                It.IsAny<NuGetSpecification>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(resoveResult);

            var result = await _nuGetProcessingService.DownloadNuGetAsync(specification);

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task TestDownloadNuGetSuccessAsync()
        {
            var specification = new NuGetSpecification()
            {
                Id = "PACKAGE",
                PreRelease = false,
                Version = "1.0.0",
            };

            var version = new NuGetVersion(specification.Version);

            var resoveResult = new NuGetSpecificationResolveResult(true);

            var packageDependancies = new List<PackageDependency>() 
            {
                new PackageDependency("Dependency")
            };

            resoveResult.Dependencies.Add(
                new SourcePackageDependencyInfo(
                    specification.Id,
                    version,
                    packageDependancies,
                    true,
                    new SourceRepository(new PackageSource("source"),
                    new List<INuGetResourceProvider>())));

            resoveResult.Identity = new PackageIdentity(
                specification.Id,
                version);

            _mockNuGetPackageSource.Setup(_ => _.ResolveNuGetSpecificationAsync(
                It.IsAny<NuGetSpecification>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(resoveResult);

            _mockNuGetPackageSource.Setup(_ => _.DownloadPackageDependenciesAsync(
                It.IsAny<SourcePackageDependencyInfo>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PackageDownload(new PackageSpecification("id", "version"))
                {
                    Status = DownloadStatus.Success
                });

            var result = await _nuGetProcessingService.DownloadNuGetAsync(specification);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotEmpty(result.PackageDownloads);
        }
    }
}