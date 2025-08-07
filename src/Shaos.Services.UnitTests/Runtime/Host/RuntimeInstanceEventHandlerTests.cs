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
using Shaos.Sdk;
using Shaos.Sdk.Collections.Generic;
using Shaos.Sdk.Devices;
using Shaos.Sdk.Devices.Parameters;
using Shaos.Services.Runtime.Host;
using Xunit;
using Xunit.Abstractions;

using ModelDevice = Shaos.Repository.Models.Devices.Device;

namespace Shaos.Services.UnitTests.Runtime.Host
{
    public class RuntimeInstanceEventHandlerTests : BaseServiceTests
    {
        private readonly List<IBaseParameter> _baseParameters;
        private readonly Mock<IDevice> _mockDevice;
        private readonly Mock<IObservableList<IDevice>> _mockObservableListDevices;
        private readonly Mock<IChildObservableList<IBaseParameter, IDevice>> _mockObservableListParameters;
        private readonly Mock<IPlugIn> _mockPlugIn;
        private readonly RuntimeInstanceEventHandler _runtimeInstanceEventHandler;

        public RuntimeInstanceEventHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _mockDevice = new Mock<IDevice>();
            _mockObservableListDevices = new Mock<IObservableList<IDevice>>();
            _mockObservableListParameters = new Mock<IChildObservableList<IBaseParameter, IDevice>>();
            _mockPlugIn = new Mock<IPlugIn>();

            _baseParameters = new List<IBaseParameter>()
            {
                new BoolParameter(false, "name", "units", ParameterType.Iaq),
                new FloatParameter(1.0f, "name", "units", ParameterType.Iaq),
                new IntParameter(1, "name", "units", ParameterType.Iaq),
                new StringParameter("value", "name", "units", ParameterType.Iaq),
                new UIntParameter(2, "name", "units", ParameterType.Iaq)
            };

            _runtimeInstanceEventHandler = new RuntimeInstanceEventHandler(LoggerFactory.CreateLogger<RuntimeInstanceEventHandler>(),
                                                                           MockRepository.Object);
        }

        [Fact]
        public void TestAttach()
        {
            SetupCommonMocks();

            _runtimeInstanceEventHandler.Attach(_mockPlugIn.Object);

            _mockDevice.VerifyAdd(_ => _.DeviceChanged += It.IsAny<AsyncEventHandler<DeviceChangedEventArgs>>());
            _mockObservableListDevices.VerifyAdd(_ => _.ListChanged += It.IsAny<AsyncEventHandler<ListChangedEventArgs<IDevice>>>());
            _mockObservableListParameters.VerifyAdd(_ => _.ListChanged += It.IsAny<AsyncEventHandler<ListChangedEventArgs<IBaseParameter>>>());
        }

        [Fact]
        public void TestDetach()
        {
            SetupCommonMocks();

            _runtimeInstanceEventHandler.Detach(_mockPlugIn.Object);

            _mockDevice.VerifyRemove(_ => _.DeviceChanged -= It.IsAny<AsyncEventHandler<DeviceChangedEventArgs>>());
            _mockObservableListDevices.VerifyRemove(_ => _.ListChanged -= It.IsAny<AsyncEventHandler<ListChangedEventArgs<IDevice>>>());
            _mockObservableListParameters.VerifyRemove(_ => _.ListChanged -= It.IsAny<AsyncEventHandler<ListChangedEventArgs<IBaseParameter>>>());
        }

        [Fact]
        public void TestDevicesListChangedDeviceAdded()
        {
            _runtimeInstanceEventHandler
                .AttachDevicesListChange(_mockObservableListDevices.Object);

            _mockObservableListDevices
                .Raise(_ => _.ListChanged += null,
                       _mockObservableListDevices.Object,
                       new ListChangedEventArgs<IDevice>(ListChangedAction.Add, [new Device("name", [], 0, 0)]));

            MockRepository
                .Verify(_ => _.AddAsync(It.IsAny<ModelDevice>(),
                                        It.IsAny<CancellationToken>()));

            MockRepository
                .Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData(ListChangedAction.Reset)]
        [InlineData(ListChangedAction.Remove)]
        public void TestDevicesListChangedDeviceDelete(ListChangedAction action)
        {
            _runtimeInstanceEventHandler
                .AttachDevicesListChange(_mockObservableListDevices.Object);

            _mockObservableListDevices
                .Raise(_ => _.ListChanged += null,
                       _mockObservableListDevices.Object,
                       new ListChangedEventArgs<IDevice>(action, [new Device("name", [], 0, 0)]));

            MockRepository
                .Verify(_ => _.DeleteAsync<ModelDevice>(It.IsAny<int>(),
                                                        It.IsAny<CancellationToken>()));

            MockRepository
                .Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        private void SetupCommonMocks()
        {
            _mockObservableListParameters
                .Setup(_ => _.GetEnumerator())
                .Returns(_baseParameters.GetEnumerator());

            _mockDevice
                .Setup(_ => _.Parameters)
                .Returns(_mockObservableListParameters.Object);

            _mockPlugIn
                .Setup(_ => _.Devices)
                .Returns(_mockObservableListDevices.Object);

            _mockObservableListDevices
                .Setup(_ => _.GetEnumerator())
                .Returns(new List<IDevice>() { _mockDevice.Object }.GetEnumerator());
        }

        [Fact]
        public void TestParametersListChangedParameterAdded()
        {
            _runtimeInstanceEventHandler
                .AttachDevicesListChange(_mockObservableListDevices.Object);

            _mockObservableListDevices
                .Raise(_ => _.ListChanged += null,
                       _mockObservableListDevices.Object,
                       new ListChangedEventArgs<IDevice>(ListChangedAction.Add, [new Device("name", [], 0, 0)]));

            MockRepository
                .Verify(_ => _.AddAsync(It.IsAny<ModelDevice>(),
                                        It.IsAny<CancellationToken>()));

            MockRepository
                .Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}