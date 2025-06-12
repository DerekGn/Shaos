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
using Shaos.Services.Runtime.Factories;
using Shaos.Services.Runtime.Host;
using Shaos.Services.Runtime.Validation;
using Shaos.Testing.Shared;
using Shaos.Testing.Shared.Extensions;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests
{
    public class PlugInServiceTests : BaseTests
    {
        private readonly Mock<ITypeLoaderService> _mockTypeLoaderService;
        private readonly Mock<IFileStoreService> _mockFileStoreService;
        private readonly Mock<IInstanceHost> _mockInstanceHost;
        private readonly Mock<IPlugInFactory> _mockPlugInFactory;
        private readonly Mock<IPlugInTypeValidator> _mockPlugInTypeValidator;
        private readonly Mock<IShaosRepository> _mockRepository;
        private readonly PlugInService _plugInService;

        public PlugInServiceTests(ITestOutputHelper output) : base(output)
        {
            _mockFileStoreService = new Mock<IFileStoreService>();
            _mockInstanceHost = new Mock<IInstanceHost>();
            _mockPlugInFactory = new Mock<IPlugInFactory>();
            _mockPlugInTypeValidator = new Mock<IPlugInTypeValidator>();
            _mockRepository = new Mock<IShaosRepository>();
            _mockTypeLoaderService = new Mock<ITypeLoaderService>();

            _plugInService = new PlugInService(LoggerFactory!.CreateLogger<PlugInService>(),
                                               _mockInstanceHost.Object,
                                               _mockRepository.Object,
                                               _mockPlugInFactory.Object,
                                               _mockFileStoreService.Object,
                                               _mockPlugInTypeValidator.Object,
                                               _mockTypeLoaderService.Object);
        }

        [Fact]
        public async Task TestCreatePlugInInstancePackageNotAssignedAsync()
        {
            SetupPlugInGetByIdAsync();

            await Assert.ThrowsAsync<PlugInPackageNotAssignedException>(async () =>
            await _plugInService.CreatePlugInInstanceAsync(1, new PlugInInstance()
            {
                Description = "description",
                Name = "name"
            }));
        }

        [Fact]
        public async Task TestCreatePlugInInstancePlugInNotFoundAsync()
        {
            await Assert.ThrowsAsync<ShaosNotFoundException>(async () =>
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
            var plugIn = SetupPlugInGetByIdAsync();

            plugIn.Package = new Package()
            {
                AssemblyFile = "assemblyfile"
            };

            _mockRepository.Setup(_ => _.CreatePlugInInstanceAsync(It.IsAny<PlugIn>(),
                                                                   It.IsAny<PlugInInstance>(),
                                                                   It.IsAny<CancellationToken>()))
                .ReturnsAsync(10);

            _mockFileStoreService.Setup(_ => _.GetAssemblyPath(It.IsAny<int>(),
                                                               It.IsAny<string>()))
                .Returns("");

            var result = await _plugInService.CreatePlugInInstanceAsync(1, new PlugInInstance()
            {
                Description = "description",
                Name = "name"
            });

            Assert.Equal(10, result);
        }

        [Fact]
        public async Task TestDeletePlugInInstanceNotExistAsync()
        {
            _mockInstanceHost
                .Setup(_ => _.Instances)
                .Returns([]);

            await _plugInService.DeletePlugInInstanceAsync(12);

            _mockRepository.Verify(_ => _.DeleteAsync<PlugInInstance>(It.IsAny<int>(),
                                                                      It.IsAny<CancellationToken>()),
                                                                      Times.Never);
        }

        [Fact]
        public async Task TestDeletePlugInInstanceRunningAsync()
        {
            var configuration = new InstanceConfiguration(true, string.Empty);
            var instance = new Instance(12, 1, "Test", InstanceState.None, configuration);

            instance.SetRunning();

            _mockInstanceHost
                .Setup(_ => _.Instances)
                .Returns([instance]);

            var exception = await Assert.ThrowsAsync<PlugInInstanceRunningException>(async () =>
               await _plugInService.DeletePlugInInstanceAsync(12));

            Assert.NotNull(exception);
            Assert.Equal(12, exception.Id);
        }

        [Fact]
        public async Task TestDeletePlugInInstanceSuccessAsync()
        {
            var configuration = new InstanceConfiguration(true, string.Empty);
            var instance = new Instance(12, 1, "Test", InstanceState.None, configuration);

            instance.SetComplete();

            _mockInstanceHost
                .Setup(_ => _.Instances)
                .Returns([instance]);

            await _plugInService.DeletePlugInInstanceAsync(12);

            _mockRepository.Verify(_ => _.DeleteAsync<PlugInInstance>(It.IsAny<int>(),
                                                                      It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TestDeletePlugInNotFoundAsync()
        {
            await Assert.ThrowsAsync<ShaosNotFoundException>(async () =>
                await _plugInService.DeletePlugInAsync(1));
        }

        [Fact]
        public async Task TestDeletePlugInRunningAsync()
        {
            var plugIn = SetupPlugInGetByIdAsync();

            var plugInInstance = new PlugInInstance()
            {
                Id = 1,
                Name = "Test",
                Description = "description"
            };

            plugIn.Instances.Add(plugInInstance);

            var configuration = new InstanceConfiguration(true, string.Empty);
            var instance = new Instance(plugInInstance.Id, 1, "Test", InstanceState.None, configuration);

            instance.SetRunning();

            _mockInstanceHost
                .Setup(_ => _.Instances)
                .Returns([instance]);

            var exception = await Assert.ThrowsAsync<PlugInInstanceRunningException>(async () =>
                await _plugInService.DeletePlugInAsync(1));

            Assert.NotNull(exception);
            Assert.Equal(1, exception.Id);
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

            var configuration = new InstanceConfiguration(true, string.Empty);

            var instance = new Instance(12, 1, "Test", InstanceState.None, configuration);

            instance.SetComplete();

            _mockInstanceHost
                .Setup(_ => _.Instances)
                .Returns([instance]);

            await _plugInService.DeletePlugInAsync(1);

            _mockInstanceHost.Verify(_ => _.RemoveInstance(It.IsAny<int>()), Times.Once);

            _mockFileStoreService.Verify(_ => _.DeletePackage(It.IsAny<int>(),
                                                              It.IsAny<string>()),
                Times.Once);

            _mockRepository.Verify(_ => _.DeleteAsync<PlugInInstance>(It.IsAny<int>(),
                                                                      It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TestLoadPlugInInstanceConfigurationOkAsync()
        {
            var plugIn = SetupPlugInGetByIdAsync();
            plugIn.Package = new Package()
            {
                HasConfiguration = true
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

            _mockTypeLoaderService.Setup(_ => _.LoadConfiguration(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string?>()))
                .Returns(new object());

            var result = await _plugInService.LoadPlugInInstanceConfigurationAsync(1);

            Assert.NotNull(result);
            Assert.IsType<object>(result);
        }

        [Fact]
        public async Task TestLoadPlugInInstanceConfigurationPackageHasNoConfigurationAsync()
        {
            var plugIn = SetupPlugInGetByIdAsync();
            plugIn.Package = new Package()
            {
                HasConfiguration = false
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

            var exception = await Assert.ThrowsAsync<PackageHasNoConfigurationException>(async () =>
                await _plugInService.LoadPlugInInstanceConfigurationAsync(1));

            Assert.NotNull(exception);
            Assert.Equal(1, exception.Id);
        }

        [Fact]
        public async Task TestLoadPlugInInstanceConfigurationPackageNotAssignedAsync()
        {
            var plugIn = SetupPlugInGetByIdAsync();

            _mockRepository.
               Setup(_ => _.GetByIdAsync<PlugInInstance>(It.IsAny<int>(),
                                                         It.IsAny<bool>(),
                                                         It.IsAny<List<string>?>(),
                                                         It.IsAny<CancellationToken>()))
               .ReturnsAsync(new PlugInInstance()
               {
                   PlugIn = plugIn
               });

            var exception = await Assert.ThrowsAsync<PlugInPackageNotAssignedException>(async () =>
                await _plugInService.LoadPlugInInstanceConfigurationAsync(1));

            Assert.NotNull(exception);
            Assert.Equal(1, exception.Id);
        }

        [Fact]
        public async Task TestSetPlugInInstanceEnableNotFoundAsync()
        {
            await Assert.ThrowsAsync<ShaosNotFoundException>(async () =>
                await _plugInService.SetPlugInInstanceEnableAsync(10, true));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TestSetPlugInInstanceEnableSuccessAsync(bool state)
        {
            _mockRepository
                .Setup(_ => _.GetByIdAsync<PlugInInstance>(It.IsAny<int>(),
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

            _mockRepository
                .Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestUploadPlugInPackageNoValidPlugInAsync()
        {
            MemoryStream stream = new MemoryStream();

            var plugIn = SetupPlugInGetByIdAsync();

            plugIn.Instances.Add(new PlugInInstance()
            {
                Id = 1,
                Description = "description",
                Name = "name",
                PlugInId = plugIn.Id
            });

            SetupRunningInstances();

            _mockFileStoreService
                .Setup(_ => _.ExtractPackage(
                    It.IsAny<int>(),
                    It.IsAny<string>()))
                .Returns(["file.dll"]);

            var exception = await Assert
                .ThrowsAsync<NoValidPlugInAssemblyFoundException>(async () => await _plugInService
                    .UploadPlugInPackageAsync(1, "filename", stream));

            Assert.Equal(
                "No assembly file ending with [.PlugIn.dll] was found in the package [filename] files",
                exception.Message);

            _mockFileStoreService
                .Verify(_ => _.WritePackageFileStreamAsync(It.IsAny<int>(),
                                                           It.IsAny<string>(),
                                                           It.IsAny<Stream>(),
                                                           It.IsAny<CancellationToken>()),
                    Times.Once);

            _mockRepository.Verify(_ => _.CreatePackageAsync(It.IsAny<PlugIn>(),
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
                Description = "description",
                Name = "name",
                PlugInId = plugIn.Id
            });

            var instance = SetupRunningInstances();

            instance.SetRunning();

            var exception = await Assert
                .ThrowsAsync<PlugInInstancesRunningException>(async () => await _plugInService
                    .UploadPlugInPackageAsync(1, "filename", stream));

            Assert.Equal(
                "Instances are Running",
                exception.Message);

            _mockFileStoreService
                .Verify(_ => _.WritePackageFileStreamAsync(It.IsAny<int>(),
                                                           It.IsAny<string>(),
                                                           It.IsAny<Stream>(),
                                                           It.IsAny<CancellationToken>()),
                    Times.Never);

            _mockRepository.Verify(_ => _.CreatePackageAsync(It.IsAny<PlugIn>(),
                                                             It.IsAny<Package>(),
                                                             It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        [Fact]
        public async Task TestUploadPlugInPackageSuccessAsync()
        {
            MemoryStream stream = new MemoryStream();

            var plugIn = SetupPlugInGetByIdAsync();

            plugIn.Instances.Add(new PlugInInstance()
            {
                Id = 1,
                Description = "description",
                Name = "name",
                PlugInId = plugIn.Id
            });

            SetupRunningInstances();

            _mockFileStoreService
                .Setup(_ => _.ExtractPackage(It.IsAny<int>(),
                                             It.IsAny<string>()))
                .Returns([".PlugIn.dll"]);

            _mockPlugInTypeValidator
                .Setup(_ => _.Validate(It.IsAny<string>()))
                .Returns(new PlugInTypeInformation("name", true, true, "1.0.0"));

            await _plugInService
                .UploadPlugInPackageAsync(1, "filename", stream);

            _mockFileStoreService
                .Verify(_ => _.WritePackageFileStreamAsync(It.IsAny<int>(),
                                                           It.IsAny<string>(),
                                                           It.IsAny<Stream>(),
                                                           It.IsAny<CancellationToken>()),
                    Times.Once);

            _mockRepository.Verify(_ => _.CreatePackageAsync(It.IsAny<PlugIn>(),
                                                             It.IsAny<Package>(),
                                                             It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TestUploadPlugInPackageUpdateSuccessAsync()
        {
            MemoryStream stream = new MemoryStream();

            var plugIn = SetupPlugInGetByIdAsync();

            plugIn.Package = new Package()
            {
                AssemblyFile = "assemblyfile"
            };

            plugIn.Instances.Add(new PlugInInstance()
            {
                Id = 1,
                Description = "description",
                Name = "name",
                PlugInId = plugIn.Id
            });

            SetupRunningInstances();

            _mockFileStoreService
                .Setup(_ => _.ExtractPackage(It.IsAny<int>(),
                                             It.IsAny<string>()))
                .Returns([".PlugIn.dll"]);

            _mockPlugInTypeValidator
               .Setup(_ => _.Validate(It.IsAny<string>()))
               .Returns(new PlugInTypeInformation("name", true, true, "1.0.0"));

            await _plugInService
                .UploadPlugInPackageAsync(1, "filename", stream);

            _mockFileStoreService
                .Verify(_ => _.WritePackageFileStreamAsync(It.IsAny<int>(),
                                                           It.IsAny<string>(),
                                                           It.IsAny<Stream>(),
                                                           It.IsAny<CancellationToken>()),
                    Times.Once);

            _mockRepository.Verify(_ => _.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private PlugIn SetupPlugInGetAsync()
        {
            var plugIn = new PlugIn()
            {
                Name = "plugin",
                Description = "description"
            };

            _mockRepository
                .Setup(_ => _.GetAsync(It.IsAny<Expression<Func<PlugIn, bool>>?>(),
                                       It.IsAny<Func<IQueryable<PlugIn>, IOrderedQueryable<PlugIn>>?>(),
                                       It.IsAny<bool>(),
                                       It.IsAny<List<string>?>(),
                                       It.IsAny<CancellationToken>()))
                .Returns(new List<PlugIn>() { plugIn }.ToAsyncEnumerable());

            return plugIn;
        }

        private PlugIn SetupPlugInGetByIdAsync()
        {
            var plugIn = new PlugIn()
            {
                Id = 1,
                Name = "plugin",
                Description = "description"
            };

            _mockRepository
                .Setup(_ => _.GetByIdAsync<PlugIn>(It.IsAny<int>(),
                                                   It.IsAny<bool>(),
                                                   It.IsAny<List<string>?>(),
                                                   It.IsAny<CancellationToken>()))
                .ReturnsAsync(plugIn);

            return plugIn;
        }

        private Instance SetupRunningInstances()
        {
            var configuration = new InstanceConfiguration(true, string.Empty);

            var instance = new Instance(1, 1, "Test", InstanceState.None, configuration);

            _mockInstanceHost
               .Setup(_ => _.Instances)
               .Returns([instance]);

            return instance;
        }
    }
}