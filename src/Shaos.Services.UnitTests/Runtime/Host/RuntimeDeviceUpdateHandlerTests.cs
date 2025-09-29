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
using Shaos.Repository;
using Shaos.Repository.Models;
using Shaos.Sdk.Devices.Parameters;
using Shaos.Services.Processing;
using Shaos.Services.Runtime.Host;
using Xunit;
using Xunit.Abstractions;
using ModelBaseParameter = Shaos.Repository.Models.Devices.Parameters.BaseParameter;
using ModelBoolParameter = Shaos.Repository.Models.Devices.Parameters.BoolParameter;
using ModelDevice = Shaos.Repository.Models.Devices.Device;
using ModelFloatParameter = Shaos.Repository.Models.Devices.Parameters.FloatParameter;
using ModelIntParameter = Shaos.Repository.Models.Devices.Parameters.IntParameter;
using ModelStringParameter = Shaos.Repository.Models.Devices.Parameters.StringParameter;
using ModelUIntParameter = Shaos.Repository.Models.Devices.Parameters.UIntParameter;
using SdkBoolParameter = Shaos.Sdk.Devices.Parameters.BoolParameter;
using SdkDevice = Shaos.Sdk.Devices.Device;

namespace Shaos.Services.UnitTests.Runtime.Host
{
    public class RuntimeDeviceUpdateHandlerTests : BaseServiceTests
    {
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<IServiceScope> _mockServiceScope;
        private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
        private readonly Mock<IWorkItemQueue> _mockWorkItemQueue;
        private readonly RuntimeDeviceUpdateHandler _runtimeDeviceUpdateHandler;

        public RuntimeDeviceUpdateHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _mockWorkItemQueue = new Mock<IWorkItemQueue>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockServiceScope = new Mock<IServiceScope>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();

            _runtimeDeviceUpdateHandler = new RuntimeDeviceUpdateHandler(LoggerFactory.CreateLogger<RuntimeDeviceUpdateHandler>(),
                                                                         _mockServiceScopeFactory.Object,
                                                                         _mockWorkItemQueue.Object);
        }

        [Fact]
        public async Task TestCreateDeviceParametersAsync()
        {
            SetupServiceScopeFactory();

            MockRepository
                .Setup(_ => _.GetByIdAsync<ModelDevice>(It.IsAny<int>(),
                                                        It.IsAny<bool>(),
                                                        It.IsAny<List<string>>(),
                                                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ModelDevice());

            SdkBoolParameter parameter = new(false,
                                             "name",
                                             "units",
                                             ParameterType.Level);

            await _runtimeDeviceUpdateHandler.CreateDeviceParametersAsync(1, [parameter]);

            MockRepository
               .Verify(_ => _.AddAsync(It.IsAny<ModelBaseParameter>(),
                                       It.IsAny<CancellationToken>()));

            MockRepository
                .Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task TestCreateDevicesAsync()
        {
            SetupServiceScopeFactory();

            PlugInInstance plugInInstance = new()
            {
                Description = "description",
                Name = "Name"
            };

            MockRepository
                .Setup(_ => _.GetByIdAsync<PlugInInstance>(It.IsAny<int>(),
                                                           It.IsAny<bool>(),
                                                           It.IsAny<List<string>>(),
                                                           It.IsAny<CancellationToken>()))
                .ReturnsAsync(plugInInstance);

            SdkBoolParameter parameter = new(false,
                                             "name",
                                             "units",
                                             ParameterType.Level);

            var device = new SdkDevice("name",
                                       Sdk.Devices.DeviceFeatures.None,
                                       [parameter]);

            await _runtimeDeviceUpdateHandler.CreateDevicesAsync(1, [device]);

            MockRepository
               .Verify(_ => _.AddAsync(It.IsAny<ModelDevice>(),
                                       It.IsAny<CancellationToken>()));

            MockRepository
                .Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task TestDeleteDeviceParametersAsync()
        {
            SetupServiceScopeFactory();

            await _runtimeDeviceUpdateHandler.DeleteDeviceParametersAsync([1]);

            MockRepository.Verify(_ => _.DeleteAsync<ModelBaseParameter>(It.IsAny<int>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task TestDeleteDevicesAsync()
        {
            SetupServiceScopeFactory();

            await _runtimeDeviceUpdateHandler.DeleteDevicesAsync([1]);

            MockRepository.Verify(_ => _.DeleteAsync<ModelDevice>(It.IsAny<int>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task TestDeviceBatteryLevelUpdateAsync()
        {
            SetupServiceScopeFactory();

            await _runtimeDeviceUpdateHandler.DeviceBatteryLevelUpdateAsync(1, 10, DateTime.UtcNow);

            _mockWorkItemQueue.Verify(_ => _.QueueAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task TestDeviceSignalLevelUpdateAsync()
        {
            SetupServiceScopeFactory();

            await _runtimeDeviceUpdateHandler.DeviceSignalLevelUpdateAsync(1, -10, DateTime.UtcNow);

            _mockWorkItemQueue.Verify(_ => _.QueueAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task TestSaveParameterChangeAsyncBool()
        {
            SetupServiceScopeFactory();

            await _runtimeDeviceUpdateHandler.SaveParameterChangeAsync(1, true, DateTime.UtcNow);

            _mockWorkItemQueue.Verify(_ => _.QueueAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task TestSaveParameterChangeAsyncFloat()
        {
            SetupServiceScopeFactory();

            await _runtimeDeviceUpdateHandler.SaveParameterChangeAsync(1, 5.0f, DateTime.UtcNow);

            _mockWorkItemQueue.Verify(_ => _.QueueAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task TestSaveParameterChangeAsyncInt()
        {
            SetupServiceScopeFactory();

            await _runtimeDeviceUpdateHandler.SaveParameterChangeAsync(1, -10, DateTime.UtcNow);

            _mockWorkItemQueue.Verify(_ => _.QueueAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task TestSaveParameterChangeAsyncInternal()
        {
            SetupServiceScopeFactory();

            CancellationToken cancellationToken = default(CancellationToken);

            ModelFloatParameter parameter = new ModelFloatParameter()
            {
                Name = "name",
            };

            MockRepository.Setup(_ => _.GetByIdAsync<ModelFloatParameter>(It.IsAny<int>(),
                                                                         It.IsAny<bool>(),
                                                                         It.IsAny<List<string>>(),
                                                                         It.IsAny<CancellationToken>()))
                .ReturnsAsync(parameter);

            await _runtimeDeviceUpdateHandler.SaveParameterChangeAsync(1,
                                                                       4.6f,
                                                                       DateTime.UtcNow,
                                                                       cancellationToken);

            Assert.Single(parameter.Values);

            MockRepository.Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task TestSaveParameterChangeAsyncInternalBool()
        {
            SetupServiceScopeFactory();

            CancellationToken cancellationToken = default(CancellationToken);

            ModelBoolParameter parameter = new ModelBoolParameter()
            {
                Name = "name",
            };

            MockRepository.Setup(_ => _.GetByIdAsync<ModelBoolParameter>(It.IsAny<int>(),
                                                                         It.IsAny<bool>(),
                                                                         It.IsAny<List<string>>(),
                                                                         It.IsAny<CancellationToken>()))
                .ReturnsAsync(parameter);

            await _runtimeDeviceUpdateHandler.SaveParameterChangeAsync(1,
                                                                       true,
                                                                       DateTime.UtcNow,
                                                                       cancellationToken);

            Assert.Single(parameter.Values);

            MockRepository.Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task TestSaveParameterChangeAsyncInternalInt()
        {
            SetupServiceScopeFactory();

            CancellationToken cancellationToken = default(CancellationToken);

            ModelIntParameter parameter = new ModelIntParameter()
            {
                Name = "name",
            };

            MockRepository.Setup(_ => _.GetByIdAsync<ModelIntParameter>(It.IsAny<int>(),
                                                                         It.IsAny<bool>(),
                                                                         It.IsAny<List<string>>(),
                                                                         It.IsAny<CancellationToken>()))
                .ReturnsAsync(parameter);

            await _runtimeDeviceUpdateHandler.SaveParameterChangeAsync(1,
                                                                       -10,
                                                                       DateTime.UtcNow,
                                                                       cancellationToken);

            Assert.Single(parameter.Values);

            MockRepository.Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task TestSaveParameterChangeAsyncInternalString()
        {
            SetupServiceScopeFactory();

            CancellationToken cancellationToken = default(CancellationToken);

            ModelStringParameter parameter = new ModelStringParameter()
            {
                Name = "name",
            };

            MockRepository.Setup(_ => _.GetByIdAsync<ModelStringParameter>(It.IsAny<int>(),
                                                                         It.IsAny<bool>(),
                                                                         It.IsAny<List<string>>(),
                                                                         It.IsAny<CancellationToken>()))
                .ReturnsAsync(parameter);

            await _runtimeDeviceUpdateHandler.SaveParameterChangeAsync(1,
                                                                       "value",
                                                                       DateTime.UtcNow,
                                                                       cancellationToken);

            Assert.Single(parameter.Values);

            MockRepository.Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task TestSaveParameterChangeAsyncInternalUInt()
        {
            SetupServiceScopeFactory();

            CancellationToken cancellationToken = default(CancellationToken);

            ModelUIntParameter parameter = new ModelUIntParameter()
            {
                Name = "name",
            };

            MockRepository.Setup(_ => _.GetByIdAsync<ModelUIntParameter>(It.IsAny<int>(),
                                                                         It.IsAny<bool>(),
                                                                         It.IsAny<List<string>>(),
                                                                         It.IsAny<CancellationToken>()))
                .ReturnsAsync(parameter);

            await _runtimeDeviceUpdateHandler.SaveParameterChangeAsync(1,
                                                                       10u,
                                                                       DateTime.UtcNow,
                                                                       cancellationToken);

            Assert.Single(parameter.Values);

            MockRepository.Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task TestSaveParameterChangeAsyncString()
        {
            SetupServiceScopeFactory();

            await _runtimeDeviceUpdateHandler.SaveParameterChangeAsync(1,
                                                                       "name",
                                                                       DateTime.UtcNow);

            _mockWorkItemQueue.Verify(_ => _.QueueAsync(It.IsAny<Func<CancellationToken, Task>>(),
                                                        It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task TestSaveParameterChangeAsyncUInt()
        {
            SetupServiceScopeFactory();

            await _runtimeDeviceUpdateHandler.SaveParameterChangeAsync(1, 10u, DateTime.UtcNow);

            _mockWorkItemQueue.Verify(_ => _.QueueAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task TestUpdateDeviceBatteryLevelAsync()
        {
            SetupServiceScopeFactory();

            ModelDevice modelDevice = new ModelDevice()
            {
                Features = Sdk.Devices.DeviceFeatures.BatteryPowered
            };

            modelDevice.CreateDeviceFeatureParameters();

            MockRepository.Setup(_ => _.GetByIdAsync<ModelDevice>(It.IsAny<int>(),
                                                                  It.IsAny<bool>(),
                                                                  It.IsAny<List<string>>(),
                                                                  It.IsAny<CancellationToken>()))
                .ReturnsAsync(modelDevice);

            await _runtimeDeviceUpdateHandler.UpdateDeviceBatteryLevelAsync(1,
                                                                            10,
                                                                            DateTime.UtcNow);
            
            MockRepository.Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()));

            Assert.NotNull(modelDevice.BatteryLevel);
            Assert.Equal(10u, modelDevice.BatteryLevel!.Value);
        }

        [Fact]
        public async Task TestUpdateDeviceSignalLevelAsync()
        {
            SetupServiceScopeFactory();

            ModelDevice modelDevice = new ModelDevice()
            {
                Features = Sdk.Devices.DeviceFeatures.Wireless
            };

            modelDevice.CreateDeviceFeatureParameters();

            MockRepository.Setup(_ => _.GetByIdAsync<ModelDevice>(It.IsAny<int>(),
                                                                  It.IsAny<bool>(),
                                                                  It.IsAny<List<string>>(),
                                                                  It.IsAny<CancellationToken>()))
                .ReturnsAsync(modelDevice);

            await _runtimeDeviceUpdateHandler.UpdateDeviceSignalLevelAsync(1,
                                                                           -10,
                                                                           DateTime.UtcNow);

            MockRepository.Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()));

            Assert.NotNull(modelDevice.SignalLevel);
            Assert.Equal(-10, modelDevice.SignalLevel!.Value);
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
                .Setup(_ => _.GetService(typeof(IRepository)))
                .Returns(MockRepository.Object);
        }
    }
}