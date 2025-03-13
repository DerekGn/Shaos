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
using Microsoft.Extensions.Options;
using Shaos.Services.Options;
using Shaos.Services.Package;
using Shaos.Services.Shared.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests.Package
{
    public class NuGetPackageServiceTests : BaseTests
    {
        private readonly NuGetPackageSourceService _nuGetPackageService;

        public NuGetPackageServiceTests(ITestOutputHelper output) : base(output)
        {
            var factory = ServiceProvider.GetService<ILoggerFactory>();
            var optionsInstance = new NuGetPackageServiceOptions()
            {
                PackageFolder = Path.Combine(Environment.CurrentDirectory, "Packages"),
                PackageSources = new List<Uri>() { new Uri("https://api.nuget.org/v3/index.json") }
            };

            IOptions<NuGetPackageServiceOptions> options = Microsoft.Extensions.Options.Options.Create(optionsInstance);

            _nuGetPackageService = new NuGetPackageSourceService(
                factory!.CreateLogger<NuGetPackageSourceService>(),
                options);
        }

        [Fact]
        public async Task TestDownloadPackageDependenciesAsync()
        {
            var nuGetSpecification = new NuGetSpecification()
            {
                Package = "MCP2221IO",
                Version = "4.0.1"
            };

            var resolveResult = await _nuGetPackageService.ResolveNuGetSpecificationAsync(nuGetSpecification);

            AssertResolveResult(resolveResult, 5);

            foreach (var dependencyInfo in resolveResult.Dependencies!)
            {
                var downloadResult = await _nuGetPackageService.DownloadPackageDependenciesAsync(dependencyInfo);

                Assert.NotNull(downloadResult);
                Assert.Equal(DownloadStatus.Success, downloadResult.Status);
                Assert.NotNull(downloadResult.ExtractedFiles);
                Assert.NotEmpty(downloadResult.ExtractedFiles);
            }
        }

        [Fact]
        public async Task TestResolvePackageFoundAsync()
        {
            var nuGetSpecification = new NuGetSpecification()
            {
                Package = "HexIO",
                Version = "5.0.1"
            };

            var result = await _nuGetPackageService.ResolveNuGetSpecificationAsync(nuGetSpecification);

            Assert.NotNull(result);
            Assert.NotNull(result.Identity);
            Assert.Equal(ResolveStatus.Success, result.Status);
            Assert.NotNull(result.Dependencies);
            Assert.Single(result.Dependencies);
        }

        [Fact]
        public async Task TestResolvePackageFoundWithDependenciesAsync()
        {
            var nuGetSpecification = new NuGetSpecification()
            {
                Package = "MCP2221IO",
                Version = "4.0.1"
            };

            var result = await _nuGetPackageService.ResolveNuGetSpecificationAsync(nuGetSpecification);

            AssertResolveResult(result, 5);
        }

        [Fact]
        public async Task TestResolvePackageNotFoundAsync()
        {
            var nuGetSpecification = new NuGetSpecification()
            {
                Package = "287391",
                Version = "9.9.9"
            };

            var result = await _nuGetPackageService.ResolveNuGetSpecificationAsync(nuGetSpecification);

            Assert.NotNull(result);
            Assert.Null(result.Identity);
            Assert.Equal(ResolveStatus.NotFound, result.Status);
            Assert.NotNull(result.Dependencies);
            Assert.Empty(result.Dependencies);
        }

        private static void AssertResolveResult(
            NuGetSpecificationResolveResult result,
            int dedpendancyCount)
        {
            Assert.NotNull(result);
            Assert.NotNull(result.Identity);
            Assert.Equal(ResolveStatus.Success, result.Status);
            Assert.NotNull(result.Dependencies);
            Assert.Equal(dedpendancyCount, result.Dependencies.Count());
        }
    }
}