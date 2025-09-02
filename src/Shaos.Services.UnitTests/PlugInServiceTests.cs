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
using Shaos.Repository.Exceptions;
using Shaos.Repository.Models;
using Shaos.Services.Exceptions;
using Shaos.Services.IO;
using Shaos.Services.Runtime.Host;
using Shaos.Services.Runtime.Validation;
using Shaos.Testing.Shared.Extensions;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests
{
    public class PlugInServiceTests : BaseServiceTests
    {
        private const string AssemblyPath = "AssemblyPath";
        private const string InstanceName = "Test";
        private readonly Mock<IFileStoreService> _mockFileStoreService;
        private readonly Mock<IRuntimeInstanceHost> _mockInstanceHost;
        private readonly Mock<IPlugInConfigurationBuilder> _mockPlugConfigurationBuilder;
        private readonly Mock<IPlugInTypeValidator> _mockPlugInTypeValidator;
        private readonly PlugInService _plugInService;

        public PlugInServiceTests(ITestOutputHelper output) : base(output)
        {
            _mockFileStoreService = new Mock<IFileStoreService>();
            _mockInstanceHost = new Mock<IRuntimeInstanceHost>();
            _mockPlugInTypeValidator = new Mock<IPlugInTypeValidator>();
            _mockPlugConfigurationBuilder = new Mock<IPlugInConfigurationBuilder>();

            _plugInService = new PlugInService(LoggerFactory!.CreateLogger<PlugInService>(),
                                               _mockInstanceHost.Object,
                                               MockRepository.Object,
                                               _mockFileStoreService.Object,
                                               _mockPlugInTypeValidator.Object,
                                               _mockPlugConfigurationBuilder.Object);
        }

        [Fact]
        public async Task TestCreatePlugInAsync()
        {
            _mockFileStoreService
                .Setup(_ => _.GetAssemblyPath(It.IsAny<string>(),
                                              It.IsAny<string>()))
                .Returns("path");

            _mockPlugInTypeValidator
                .Setup(_ => _.Validate(It.IsAny<string>()))
                .Returns(new PlugInTypeInformation());

            await _plugInService.CreatePlugInAsync("", "", "");

            MockRepository.Verify(_ => _.CreatePlugInAsync(It.IsAny<PlugIn>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(Skip = "Refactor")]
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
            await Assert.ThrowsAsync<NotFoundException>(async () =>
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

            plugIn.PlugInInformation = new PlugInInformation()
            {
                AssemblyFileName = "assemblyfile"
            };

            MockRepository.Setup(_ => _.CreatePlugInInstanceAsync(It.IsAny<PlugIn>(),
                                                                  It.IsAny<PlugInInstance>(),
                                                                  It.IsAny<CancellationToken>()))
                .ReturnsAsync(10);

            _mockFileStoreService.Setup(_ => _.GetAssemblyPath(It.IsAny<string>(),
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
        public void TestDeletePlugInFiles()
        {
            _plugInService.DeletePlugInFiles("packagePath", "plugInDirectory");

            _mockFileStoreService.Verify(_ => _.DeletePackage(It.IsAny<string>()), Times.Once);

            _mockFileStoreService.Verify(_ => _.DeletePlugDirectory(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task TestDeletePlugInInstanceNotExistAsync()
        {
            _mockInstanceHost
                .Setup(_ => _.Instances)
                .Returns([]);

            await _plugInService.DeletePlugInInstanceAsync(12);

            MockRepository.Verify(_ => _.DeleteAsync<PlugInInstance>(It.IsAny<int>(),
                                                                      It.IsAny<CancellationToken>()),
                                                                      Times.Never);
        }

        [Fact]
        public void TestExtractPackage()
        {
            var packageFile = "packageFile";
            var files = new List<string> { "test.PlugIn.dll" }.AsEnumerable();

            _mockFileStoreService.Setup(_ => _.ExtractPackage(It.IsAny<string>(), out packageFile, out files));

            var packageDetails = _plugInService.ExtractPackage("packagefilename");

            Assert.NotNull(packageDetails);

            Assert.NotEmpty(packageDetails.FileName);
            Assert.NotNull(packageDetails.Files);
            Assert.NotEmpty(packageDetails.Files);
            Assert.NotEmpty(packageDetails.PlugInAssemblyFileName);
            Assert.NotEmpty(packageDetails.PlugInDirectory);
        }

        [Fact]
        public async Task TestDeletePlugInInstanceRunningAsync()
        {
            var instance = new RuntimeInstance(12,
                                        1,
                                        InstanceName,
                                        AssemblyPath,
                                        RuntimeInstanceState.None);

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
            var instance = new RuntimeInstance(12, 1, InstanceName, AssemblyPath, RuntimeInstanceState.None);

            instance.SetComplete();

            _mockInstanceHost
                .Setup(_ => _.Instances)
                .Returns([instance]);

            await _plugInService.DeletePlugInInstanceAsync(12);

            MockRepository.Verify(_ => _.DeleteAsync<PlugInInstance>(It.IsAny<int>(),
                                                                     It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TestDeletePlugInNotFoundAsync()
        {
            await Assert.ThrowsAsync<NotFoundException>(async () =>
                await _plugInService.DeletePlugInAsync(1));
        }

        [Fact]
        public async Task TestDeletePlugInRunningAsync()
        {
            var plugIn = SetupPlugInGetByIdAsync();

            var plugInInstance = new PlugInInstance()
            {
                Id = 1,
                Name = InstanceName,
                Description = "description"
            };

            plugIn.Instances.Add(plugInInstance);

            var instance = new RuntimeInstance(plugInInstance.Id,
                                        1,
                                        InstanceName,
                                        AssemblyPath,
                                        RuntimeInstanceState.None);

            instance.SetRunning();

            _mockInstanceHost
                .Setup(_ => _.Instances)
                .Returns([instance]);

            var exception = await Assert.ThrowsAsync<PlugInInstanceRunningException>(async () =>
                await _plugInService.DeletePlugInAsync(1));

            Assert.NotNull(exception);
            Assert.Equal(1, exception.Id);
        }

        [Fact(Skip = "refactor")]
        public async Task TestDeletePlugInSuccessAsync()
        {
            var plugIn = SetupPlugInGetByIdAsync();

            plugIn.PlugInInformation = new PlugInInformation();

            plugIn.Instances.Add(new PlugInInstance()
            {
                Id = 10,
                Name = InstanceName,
                Description = "description"
            });

            var instance = new RuntimeInstance(12,
                                        1,
                                        InstanceName,
                                        AssemblyPath,
                                        RuntimeInstanceState.None);

            instance.SetComplete();

            _mockInstanceHost
                .Setup(_ => _.Instances)
                .Returns([instance]);

            await _plugInService.DeletePlugInAsync(1);

            _mockInstanceHost.Verify(_ => _.RemoveInstance(It.IsAny<int>()), Times.Once);

            _mockFileStoreService.Verify(_ => _.DeletePackage(It.IsAny<string>()), Times.Once);

            MockRepository.Verify(_ => _.DeleteAsync<PlugInInstance>(It.IsAny<int>(),
                                                                     It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TestLoadPlugInInstanceConfigurationOkAsync()
        {
            SetupPlugInInstanceGetByIdAsync();

            _mockPlugConfigurationBuilder.Setup(_ => _.LoadConfiguration(It.IsAny<string>(),
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
            plugIn.PlugInInformation = new PlugInInformation()
            {
                HasConfiguration = false
            };

            SetupPlugInInstanceGetByIdAsync(new PlugInInstance()
            {
                PlugIn = plugIn
            });

            var exception = await Assert.ThrowsAsync<PlugInPackageHasNoConfigurationException>(async () =>
                await _plugInService.LoadPlugInInstanceConfigurationAsync(1));

            Assert.NotNull(exception);
            Assert.Equal(1, exception.Id);
        }

        [Fact(Skip = "Refactor")]
        public async Task TestLoadPlugInInstanceConfigurationPackageNotAssignedAsync()
        {
            var plugIn = SetupPlugInGetByIdAsync();

            SetupPlugInInstanceGetByIdAsync(new PlugInInstance()
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
            await Assert.ThrowsAsync<NotFoundException>(async () =>
                await _plugInService.SetPlugInInstanceEnableAsync(10, true));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TestSetPlugInInstanceEnableSuccessAsync(bool state)
        {
            SetupPlugInInstanceGetByIdAsync(new PlugInInstance()
            {
                Name = "name",
                Description = "description"
            });

            var result = await _plugInService.SetPlugInInstanceEnableAsync(10, state);

            Assert.NotNull(result);
            Assert.Equal(state, result.Enabled);

            VerifySaveAsync();
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

            MockRepository.Verify(_ => _.CreatePlugInInformationAsync(It.IsAny<PlugIn>(),
                                                                      It.IsAny<PlugInInformation>(),
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

            MockRepository.Verify(_ => _.CreatePlugInInformationAsync(It.IsAny<PlugIn>(),
                                                                      It.IsAny<PlugInInformation>(),
                                                                      It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        [Fact(Skip = "Refactor")]
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
                .Returns(new PlugInTypeInformation("name",
                                                   "typename",
                                                   "description",
                                                   "directory",
                                                   true,
                                                   true,
                                                   "assemblyfile",
                                                   "1.0.0"));

            await _plugInService
                .UploadPlugInPackageAsync(1, "filename", stream);

            _mockFileStoreService
                .Verify(_ => _.WritePackageFileStreamAsync(It.IsAny<int>(),
                                                           It.IsAny<string>(),
                                                           It.IsAny<Stream>(),
                                                           It.IsAny<CancellationToken>()),
                    Times.Once);

            MockRepository.Verify(_ => _.CreatePlugInInformationAsync(It.IsAny<PlugIn>(),
                                                                      It.IsAny<PlugInInformation>(),
                                                                      It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TestUploadPlugInPackageUpdateSuccessAsync()
        {
            MemoryStream stream = new MemoryStream();

            var plugIn = SetupPlugInGetByIdAsync();

            plugIn.PlugInInformation = new PlugInInformation()
            {
                AssemblyFileName = "assemblyfile"
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
               .Returns(new PlugInTypeInformation("name",
                                                  "typename",
                                                  "description",
                                                  "directory",
                                                  true,
                                                  true,
                                                  "assemblyfile",
                                                  "1.0.0"));

            await _plugInService
                .UploadPlugInPackageAsync(1, "filename", stream);

            _mockFileStoreService
                .Verify(_ => _.WritePackageFileStreamAsync(It.IsAny<int>(),
                                                           It.IsAny<string>(),
                                                           It.IsAny<Stream>(),
                                                           It.IsAny<CancellationToken>()),
                    Times.Once);

            VerifySaveAsync();
        }
        private PlugIn SetupPlugInGetAsync()
        {
            var plugIn = new PlugIn()
            {
                Name = "plugin",
                Description = "description",
            };

            MockRepository
                .Setup(_ => _.GetEnumerableAsync(It.IsAny<Expression<Func<PlugIn, bool>>?>(),
                                                 It.IsAny<Func<IQueryable<PlugIn>, IOrderedQueryable<PlugIn>>?>(),
                                                 It.IsAny<bool>(),
                                                 It.IsAny<List<string>?>(),
                                                 It.IsAny<CancellationToken>()))
                .Returns(new List<PlugIn>() { plugIn }.ToAsyncEnumerable());

            return plugIn;
        }

        private PlugIn SetupPlugInGetByIdAsync()
        {
            var plugInInformation = new PlugInInformation()
            {
            };

            var plugIn = new PlugIn()
            {
                Id = 1,
                Name = "plugin",
                Description = "description",
                PlugInInformation = plugInInformation
            };

            MockRepository
                .Setup(_ => _.GetByIdAsync<PlugIn>(It.IsAny<int>(),
                                                   It.IsAny<bool>(),
                                                   It.IsAny<List<string>?>(),
                                                   It.IsAny<CancellationToken>()))
                .ReturnsAsync(plugIn);

            return plugIn;
        }

        private RuntimeInstance SetupRunningInstances()
        {
            var instance = new RuntimeInstance(1,
                                        1,
                                        InstanceName,
                                        AssemblyPath,
                                        RuntimeInstanceState.None);

            _mockInstanceHost
               .Setup(_ => _.Instances)
               .Returns([instance]);

            return instance;
        }
    }
}