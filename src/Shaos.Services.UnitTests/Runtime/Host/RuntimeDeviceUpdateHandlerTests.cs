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
using ModelDevice = Shaos.Repository.Models.Devices.Device;

using SdkBoolParameter = Shaos.Sdk.Devices.Parameters.BoolParameter;
using SdkDevice = Shaos.Sdk.Devices.Device;

namespace Shaos.Services.UnitTests.Runtime.Host
{
    public class RuntimeDeviceUpdateHandlerTests : BaseServiceTests
    {
        private readonly RuntimeDeviceUpdateHandler _runtimeDeviceUpdateHandler;
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<IServiceScope> _mockServiceScope;
        private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
        private readonly Mock<IWorkItemQueue> _mockWorkItemQueue;

        public RuntimeDeviceUpdateHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _mockWorkItemQueue = new Mock<IWorkItemQueue>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockServiceScope = new Mock<IServiceScope>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();

            _runtimeDeviceUpdateHandler = new RuntimeDeviceUpdateHandler(
                LoggerFactory.CreateLogger<RuntimeDeviceUpdateHandler>(),
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

            SdkBoolParameter boolParameter = new(false, "name", "units", ParameterType.Level);

            await _runtimeDeviceUpdateHandler.CreateDeviceParametersAsync(1, [boolParameter]);

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

            MockRepository
                .Setup(_ => _.GetByIdAsync<PlugInInstance>(It.IsAny<int>(),
                                                               It.IsAny<bool>(),
                                                               It.IsAny<List<string>>(),
                                                               It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlugInInstance()
                {
                    Description = "description",
                    Name = "Name"
                });

            var device = new SdkDevice("name", Sdk.Devices.DeviceFeatures.None, []);

            await _runtimeDeviceUpdateHandler.CreateDevicesAsync(1, [device]);

            MockRepository
               .Verify(_ => _.AddAsync(It.IsAny<ModelDevice>(),
                                       It.IsAny<CancellationToken>()));

            MockRepository
                .Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()));
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