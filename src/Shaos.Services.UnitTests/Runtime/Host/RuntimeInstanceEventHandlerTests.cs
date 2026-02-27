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
using Shaos.Sdk.Collections.Generic;
using Shaos.Sdk.Devices;
using Shaos.Sdk.Devices.Parameters;
using Shaos.Services.Runtime.Host;
using Xunit;
using ModelBaseParameter = Shaos.Repository.Models.Devices.Parameters.BaseParameter;
using ModelBoolParameter = Shaos.Repository.Models.Devices.Parameters.BoolParameter;
using ModelFloatParameter = Shaos.Repository.Models.Devices.Parameters.FloatParameter;
using ModelIntParameter = Shaos.Repository.Models.Devices.Parameters.IntParameter;
using ModelStringParameter = Shaos.Repository.Models.Devices.Parameters.StringParameter;
using ModelUIntParameter = Shaos.Repository.Models.Devices.Parameters.UIntParameter;

using SdkDevice = Shaos.Sdk.Devices.Device;

namespace Shaos.Services.UnitTests.Runtime.Host
{
    public class RuntimeInstanceEventHandlerTests : BaseServiceTests
    {
        private const string Name = "name";
        private const string Units = "units";

        private readonly List<Mock<IBaseParameter>> _mockBaseParameters;
        private readonly Mock<IChildObservableList<IPlugIn, IDevice>> _mockChildObservableListDevices;
        private readonly Mock<IDevice> _mockDevice;
        private readonly Mock<IChildObservableList<IDevice, IBaseParameter>> _mockObservableListParameters;
        private readonly Mock<IPlugIn> _mockPlugIn;
        private readonly Mock<IRuntimeDeviceUpdateHandler> _mockRuntimeDeviceUpdateHandler;
        private readonly RuntimeInstanceEventHandler _runtimeInstanceEventHandler;

        public RuntimeInstanceEventHandlerTests()
        {
            _mockDevice = new Mock<IDevice>();
            _mockChildObservableListDevices = new Mock<IChildObservableList<IPlugIn, IDevice>>();
            _mockObservableListParameters = new Mock<IChildObservableList<IDevice, IBaseParameter>>();
            _mockPlugIn = new Mock<IPlugIn>();

            _mockRuntimeDeviceUpdateHandler = new Mock<IRuntimeDeviceUpdateHandler>();

            _mockBaseParameters =
            [
                new Mock<IBaseParameter>(),
                new Mock<IBaseParameter>(),
                new Mock<IBaseParameter>(),
                new Mock<IBaseParameter>(),
                new Mock<IBaseParameter>()
            ];

            _runtimeInstanceEventHandler = new RuntimeInstanceEventHandler(LoggerFactory!.CreateLogger<RuntimeInstanceEventHandler>(),
                                                                           _mockRuntimeDeviceUpdateHandler.Object);
        }

        [Fact]
        public void TestAttach()
        {
            SetupCommonMocks();

            _runtimeInstanceEventHandler.Attach(_mockPlugIn.Object);
            _mockChildObservableListDevices.VerifyAdd(_ => _.ListChanged += It.IsAny<AsyncEventHandler<ListChangedEventArgs<IDevice>>>());
            _mockObservableListParameters.VerifyAdd(_ => _.ListChanged += It.IsAny<AsyncEventHandler<ListChangedEventArgs<IBaseParameter>>>());

            _mockBaseParameters[0].As<IBaseParameter<bool>>().VerifyAdd(_ => _.ValueChanged += It.IsAny<AsyncEventHandler<ParameterValueChangedEventArgs<bool>>>());
            _mockBaseParameters[1].As<IBaseParameter<float>>().VerifyAdd(_ => _.ValueChanged += It.IsAny<AsyncEventHandler<ParameterValueChangedEventArgs<float>>>());
            _mockBaseParameters[2].As<IBaseParameter<int>>().VerifyAdd(_ => _.ValueChanged += It.IsAny<AsyncEventHandler<ParameterValueChangedEventArgs<int>>>());
            _mockBaseParameters[3].As<IBaseParameter<string>>().VerifyAdd(_ => _.ValueChanged += It.IsAny<AsyncEventHandler<ParameterValueChangedEventArgs<string>>>());
            _mockBaseParameters[4].As<IBaseParameter<uint>>().VerifyAdd(_ => _.ValueChanged += It.IsAny<AsyncEventHandler<ParameterValueChangedEventArgs<uint>>>());
        }


        [Fact]
        public void TestDetach()
        {
            SetupCommonMocks();

            _runtimeInstanceEventHandler.Detach(_mockPlugIn.Object);
            _mockChildObservableListDevices.VerifyRemove(_ => _.ListChanged -= It.IsAny<AsyncEventHandler<ListChangedEventArgs<IDevice>>>());
            _mockObservableListParameters.VerifyRemove(_ => _.ListChanged -= It.IsAny<AsyncEventHandler<ListChangedEventArgs<IBaseParameter>>>());

            _mockBaseParameters[0].As<IBaseParameter<bool>>().VerifyRemove(_ => _.ValueChanged -= It.IsAny<AsyncEventHandler<ParameterValueChangedEventArgs<bool>>>());
            _mockBaseParameters[1].As<IBaseParameter<float>>().VerifyRemove(_ => _.ValueChanged -= It.IsAny<AsyncEventHandler<ParameterValueChangedEventArgs<float>>>());
            _mockBaseParameters[2].As<IBaseParameter<int>>().VerifyRemove(_ => _.ValueChanged -= It.IsAny<AsyncEventHandler<ParameterValueChangedEventArgs<int>>>());
            _mockBaseParameters[3].As<IBaseParameter<string>>().VerifyRemove(_ => _.ValueChanged -= It.IsAny<AsyncEventHandler<ParameterValueChangedEventArgs<string>>>());
            _mockBaseParameters[4].As<IBaseParameter<uint>>().VerifyRemove(_ => _.ValueChanged -= It.IsAny<AsyncEventHandler<ParameterValueChangedEventArgs<uint>>>());
        }

        [Fact]
        public void TestDevicesListChangedDeviceAdded()
        {
            var plugInInstance = new PlugInInstance();

            SetupCommonMocks();

            MockRepository
               .Setup(_ => _.GetByIdAsync<PlugInInstance>(It.IsAny<int>(),
                                                       It.IsAny<bool>(),
                                                       It.IsAny<List<string>>(),
                                                       It.IsAny<CancellationToken>()))
               .ReturnsAsync(plugInInstance);

            _runtimeInstanceEventHandler
                .AttachDevicesListChange(_mockChildObservableListDevices.Object);

            _mockChildObservableListDevices
                .Raise(_ => _.ListChanged += null,
                       _mockChildObservableListDevices.Object,
                       new ListChangedEventArgs<IDevice>(ListChangedAction.Add,
                       [
                           new SdkDevice(1, Name,
                           [
                               new BoolParameter(1, true, Name, Units, ParameterType.Iaq),
                               new FloatParameter(2, 0.2f, 0, 10, Name, Units, ParameterType.Iaq),
                               new IntParameter(3, -18, -1, 10, Name, Units, ParameterType.Iaq),
                               new StringParameter(4, "string", Name, Units, ParameterType.Iaq),
                               new UIntParameter(6, 7218, 0, 10, Name, Units, ParameterType.Iaq)
                           ])
                       ]));
        }

        [Theory]
        [InlineData(ListChangedAction.Reset)]
        [InlineData(ListChangedAction.Remove)]
        public void TestDevicesListChangedDeviceDelete(ListChangedAction action)
        {
            SetupCommonMocks();

            _runtimeInstanceEventHandler
                .AttachDevicesListChange(_mockChildObservableListDevices.Object);

            _mockChildObservableListDevices
                .Raise(_ => _.ListChanged += null,
                       _mockChildObservableListDevices.Object,
                       new ListChangedEventArgs<IDevice>(action,
                       [
                           new SdkDevice(1, Name, [])
                       ]));

            _mockRuntimeDeviceUpdateHandler.Verify(_ => _.DeleteDevicesAsync(It.IsAny<IEnumerable<int>>()));
        }

        [Fact]
        public void TestParametersListChangedParameterAdded()
        {
            _mockObservableListParameters
                .Setup(_ => _.Parent)
                .Returns(_mockDevice.Object);

            _runtimeInstanceEventHandler
                .AttachParametersListChanged(_mockObservableListParameters.Object);

            _mockObservableListParameters
                .Raise(_ => _.ListChanged += null,
                       _mockObservableListParameters.Object,
                       new ListChangedEventArgs<IBaseParameter>(ListChangedAction.Add,
                       [
                           new BoolParameter(1, true, Name, Units, ParameterType.Iaq),
                           new FloatParameter(2, 1.0f, 0, 10, Name, Units, ParameterType.Iaq),
                           new IntParameter(3, 1, -1, 20, Name, Units, ParameterType.Iaq),
                           new StringParameter(4, "string", Name, Units, ParameterType.Iaq),
                           new UIntParameter(5, 1, 0, 299, Name, Units, ParameterType.Iaq)
                       ]));

            _mockRuntimeDeviceUpdateHandler.Verify(_ => _.CreateDeviceParametersAsync(It.IsAny<int>(), It.IsAny<IList<IBaseParameter>>()));
        }

        [Theory]
        [InlineData(ListChangedAction.Reset)]
        [InlineData(ListChangedAction.Remove)]
        public void TestParametersListChangedParameterDeleted(ListChangedAction action)
        {
            SetupCommonMocks();

            _runtimeInstanceEventHandler
                .AttachParametersListChanged(_mockObservableListParameters.Object);

            _mockObservableListParameters
                .Raise(_ => _.ListChanged += null,
                       _mockObservableListParameters.Object,
                       new ListChangedEventArgs<IBaseParameter>(action,
                       [
                           _mockBaseParameters[0].As<IBaseParameter<bool>>().Object,
                           _mockBaseParameters[1].As<IBaseParameter<float>>().Object,
                           _mockBaseParameters[2].As<IBaseParameter<int>>().Object,
                           _mockBaseParameters[3].As<IBaseParameter<string>>().Object,
                           _mockBaseParameters[4].As<IBaseParameter<uint>>().Object
                       ]));

            _mockRuntimeDeviceUpdateHandler
                .Verify(_ => _
                .DeleteDeviceParametersAsync(It.IsAny<IEnumerable<int>>()));
        }

        [Fact]
        public void TestParameterValueChangedBoolAsync()
        {
            _mockBaseParameters[0]
                .As<IBaseParameter<bool>>().Object.ValueChanged += _runtimeInstanceEventHandler.ParameterValueChangedAsync;

            _mockBaseParameters[0]
                .As<IBaseParameter<bool>>()
                .RaiseAsync(_ => _.ValueChanged += null,
                            _mockBaseParameters[0].As<IBaseParameter<bool>>().Object,
                            new ParameterValueChangedEventArgs<bool>(true));
        }

        [Fact]
        public void TestParameterValueChangedFloatAsync()
        {
            _mockBaseParameters[0]
                .As<IBaseParameter<float>>().Object.ValueChanged += _runtimeInstanceEventHandler.ParameterValueChangedAsync;

            _mockBaseParameters[0]
                .As<IBaseParameter<float>>()
                .RaiseAsync(_ => _.ValueChanged += null,
                            _mockBaseParameters[0].As<IBaseParameter<float>>().Object,
                            new ParameterValueChangedEventArgs<float>(1.0f));
        }

        [Fact]
        public void TestParameterValueChangedIntAsync()
        {
            _mockBaseParameters[0]
                .As<IBaseParameter<int>>().Object.ValueChanged += _runtimeInstanceEventHandler.ParameterValueChangedAsync;

            _mockBaseParameters[0]
                .As<IBaseParameter<int>>()
                .RaiseAsync(_ => _.ValueChanged += null,
                            _mockBaseParameters[0].As<IBaseParameter<int>>().Object,
                            new ParameterValueChangedEventArgs<int>(1));
        }

        [Fact]
        public void TestParameterValueChangedStringAsync()
        {
            _mockBaseParameters[0]
                .As<IBaseParameter<string>>().Object.ValueChanged += _runtimeInstanceEventHandler.ParameterValueChangedAsync;

            _mockBaseParameters[0]
                .As<IBaseParameter<string>>()
                .RaiseAsync(_ => _.ValueChanged += null,
                            _mockBaseParameters[0].As<IBaseParameter<string>>().Object,
                            new ParameterValueChangedEventArgs<string>(""));
        }

        [Fact]
        public void TestParameterValueChangedUIntAsync()
        {
            _mockBaseParameters[0]
                .As<IBaseParameter<uint>>().Object.ValueChanged += _runtimeInstanceEventHandler.ParameterValueChangedAsync;

            _mockBaseParameters[0]
                .As<IBaseParameter<uint>>()
                .RaiseAsync(_ => _.ValueChanged += null,
                            _mockBaseParameters[0].As<IBaseParameter<uint>>().Object,
                            new ParameterValueChangedEventArgs<uint>(10));
        }

        private void SetupCommonMocks()
        {
            _mockPlugIn
                .Setup(_ => _.Id)
                .Returns(10);

            _mockPlugIn
                .Setup(_ => _.Devices)
                .Returns(_mockChildObservableListDevices.Object);

            _mockChildObservableListDevices
                .Setup(_ => _.Parent)
                .Returns(_mockPlugIn.Object);

            _mockChildObservableListDevices
                .Setup(_ => _.GetEnumerator())
                .Returns(new List<IDevice>() { _mockDevice.Object }.GetEnumerator());

            _mockDevice
               .Setup(_ => _.Id)
               .Returns(100);

            _mockDevice
               .Setup(_ => _.Name)
               .Returns("Name");

            _mockDevice
                .Setup(_ => _.Parameters)
                .Returns(_mockObservableListParameters.Object);

            _mockObservableListParameters
                .Setup(_ => _.GetEnumerator())
                .Returns(
                new List<IBaseParameter>()
                {
                    _mockBaseParameters[0].As<IBaseParameter<bool>>().Object,
                    _mockBaseParameters[1].As<IBaseParameter<float>>().Object,
                    _mockBaseParameters[2].As<IBaseParameter<int>>().Object,
                    _mockBaseParameters[3].As<IBaseParameter<string>>().Object,
                    _mockBaseParameters[4].As<IBaseParameter<uint>>().Object
                }.GetEnumerator());

            _mockObservableListParameters.
                Setup(_ => _.Parent)
                .Returns(_mockDevice.Object);
        }
    }
}