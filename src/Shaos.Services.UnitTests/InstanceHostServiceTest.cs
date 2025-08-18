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
using Shaos.Repository.Models;
using Shaos.Repository.Models.Devices.Parameters;
using Shaos.Sdk;
using Shaos.Services.Exceptions;
using Shaos.Services.IO;
using Shaos.Services.Runtime.Exceptions;
using Shaos.Services.Runtime.Host;
using Shaos.Test.PlugIn;
using Shaos.Testing.Shared.Extensions;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests
{
    public class InstanceHostServiceTest : BaseServiceTests
    {
        private const string AssemblyPath = "AssemblyPath";
        private const string configurationValue = "configuration";
        private const string InstanceName = "Test";
        private readonly InstanceHostService _instanceHostService;
        private readonly Mock<IFileStoreService> _mockFileStoreService;
        private readonly Mock<IRuntimeInstanceEventHandler> _mockInstanceEventHandler;
        private readonly Mock<IRuntimeInstanceHost> _mockInstanceHost;
        private readonly Mock<IPlugInBuilder> _mockPlugInBuilder;
        private readonly Mock<IPlugInConfigurationBuilder> _mockPlugInConfigurationBuilder;
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<IServiceScope> _mockServiceScope;
        private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;

        public InstanceHostServiceTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            _mockFileStoreService = new Mock<IFileStoreService>();
            _mockInstanceEventHandler = new Mock<IRuntimeInstanceEventHandler>();
            _mockInstanceHost = new Mock<IRuntimeInstanceHost>();
            _mockPlugInBuilder = new Mock<IPlugInBuilder>();
            _mockPlugInConfigurationBuilder = new Mock<IPlugInConfigurationBuilder>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockServiceScope = new Mock<IServiceScope>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();

            _instanceHostService = new InstanceHostService(LoggerFactory!.CreateLogger<InstanceHostService>(),
                                                           _mockInstanceHost.Object,
                                                           MockRepository.Object,
                                                           _mockFileStoreService.Object,
                                                           _mockServiceScopeFactory.Object,
                                                           _mockInstanceEventHandler.Object,
                                                           _mockPlugInConfigurationBuilder.Object);
        }

        [Fact]
        public async Task TestLoadInstanceConfigurationPlugInInstanceNotFoundExceptionAsync()
        {
            var exception = await Assert.ThrowsAsync<PlugInInstanceNotFoundException>(
                async () => await _instanceHostService.LoadInstanceConfigurationAsync(1));

            Assert.NotNull(exception);
            Assert.Equal(1, exception.Id);
        }

        [Fact]
        public async Task TestLoadInstanceConfigurationWithConfigurationAsync()
        {
            SetupPlugInInstanceGetByIdAsync();

            SetupInstanceLoadContext();

            SetupConfigurationLoad();

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

            SetupPlugInInstanceGetByIdAsync(new PlugInInstance()
            {
                PlugIn = plugIn
            });

            var exception = await Assert.ThrowsAsync<PlugInInstanceNotConfiguredException>(
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

            SetupPlugInInstanceGetByIdAsync(new PlugInInstance()
            {
                PlugIn = plugIn
            });

            SetupInstanceLoadContext();

            SetupServiceScopeFactory();

            await _instanceHostService.StartInstanceAsync(1);

            _mockInstanceHost
                .Verify(_ => _.InstanceExists(1), Times.Once);

            VerifyGetByIdAsync();

            _mockInstanceHost.Verify(_ => _.StartInstance(It.IsAny<int>(),
                                                          It.IsAny<IPlugIn>()), Times.Once);
        }

        [Fact]
        public async Task TestStartInstanceNoPackageAsync()
        {
            var plugIn = new PlugIn()
            {
                Description = "Test",
                Name = "Test"
            };

            _mockInstanceHost
                .Setup(_ => _.InstanceExists(It.IsAny<int>()))
                .Returns(true);

            SetupPlugInInstanceGetByIdAsync(new PlugInInstance()
            {
                PlugIn = plugIn
            });

            var exception = await Assert.ThrowsAsync<PlugInPackageNotAssignedException>(
                async () => await _instanceHostService.StartInstanceAsync(1));

            Assert.NotNull(exception);
            Assert.Equal(1, exception.Id);
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
                Name = InstanceName,
                Package = new Package()
                {
                    AssemblyFile = "AssemblyFile",
                    HasConfiguration = true
                }
            };

            var plugInInstance = new PlugInInstance()
            {
                Id = 1,
                Enabled = true,
                Configuration = configurationValue
            };

            await TestStartInstance(plugIn, plugInInstance);

            _mockInstanceHost
                .Verify(_ => _.StartInstance(It.IsAny<int>(),
                                             It.IsAny<IPlugIn>()), Times.Once);
        }

        [Fact]
        public async Task TestStartInstancesNotConfiguredAsync()
        {
            var plugIn = new PlugIn()
            {
                Id = 1,
                Description = "description",
                Name = InstanceName,
                Package = new Package()
                {
                    AssemblyFile = "AssemblyFile",
                    HasConfiguration = true
                }
            };

            var plugInInstance = new PlugInInstance()
            {
                Id = 1,
                Enabled = true
            };

            await TestStartInstance(plugIn, plugInInstance);

            _mockInstanceHost
                .Verify(_ => _.StartInstance(It.IsAny<int>(),
                                             It.IsAny<IPlugIn>()), Times.Never);
        }

        [Fact]
        public async Task TestStartInstancesNotEnabledAsync()
        {
            var plugIn = new PlugIn()
            {
                Id = 1,
                Description = "description",
                Name = InstanceName,
                Package = new Package()
                {
                    AssemblyFile = "AssemblyFile",
                    HasConfiguration = true
                }
            };

            var plugInInstance = new PlugInInstance()
            {
                Id = 1,
                Configuration = configurationValue
            };

            await TestStartInstance(plugIn, plugInInstance);

            _mockInstanceHost
                .Verify(_ => _.StartInstance(It.IsAny<int>(),
                                             It.IsAny<IPlugIn>()), Times.Never);
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

            SetupPlugInInstanceGetByIdAsync(new PlugInInstance()
            {
                PlugIn = plugIn
            });

            _mockInstanceHost
                .Setup(_ => _.InstanceExists(It.IsAny<int>()))
                .Returns(true);

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

            SetupPlugInInstanceGetByIdAsync((PlugInInstance)null!);

            var exception = await Assert.ThrowsAsync<PlugInInstanceNotFoundException>(
                async () => await _instanceHostService.StartInstanceAsync(1));

            Assert.NotNull(exception);
            Assert.Equal(1, exception.Id);
        }

        [Fact]
        public void TestStopInstance()
        {
            var mockPlugin = new Mock<IPlugIn>();

            var instance = new RuntimeInstance(10, 1, "name", "assembly");

            instance.LoadContext(mockPlugin.Object);

            _mockInstanceHost
                .Setup(_ => _.Instances)
                .Returns([instance]);

            _instanceHostService.StopInstance(10);

            _mockInstanceEventHandler.Verify(_ => _.Detach(It.IsAny<IPlugIn>()));
            _mockInstanceHost
                .Verify(_ => _.StopInstance(It.IsAny<int>()));
        }

        [Fact]
        public async Task TestUpdateInstanceConfigurationAsync()
        {
            List<KeyValuePair<string, string>> collection =
            [
                new("TestSetting","10")
            ];

            SetupPlugInInstanceGetByIdAsync();

            SetupInstanceLoadContext();

            SetupConfigurationLoad();

            await _instanceHostService.UpdateInstanceConfigurationAsync(1, collection);

            MockRepository.Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task TestUpdateInstanceConfigurationNoPlugInInstanceAsync()
        {
            List<KeyValuePair<string, string>> collection =
            [
                new("TestSetting","10")
            ];

            var exception = await Assert.ThrowsAsync<PlugInInstanceNotFoundException>(
                async () => await _instanceHostService.UpdateInstanceConfigurationAsync(1, collection));

            Assert.NotNull(exception);
            Assert.Equal(1, exception.Id);
        }

        private void SetupConfigurationLoad()
        {
            _mockPlugInConfigurationBuilder.Setup(_ => _.LoadConfiguration(It.IsAny<Assembly>(),
                                                                           It.IsAny<string?>()))
                .Returns(new TestConfig());
        }

        private void SetupInstanceLoadContext()
        {
            var instanceLoadContext = new RuntimeInstanceLoadContext(typeof(TestPlugIn).Assembly);

            _mockInstanceHost.Setup(_ => _.GetInstanceLoadContext(It.IsAny<int>()))
                            .Returns(instanceLoadContext);
        }

        private void SetupServiceScopeFactory()
        {
            _mockServiceScopeFactory
                .Setup(_ => _.CreateScope())
                .Returns(_mockServiceScope.Object);

            _mockServiceScope
                .Setup(_ => _.ServiceProvider)
                .Returns(_mockServiceProvider.Object);

            _mockServiceProvider
                .Setup(_ => _.GetService(typeof(IPlugInBuilder)))
                .Returns(_mockPlugInBuilder.Object);
        }

        private async Task TestStartInstance(PlugIn plugIn, PlugInInstance plugInInstance)
        {
            plugIn.Instances.Add(plugInInstance);

            plugIn.Instances[0].Devices.Add(new Repository.Models.Devices.Device()
            {
                Id = 1
            });

            plugIn.Instances[0].Devices[0].Parameters.Add(new BoolParameter());

            List<PlugIn> plugIns =
            [
                plugIn
            ];

            var instance = new RuntimeInstance(1,
                                        2,
                                        InstanceName,
                                        AssemblyPath,
                                        true);

            MockRepository.Setup(_ => _.GetEnumerableAsync<PlugIn>(It.IsAny<Expression<Func<PlugIn, bool>>?>(),
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
                                                          It.IsAny<bool>()))
                .Returns(instance);

            SetupInstanceLoadContext();

            SetupServiceScopeFactory();

            await _instanceHostService.StartInstancesAsync();

            MockRepository.Verify(_ => _.GetEnumerableAsync<PlugIn>(It.IsAny<Expression<Func<PlugIn, bool>>?>(),
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
                                              It.IsAny<bool>()),
                                              Times.Once);
        }

        public class TestConfig
        {
            public int TestSetting { get; set; }
        }
    }
}