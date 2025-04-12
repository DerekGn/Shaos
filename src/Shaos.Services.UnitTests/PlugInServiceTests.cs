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
using Shaos.Services.Exceptions;
using Shaos.Services.IO;
using Shaos.Services.Repositories;
using Shaos.Services.Runtime;
using Shaos.Services.Shared.Tests;
using Shaos.Test.PlugIn;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests
{
    public class PlugInServiceTests : BaseTests
    {
        private readonly Mock<IFileStoreService> _mockFileStoreService;
        private readonly Mock<IPlugInInstanceRepository> _mockPlugInInstanceRepository;
        private readonly Mock<IPlugInRepository> _mockPlugInRepository;
        private readonly Mock<IRuntimeAssemblyLoadContext> _mockRuntimeAssemblyLoadContext;
        private readonly Mock<IRuntimeAssemblyLoadContextFactory> _mockRuntimeAssemblyLoadContextFactory;
        private readonly Mock<IRuntimeService> _mockRuntimeService;
        private readonly PlugInService _plugInService;

        public PlugInServiceTests(ITestOutputHelper output) : base(output)
        {
            _mockFileStoreService = new Mock<IFileStoreService>();
            _mockRuntimeAssemblyLoadContextFactory = new Mock<IRuntimeAssemblyLoadContextFactory>();
            _mockRuntimeAssemblyLoadContext = new Mock<IRuntimeAssemblyLoadContext>();
            _mockRuntimeService = new Mock<IRuntimeService>();
            _mockPlugInInstanceRepository = new Mock<IPlugInInstanceRepository>();
            _mockPlugInRepository = new Mock<IPlugInRepository>();

            _plugInService = new PlugInService(
                LoggerFactory!.CreateLogger<PlugInService>(),
                _mockRuntimeService.Object,
                _mockFileStoreService.Object,
                _mockPlugInRepository.Object,
                _mockPlugInInstanceRepository.Object,
                _mockRuntimeAssemblyLoadContextFactory.Object);
        }

        [Fact]
        public async Task TestCreatePlugInInstanceDuplicateNameAsync()
        {
            SetupPlugInGetByIdAsync();

            _mockPlugInInstanceRepository.Setup(_ => _.CreatePlugInInstanceAsync(
                It.IsAny<PlugIn>(),
                It.IsAny<PlugInInstance>(),
                It.IsAny<CancellationToken>()))
                .Throws(() => new PlugInInstanceNameExistsException("name"));

            await Assert.ThrowsAsync<PlugInInstanceNameExistsException>(async () =>
            await _plugInService.CreatePlugInInstanceAsync(1, new PlugInInstance()
            {
                Description = "description",
                Name = "name"
            }));
        }

        [Fact]
        public async Task TestCreatePlugInInstancePlugInNotFoundAsync()
        {
            await Assert.ThrowsAsync<PlugInNotFoundException>(async () =>
                await _plugInService.CreatePlugInInstanceAsync(1, new PlugInInstance()
                {
                    Description = "description",
                    Name = "name"
                })
            );
        }

        [Fact]
        public async Task TestCreatePlugInInstanceSuccessAsync()
        {
            SetupPlugInGetByIdAsync();

            _mockPlugInInstanceRepository.Setup(_ => _.CreatePlugInInstanceAsync(
                It.IsAny<PlugIn>(),
                It.IsAny<PlugInInstance>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(10);

            var result = await _plugInService.CreatePlugInInstanceAsync(1, new PlugInInstance()
            {
                Description = "description",
                Name = "name"
            });

            Assert.Equal(10, result);
        }

        [Fact]
        public async Task TestDeletePlugInInstanceRunningAsync()
        {
            _mockRuntimeService.Setup(_ => _.GetInstance(
                It.IsAny<int>()))
                .Returns(new Instance() { Name = "Test" });

            await Assert.ThrowsAsync<PlugInInstanceRunningException>(async () =>
                await _plugInService.DeletePlugInInstanceAsync(12));
        }

        [Fact]
        public async Task TestDeletePlugInInstanceSuccessAsync()
        {
            await _plugInService.DeletePlugInInstanceAsync(12);

            _mockPlugInInstanceRepository.Verify(_ => _.DeleteAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TestDeletePlugInNotFoundAsync()
        {
            await Assert.ThrowsAsync<PlugInNotFoundException>(async () =>
                await _plugInService.DeletePlugInAsync(1));
        }

        [Fact]
        public async Task TestDeletePlugInRunningAsync()
        {
            var plugIn = SetupPlugInGetByIdAsync();

            plugIn.Instances.Add(new PlugInInstance()
            {
                Id = 10,
                Name = "Test",
                Description = "description"
            });

            _mockRuntimeService.Setup(_ => _.GetInstance(
                It.IsAny<int>()))
                .Returns(new Instance()
                {
                    Id = 10,
                    Name = "Test"
                });

            await Assert.ThrowsAsync<PlugInInstanceRunningException>(async () =>
                await _plugInService.DeletePlugInAsync(1));
        }

        [Fact]
        public async Task TestDeletePlugInSuccessAsync()
        {
            var plugIn = SetupPlugInGetByIdAsync();

            plugIn.Package = new Package();

            plugIn.Instances.Add(new PlugInInstance()
            {
                Id = 10,
                Name = "Test",
                Description = "description"
            });

            await _plugInService.DeletePlugInAsync(1);

            _mockFileStoreService.Verify(_ => _.DeletePackage(
                It.IsAny<int>(),
                It.IsAny<string>()),
                Times.Once);

            _mockPlugInRepository.Verify(_ => _.DeleteAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TestSetPlugInInstanceEnableNotFoundAsync()
        {
            await Assert.ThrowsAsync<PlugInInstanceNotFoundException>(async () =>
                await _plugInService.SetPlugInInstanceEnableAsync(10, true));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TestSetPlugInInstanceEnableSuccessAsync(bool state)
        {
            _mockPlugInInstanceRepository
                .Setup(_ => _.GetByIdAsync(
                    It.IsAny<int>(),
                    It.IsAny<bool>(),
                    It.IsAny<List<string>?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new PlugInInstance()
                    {
                        Name = "name",
                        Description = "description"
                    });

            var result = await _plugInService.SetPlugInInstanceEnableAsync(10, state);

            Assert.NotNull(result);
            Assert.Equal(state, result.Enabled);

            _mockPlugInInstanceRepository
                .Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestUploadPlugInPackageExistsAsync()
        {
            MemoryStream stream = new MemoryStream();

            SetupPlugInGetByIdAsync();

            _mockFileStoreService
                .Setup(_ => _.PackageExists(It.IsAny<string>()))
                .Returns(true);

            var result = await _plugInService
                .UploadPlugInPackageAsync(1, "filename", stream);

            Assert.Equal(UploadPackageResult.PackageExists, result);

            _mockFileStoreService
                .Verify(_ => _.WritePackageFileStreamAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Stream>(),
                    It.IsAny<CancellationToken>()),
                    Times.Never);

            _mockPlugInRepository.Verify(_ => _.CreatePackageAsync(
                It.IsAny<PlugIn>(),
                It.IsAny<Package>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task TestUploadPlugInPackageNoValidPlugInAsync()
        {
            MemoryStream stream = new MemoryStream();

            SetupPlugInGetByIdAsync();

            _mockFileStoreService
                .Setup(_ => _.PackageExists(It.IsAny<string>()))
                .Returns(false);

            _mockFileStoreService
                .Setup(_ => _.ExtractPackage(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(new List<string>()
                {
                    "file.dll"
                });

            var result = await _plugInService
                .UploadPlugInPackageAsync(1, "filename", stream);

            Assert.Equal(UploadPackageResult.NoValidPlugIn, result);

            _mockFileStoreService
                .Verify(_ => _.WritePackageFileStreamAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Stream>(),
                    It.IsAny<CancellationToken>()),
                    Times.Once);

            _mockPlugInRepository.Verify(_ => _.CreatePackageAsync(
                It.IsAny<PlugIn>(),
                It.IsAny<Package>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task TestUploadPlugInPackagePlugInRunningAsync()
        {
            MemoryStream stream = new MemoryStream();

            var plugIn = SetupPlugInGetByIdAsync();

            plugIn.Instances.Add(new PlugInInstance()
            {
                Id = 1,
                Name = "name",
                Description = "description"
            });

            _mockRuntimeService
                .Setup(_ => _.GetInstance(
                    It.IsAny<int>()))
                .Returns(new Instance()
                {
                    State = InstanceState.Active,
                    Name = "Test"
                });

            var result = await _plugInService
                .UploadPlugInPackageAsync(1, "filename", stream);

            Assert.Equal(UploadPackageResult.PlugInRunning, result);

            _mockFileStoreService
                .Verify(_ => _.WritePackageFileStreamAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Stream>(),
                    It.IsAny<CancellationToken>()),
                    Times.Never);

            _mockPlugInRepository.Verify(_ => _.CreatePackageAsync(
                    It.IsAny<PlugIn>(),
                    It.IsAny<Package>(),
                    It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        [Fact]
        public async Task TestUploadPlugInPackageSuccessAsync()
        {
            MemoryStream stream = new MemoryStream();

            SetupPlugInGetByIdAsync();

            _mockFileStoreService
                .Setup(_ => _.PackageExists(
                    It.IsAny<string>()))
                .Returns(false);

            _mockFileStoreService
                .Setup(_ => _.ExtractPackage(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(new List<string>()
                {
                    ".PlugIn.dll"
                });

            _mockRuntimeAssemblyLoadContextFactory.Setup(_ => _.Create(
                It.IsAny<string>()))
                .Returns(_mockRuntimeAssemblyLoadContext.Object);

            _mockRuntimeAssemblyLoadContext.Setup(_ => _.LoadFromAssemblyPath(
                It.IsAny<string>()))
                .Returns(typeof(TestPlugIn).Assembly);

            var result = await _plugInService
                .UploadPlugInPackageAsync(1, "filename", stream);

            Assert.Equal(UploadPackageResult.Success, result);

            _mockFileStoreService
                .Verify(_ => _.WritePackageFileStreamAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Stream>(),
                    It.IsAny<CancellationToken>()),
                    Times.Once);

            _mockPlugInRepository.Verify(_ => _.CreatePackageAsync(
                It.IsAny<PlugIn>(),
                It.IsAny<Package>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private PlugIn SetupPlugInGetByIdAsync()
        {
            var plugIn = new PlugIn()
            {
                Name = "plugin",
                Description = "description"
            };

            _mockPlugInRepository
                .Setup(_ => _.GetByIdAsync(
                    It.IsAny<int>(),
                    It.IsAny<bool>(),
                    It.IsAny<List<string>?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(plugIn);

            return plugIn;
        }
    }
}