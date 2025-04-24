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
using Shaos.Services.Runtime.Host;
using Shaos.Services.Runtime.Validation;
using Shaos.Test.PlugIn;
using Shaos.Testing.Shared;
using Shaos.Testing.Shared.Extensions;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests
{
    public class PlugInServiceTests : BaseTests
    {
        private readonly Mock<IFileStoreService> _mockFileStoreService;
        private readonly Mock<IInstanceHost> _mockInstanceHost;
        private readonly Mock<IPlugInInstanceRepository> _mockPlugInInstanceRepository;
        private readonly Mock<IPlugInRepository> _mockPlugInRepository;
        private readonly Mock<IPlugInTypeValidator> _mockPlugInTypeValidator;
        private readonly PlugInService _plugInService;

        public PlugInServiceTests(ITestOutputHelper output) : base(output)
        {
            _mockFileStoreService = new Mock<IFileStoreService>();
            _mockInstanceHost = new Mock<IInstanceHost>();
            _mockPlugInInstanceRepository = new Mock<IPlugInInstanceRepository>();
            _mockPlugInRepository = new Mock<IPlugInRepository>();
            _mockPlugInTypeValidator = new Mock<IPlugInTypeValidator>();

            _plugInService = new PlugInService(
                LoggerFactory!.CreateLogger<PlugInService>(),
                _mockInstanceHost.Object,
                _mockFileStoreService.Object,
                _mockPlugInRepository.Object,
                _mockPlugInTypeValidator.Object,
                _mockPlugInInstanceRepository.Object);
        }

        [Fact]
        public async Task TestCreatePlugInInstanceDuplicateNameAsync()
        {
            SetupPlugInGetByIdAsync();

            _mockPlugInInstanceRepository.Setup(_ => _.CreateAsync(
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
            var plugIn = SetupPlugInGetByIdAsync();

            plugIn.Package = new Package()
            {
                AssemblyFile = "assemblyfile"
            };

            _mockPlugInInstanceRepository.Setup(_ => _.CreateAsync(
                It.IsAny<PlugIn>(),
                It.IsAny<PlugInInstance>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(10);

            _mockFileStoreService
                .Setup(_ => _.GetAssemblyPath(
                    It.IsAny<int>()
                ))
                .Returns("");

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
            _mockInstanceHost
                .Setup(_ => _.Instances)
                .Returns(new List<Instance>(){
                    new Instance()
                    {
                        Id = 12,
                        State = InstanceState.Running
                    }});

            await Assert.ThrowsAsync<PlugInInstanceRunningException>(async () =>
               await _plugInService.DeletePlugInInstanceAsync(12));
        }

        [Fact]
        public async Task TestDeletePlugInInstanceSuccessAsync()
        {
            _mockInstanceHost
                .Setup(_ => _.Instances)
                .Returns(new List<Instance>(){
                    new Instance()
                    {
                        Id = 12,
                        State = InstanceState.Complete
                    }});

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

            _mockInstanceHost
                .Setup(_ => _.Instances)
                .Returns(new List<Instance>(){
                    new Instance()
                    {
                        Id = 10,
                        State = InstanceState.Running
                    }});

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

            _mockInstanceHost
                .Setup(_ => _.Instances)
                .Returns(new List<Instance>(){
                    new Instance()
                    {
                        Id = 10,
                        State = InstanceState.Complete
                    }});

            await _plugInService.DeletePlugInAsync(1);

            _mockInstanceHost.Verify(_ => _.RemoveInstance(
                It.IsAny<int>()),
                Times.Once);

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
        public async Task TestStartEnabledInstancesAsync()
        {
            var plugIn = SetupPlugInGetAsync();
            plugIn.Package = new Package()
            {
                AssemblyFile = "assemblyfilename",
                FileName = "filename"
            };

            plugIn.Instances.Add(new PlugInInstance()
            {
                Name = "name",
                Enabled = true,
                Description = "description"
            });

            _mockFileStoreService
                .Setup(_ => _.GetAssemblyPath(
                    It.IsAny<int>()
                ))
                .Returns("");

            await _plugInService.StartEnabledInstancesAsync();

            _mockInstanceHost.Verify(_ => _.AddInstance(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Once);

            _mockInstanceHost.Verify(_ => _.StartInstance(
                It.IsAny<int>()),
                Times.Once);
        }

        [Fact]
        public async Task TestUploadPlugInPackageNoValidPlugInAsync()
        {
            MemoryStream stream = new MemoryStream();

            SetupPlugInGetByIdAsync();

            _mockFileStoreService
                .Setup(_ => _.ExtractPackage(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(new List<string>()
                {
                    "file.dll"
                });

            var exception = await Assert
                .ThrowsAsync<NoValidPlugInAssemblyFoundException>(async () => await _plugInService
                    .UploadPlugInPackageAsync(1, "filename", stream));

            Assert.Equal(
                "No assembly file ending with [.PlugIn.dll] was found in the package [filename] files",
                exception.Message);

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

            var instances = new List<Instance>()
            {
                new Instance()
                {
                    Id=1,
                    State = InstanceState.Running,
                    Name = "Test"
                }
            };

            _mockInstanceHost
                .Setup(_ => _.Instances)
                .Returns(instances);

            var exception = await Assert
                .ThrowsAsync<PlugInInstanceRunningException>(async () => await _plugInService
                    .UploadPlugInPackageAsync(1, "filename", stream));

            Assert.Equal(
                "Instance Running",
                exception.Message);

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
                .Setup(_ => _.ExtractPackage(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(new List<string>()
                {
                    ".PlugIn.dll"
                });

            await _plugInService
                .UploadPlugInPackageAsync(1, "filename", stream);

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

        [Fact]
        public async Task TestUploadPlugInPackageUpdateSuccessAsync()
        {
            MemoryStream stream = new MemoryStream();

            var plugIn = SetupPlugInGetByIdAsync();

            plugIn.Package = new Package()
            {
                AssemblyFile = "assemblyfile"
            };

            _mockFileStoreService
                .Setup(_ => _.ExtractPackage(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(new List<string>()
                {
                    ".PlugIn.dll"
                });

            await _plugInService
                .UploadPlugInPackageAsync(1, "filename", stream);

            _mockFileStoreService
                .Verify(_ => _.WritePackageFileStreamAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<Stream>(),
                    It.IsAny<CancellationToken>()),
                    Times.Once);

            _mockPlugInRepository.Verify(_ => _.SaveChangesAsync(
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

            _mockPlugInRepository
                .Setup(_ => _.GetAsync(
                    It.IsAny<Expression<Func<PlugIn, bool>>?>(),
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