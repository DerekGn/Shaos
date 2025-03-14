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

using Microsoft.Extensions.Logging;
using Moq;
using Shaos.Repository.Models;
using Shaos.Services.IO;
using Shaos.Services.Package;
using Shaos.Services.Processing;
using Shaos.Services.Runtime;
using Shaos.Services.Shared.Tests;
using Shaos.Services.Store;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests
{
    public class PlugInServiceTests : BaseTests
    {
        private const string PackageName = "Package";
        private const string PackageVersion = "1.0.0";

        private readonly Mock<IFileStoreService> _mockFileStoreService;
        private readonly Mock<INuGetProcessingService> _mockNuGetProcessingService;
        private readonly Mock<IPlugInValidationService> _mockPlugInValidationService;
        private readonly Mock<IRuntimeService> _mockRuntimeService;
        private readonly Mock<IStore> _mockStore;
        private readonly PlugInService _plugInService;

        public PlugInServiceTests(ITestOutputHelper output) : base(output)
        {
            _mockFileStoreService = new Mock<IFileStoreService>();
            _mockNuGetProcessingService = new Mock<INuGetProcessingService>();
            _mockPlugInValidationService = new Mock<IPlugInValidationService>();
            _mockRuntimeService = new Mock<IRuntimeService>();
            _mockStore = new Mock<IStore>();

            _plugInService = new PlugInService(
                Factory!.CreateLogger<PlugInService>(),
                _mockStore.Object,
                _mockRuntimeService.Object,
                _mockFileStoreService.Object,
                _mockNuGetProcessingService.Object,
                _mockPlugInValidationService.Object);
        }

        [Fact]
        public async Task TestDownloadPlugInNuGetAsync()
        {
            var specification = new NuGetSpecification()
            {
                Id = PackageName,
                Version = PackageVersion
            };

            var packageDownloadResult = new PackageDownload(
                new PackageSpecification(PackageName, PackageVersion));

            packageDownloadResult.ExtractedFiles.Add("C:\\NONEXISTANT\\lib\\plugin.dll");
            packageDownloadResult.ExtractedFiles.Add("C:\\NONEXISTANT\\x.nuspec");

            var downloads = new List<PackageDownload>()
            {
                packageDownloadResult
            };

            var downloadResult = new DownloadNuGetResult(true, downloads);

            _mockStore.Setup(_ => _.GetPlugInByIdAsync(
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlugIn());

            _mockNuGetProcessingService.Setup(_ => _.DownloadNuGetAsync(
                It.IsAny<NuGetSpecification>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(downloadResult);

            _mockPlugInValidationService.Setup(_ => _.ValidatePlugIn(It.IsAny<String>()))
                .Returns(true);

            var result = await _plugInService.DownloadPlugInNuGetAsync(1, specification);

            Assert.NotNull(result);
            Assert.Equal(DownloadPlugInNuGetStatus.Success, result.Status);
        }
    }
}