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
using Shaos.Sdk;
using Shaos.Services.Exceptions;
using Shaos.Services.IO;
using Shaos.Services.Runtime;
using Shaos.Services.Shared.Tests;
using Shaos.Services.Store;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests
{
    public class PlugInServiceTests : BaseTests
    {
        private readonly Mock<IAssemblyValidationService> _mockAssemblyValidationService;
        private readonly Mock<IFileStoreService> _mockFileStoreService;
        private readonly Mock<IRuntimeService> _mockRuntimeService;
        private readonly Mock<IStore> _mockStore;
        private readonly PlugInService _plugInService;

        public PlugInServiceTests(ITestOutputHelper output) : base(output)
        {
            _mockFileStoreService = new Mock<IFileStoreService>();
            _mockAssemblyValidationService = new Mock<IAssemblyValidationService>();
            _mockRuntimeService = new Mock<IRuntimeService>();
            _mockStore = new Mock<IStore>();

            _plugInService = new PlugInService(
                LoggerFactory!.CreateLogger<PlugInService>(),
                _mockStore.Object,
                _mockRuntimeService.Object,
                _mockFileStoreService.Object,
                _mockAssemblyValidationService.Object);
        }

        [Fact]
        public async Task TestCreatePlugInInstanceDuplicateNameAsync()
        {
            SetupPlugInGet(new PlugIn());

            _mockStore.Setup(_ => _.CreatePlugInInstanceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<PlugIn>(),
                It.IsAny<CancellationToken>()))
                .Throws(() => new PlugInInstanceNameExistsException("name"));

            await Assert.ThrowsAsync<PlugInInstanceNameExistsException>(async () =>
            await _plugInService.CreatePlugInInstanceAsync(1, new CreatePlugInInstance()
            {
                Description = "description",
                Name = "name"
            }));
        }

        [Fact]
        public async Task TestCreatePlugInInstancePlugInNotFoundAsync()
        {
            await Assert.ThrowsAsync<PlugInNotFoundException>(async () =>
                await _plugInService.CreatePlugInInstanceAsync(1, new CreatePlugInInstance()
                {
                    Description = "description",
                    Name = "name"
                })
            );
        }

        [Fact]
        public async Task TestCreatePlugInInstanceSuccessAsync()
        {
            SetupPlugInGet(new PlugIn());

            _mockStore.Setup(_ => _.CreatePlugInInstanceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<PlugIn>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(10);

            var result = await _plugInService.CreatePlugInInstanceAsync(1, new CreatePlugInInstance()
            {
                Description = "description",
                Name = "name"
            });

            Assert.Equal(10, result);
        }

        [Fact]
        public async Task TestDeletePlugInInstanceRunningAsync()
        {
            _mockRuntimeService.Setup(_ => _.GetExecutingInstance(
                It.IsAny<int>()))
                .Returns(new ExecutingInstance());

            await Assert.ThrowsAsync<PlugInInstanceRunningException>(async () =>
                await _plugInService.DeletePlugInInstanceAsync(12));
        }

        [Fact]
        public async Task TestDeletePlugInInstanceSuccessAsync()
        {
            await _plugInService.DeletePlugInInstanceAsync(12);

            _mockStore.Verify(_ => _.DeleteAsync<PlugInInstance>(
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
            var plugIn = new PlugIn();

            plugIn.Instances.Add(new PlugInInstance()
            {
                Id = 10
            });

            SetupPlugInGet(plugIn);

            _mockRuntimeService.Setup(_ => _.GetExecutingInstance(
                It.IsAny<int>()))
                .Returns(new ExecutingInstance()
                {
                    Id = 10
                });

            await Assert.ThrowsAsync<PlugInInstanceRunningException>(async () =>
                await _plugInService.DeletePlugInAsync(1));
        }

        [Fact]
        public async Task TestDeletePlugInSuccessAsync()
        {
            var plugIn = new PlugIn()
            {
                Package = new Package()
            };

            plugIn.Instances.Add(new PlugInInstance()
            {
                Id = 10
            });

            SetupPlugInGet(plugIn);

            await _plugInService.DeletePlugInAsync(1);

            _mockFileStoreService.Verify(_ => _.DeletePlugInPackage(
                It.IsAny<int>(),
                It.IsAny<string>()),
                Times.Once);

            _mockStore.Verify(_ => _.DeleteAsync<PlugIn>(
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
            _mockStore.Setup(_ => _.GetPlugInInstanceByIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlugInInstance());

            var result = await _plugInService.SetPlugInInstanceEnableAsync(10, state);

            Assert.NotNull(result);
            Assert.Equal(state, result.Enabled);

            _mockStore.Verify(_ => _.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TestUploadPlugInPackageExistsAsync()
        {
            MemoryStream stream = new MemoryStream();

            _mockStore
                .Setup(_ => _.GetPlugInByIdAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlugIn());

            _mockFileStoreService
                .Setup(_ => _.PackageExists(It.IsAny<string>()))
                .Returns(true);

            var result = await _plugInService
                .UploadPlugInPackageAsync(1, "filename", stream);

            Assert.Equal(UploadPackageResult.PackageExists, result);

            _mockFileStoreService
                .Verify(_ => _.WritePlugInPackageFileStreamAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Stream>(),
                    It.IsAny<CancellationToken>()),
                    Times.Never);

            _mockStore.Verify(_ => _.CreatePlugInPackageAsync(
                It.IsAny<PlugIn>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task TestUploadPlugInPackageNoValidPlugInAsync()
        {
            MemoryStream stream = new MemoryStream();

            _mockStore
                .Setup(_ => _.GetPlugInByIdAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlugIn());

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

            var version = "1.0.0.0";

            _mockAssemblyValidationService
                .Setup(_ => _.ValidateContainsType<IPlugIn>(
                    It.IsAny<string>(),
                    out version))
                .Returns(false);

            var result = await _plugInService
                .UploadPlugInPackageAsync(1, "filename", stream);

            Assert.Equal(UploadPackageResult.NoValidPlugIn, result);

            _mockFileStoreService
                .Verify(_ => _.WritePlugInPackageFileStreamAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Stream>(),
                    It.IsAny<CancellationToken>()),
                    Times.Once);

            _mockStore.Verify(_ => _.CreatePlugInPackageAsync(
                It.IsAny<PlugIn>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task TestUploadPlugInPackagePlugInRunningAsync()
        {
            MemoryStream stream = new MemoryStream();

            var plugIn = new PlugIn();

            plugIn.Instances.Add(new PlugInInstance()
            {
                Id = 1
            });

            _mockStore
                .Setup(_ => _.GetPlugInByIdAsync(
                    It.IsAny<int>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(plugIn);

            _mockRuntimeService
                .Setup(_ => _.GetExecutingInstance(
                    It.IsAny<int>()))
                .Returns(new ExecutingInstance()
                {
                    State = ExecutionState.Active
                });

            var result = await _plugInService
                .UploadPlugInPackageAsync(1, "filename", stream);

            Assert.Equal(UploadPackageResult.PlugInRunning, result);

            _mockFileStoreService
                .Verify(_ => _.WritePlugInPackageFileStreamAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Stream>(),
                    It.IsAny<CancellationToken>()),
                    Times.Never);

            _mockStore.Verify(_ => _.CreatePlugInPackageAsync(
                It.IsAny<PlugIn>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task TestUploadPlugInPackageSuccessAsync()
        {
            MemoryStream stream = new MemoryStream();

            _mockStore
                .Setup(_ => _.GetPlugInByIdAsync(
                    It.IsAny<int>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlugIn());

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
                    "file.dll"
                });

            var version = "1.0.0.0";

            _mockAssemblyValidationService
                .Setup(_ => _.ValidateContainsType<IPlugIn>(
                    It.IsAny<string>(),
                    out version))
                .Returns(true);

            var result = await _plugInService
                .UploadPlugInPackageAsync(1, "filename", stream);

            Assert.Equal(UploadPackageResult.Success, result);

            _mockFileStoreService
                .Verify(_ => _.WritePlugInPackageFileStreamAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Stream>(),
                    It.IsAny<CancellationToken>()),
                    Times.Once);

            _mockStore.Verify(_ => _.CreatePlugInPackageAsync(
                It.IsAny<PlugIn>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private void SetupPlugInGet(PlugIn plugIn)
        {
            _mockStore.Setup(_ => _.GetPlugInByIdAsync(
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(plugIn);
        }
    }
}