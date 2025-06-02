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
using Shaos.Repository;
using Shaos.Repository.Exceptions;
using Shaos.Repository.Models;
using Shaos.Services.Exceptions;
using Shaos.Services.IO;
using Shaos.Services.Runtime.Exceptions;
using Shaos.Services.Runtime.Host;
using Shaos.Testing.Shared;
using Shaos.Testing.Shared.Extensions;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests
{
    public class InstanceHostServiceTest : BaseTests
    {
        private readonly InstanceHostService _instanceHostService;
        private readonly Mock<IFileStoreService> _mockFileStoreService;
        private readonly Mock<IInstanceHost> _mockInstanceHost;
        private readonly Mock<IShaosRepository> _mockRepository;

        public InstanceHostServiceTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            _mockFileStoreService = new Mock<IFileStoreService>();
            _mockInstanceHost = new Mock<IInstanceHost>();
            _mockRepository = new Mock<IShaosRepository>();

            _instanceHostService = new InstanceHostService(LoggerFactory!.CreateLogger<InstanceHostService>(),
                                                           _mockInstanceHost.Object,
                                                           _mockRepository.Object,
                                                           _mockFileStoreService.Object);
        }

        [Fact]
        public async Task TestLoadInstanceConfigurationNotFoundAsync()
        {
            var exception = await Assert.ThrowsAsync<ShaosNotFoundException>(
                async () => await _instanceHostService.LoadInstanceConfigurationAsync(1));

            Assert.NotNull(exception);
            Assert.Equal(1, exception.Id);
        }

        [Fact]
        public async Task TestLoadInstanceConfigurationWithConfigurationAsync()
        {
            var plugIn = new PlugIn()
            {
                Description = "Test",
                Name = "Test",
                Package = new Package()
                {
                    HasConfiguration = true
                }
            };

            _mockRepository
                .Setup(_ => _.GetByIdAsync<PlugInInstance>(It.IsAny<int>(),
                                                           It.IsAny<bool>(),
                                                           It.IsAny<List<string>?>(),
                                                           It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlugInInstance()
                {
                    Configuration = "configuration",
                    PlugIn = plugIn
                });

            _mockInstanceHost.Setup(_ => _.LoadConfiguration(1)).Returns(new object());

            var result = await _instanceHostService.LoadInstanceConfigurationAsync(1);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task TestLoadInstanceConfigurationWithoutConfigurationAsync()
        {
            var plugIn = new PlugIn()
            {
                Description = "Test",
                Name = "Test",
                Package = new Package()
            };

            _mockRepository.
                Setup(_ => _.GetByIdAsync<PlugInInstance>(It.IsAny<int>(),
                                                          It.IsAny<bool>(),
                                                          It.IsAny<List<string>?>(),
                                                          It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlugInInstance()
                {
                    PlugIn = plugIn
                });

            var exception = await Assert.ThrowsAsync<PlugInHasNoConfigurationException>(
                async () => await _instanceHostService.LoadInstanceConfigurationAsync(1));

            Assert.NotNull(exception);
            Assert.Equal(1, exception.Id);
        }

        [Fact]
        public async Task TestStartInstanceAsync()
        {
            var plugIn = new PlugIn()
            {
                Description = "Test",
                Name = "Test",
                Package = new Package()
            };

            _mockInstanceHost
                .Setup(_ => _.InstanceExists(It.IsAny<int>()))
                .Returns(true);

            _mockRepository.
                Setup(_ => _.GetByIdAsync<PlugInInstance>(It.IsAny<int>(),
                                                          It.IsAny<bool>(),
                                                          It.IsAny<List<string>?>(),
                                                          It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlugInInstance()
                {
                    PlugIn = plugIn
                });

            await _instanceHostService.StartInstanceAsync(1);

            _mockInstanceHost
                .Verify(_ => _.InstanceExists(1), Times.Once);

            _mockRepository
                .Verify(_ => _.GetByIdAsync<PlugInInstance>(It.IsAny<int>(),
                                                          It.IsAny<bool>(),
                                                          It.IsAny<List<string>?>(),
                                                          It.IsAny<CancellationToken>()),
                                                          Times.Once);
        }

        [Fact]
        public async Task TestStartInstanceNotFoundAsync()
        {
            _mockInstanceHost
                .Setup(_ => _.InstanceExists(It.IsAny<int>()))
                .Returns(false);

            var exception = await Assert.ThrowsAsync<InstanceNotFoundException>(
                async () => await _instanceHostService.StartInstanceAsync(1));

            Assert.NotNull(exception);
            Assert.Equal(1, exception.Id);
        }

        [Fact]
        public async Task TestStartInstancesAsync()
        {
            var plugIn = new PlugIn()
            {
                Id = 1,
                Description = "description",
                Name = "name",
                Package = new Package()
                {
                    AssemblyFile = "AssemblyFile"
                }
            };

            plugIn.Instances.Add(new PlugInInstance()
            {
                Id = 1,
                Enabled = true,
            });

            List<PlugIn> plugIns =
            [
                plugIn
            ];

            var instance = new Instance(1, 2, "name", new InstanceConfiguration(true, "configuration"));

            _mockRepository.Setup(_ => _.GetAsync<PlugIn>(It.IsAny<Expression<Func<PlugIn, bool>>?>(),
                                                          It.IsAny<Func<IQueryable<PlugIn>, IOrderedQueryable<PlugIn>>?>(),
                                                          It.IsAny<bool>(),
                                                          It.IsAny<List<string>?>(),
                                                          It.IsAny<CancellationToken>()))
                .Returns(plugIns.ToAsyncEnumerable());

            _mockFileStoreService
                .Setup(_ => _.GetAssemblyPath(It.IsAny<int>(),
                                              It.IsAny<string>()))
                .Returns("AssemblyFile");

            _mockInstanceHost.Setup(_ => _.CreateInstance(It.IsAny<int>(),
                                                          It.IsAny<int>(),
                                                          It.IsAny<string>(),
                                                          It.IsAny<string>(),
                                                          It.IsAny<InstanceConfiguration>()))
                .Returns(instance);

            await _instanceHostService.StartInstancesAsync();

            _mockRepository.Verify(_ => _.GetAsync<PlugIn>(It.IsAny<Expression<Func<PlugIn, bool>>?>(),
                                                           It.IsAny<Func<IQueryable<PlugIn>, IOrderedQueryable<PlugIn>>?>(),
                                                           It.IsAny<bool>(),
                                                           It.IsAny<List<string>?>(),
                                                           It.IsAny<CancellationToken>()),
                    Times.Once);

            _mockFileStoreService
                .Verify(_ => _.GetAssemblyPath(It.IsAny<int>(),
                                               It.IsAny<string>()),
                                               Times.Once);

            _mockInstanceHost
                .Verify(_ => _.CreateInstance(It.IsAny<int>(),
                                              It.IsAny<int>(),
                                              It.IsAny<string>(),
                                              It.IsAny<string>(),
                                              It.IsAny<InstanceConfiguration>()),
                                              Times.Once);

            _mockInstanceHost
                .Verify(_ => _.StartInstance(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task TestStartPlugInInstanceNotConfiguredAsync()
        {
            var plugIn = new PlugIn()
            {
                Description = "Test",
                Name = "Test",
                Package = new Package()
                {
                    HasConfiguration = true
                }
            };

            _mockInstanceHost
                .Setup(_ => _.InstanceExists(It.IsAny<int>()))
                .Returns(true);

            _mockRepository.
                Setup(_ => _.GetByIdAsync<PlugInInstance>(It.IsAny<int>(),
                                                          It.IsAny<bool>(),
                                                          It.IsAny<List<string>?>(),
                                                          It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlugInInstance()
                {
                    PlugIn = plugIn
                });

            var exception = await Assert.ThrowsAsync<PlugInInstanceNotConfiguredException>(
                async () => await _instanceHostService.StartInstanceAsync(1));

            Assert.NotNull(exception);
            Assert.Equal(1, exception.Id);
        }

        [Fact]
        public async Task TestStartPlugInInstanceNotFoundAsync()
        {
            _mockInstanceHost
                .Setup(_ => _.InstanceExists(It.IsAny<int>()))
                .Returns(true);

            _mockRepository.
                Setup(_ => _.GetByIdAsync<PlugInInstance>(It.IsAny<int>(),
                                                          It.IsAny<bool>(),
                                                          It.IsAny<List<string>?>(),
                                                          It.IsAny<CancellationToken>()))
                .ReturnsAsync((PlugInInstance)null!);

            var exception = await Assert.ThrowsAsync<PlugInInstanceNotFoundException>(
                async () => await _instanceHostService.StartInstanceAsync(1));

            Assert.NotNull(exception);
            Assert.Equal(1, exception.Id);
        }
    }
}