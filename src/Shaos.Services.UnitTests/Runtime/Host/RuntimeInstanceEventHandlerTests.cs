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
using Shaos.Sdk;
using Shaos.Sdk.Collections.Generic;
using Shaos.Sdk.Devices;
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
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<IServiceScope> _mockServiceScope;
        private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
        private readonly RuntimeInstanceEventHandler _runtimeInstanceEventHandler;

        private readonly Mock<IRuntimeDeviceUpdateHandler> _mockRuntimeDeviceUpdateHandler;

        public RuntimeInstanceEventHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _mockDevice = new Mock<IDevice>();
            _mockChildObservableListDevices = new Mock<IChildObservableList<IPlugIn, IDevice>>();
            _mockObservableListParameters = new Mock<IChildObservableList<IDevice, IBaseParameter>>();
            _mockPlugIn = new Mock<IPlugIn>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockServiceScope = new Mock<IServiceScope>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();

            _mockRuntimeDeviceUpdateHandler = new Mock<IRuntimeDeviceUpdateHandler>();

            _mockBaseParameters =
            [
                new Mock<IBaseParameter>(),
                new Mock<IBaseParameter>(),
                new Mock<IBaseParameter>(),
                new Mock<IBaseParameter>(),
                new Mock<IBaseParameter>()
            ];

            _runtimeInstanceEventHandler = new RuntimeInstanceEventHandler(LoggerFactory.CreateLogger<RuntimeInstanceEventHandler>(),
                                                                           _mockRuntimeDeviceUpdateHandler.Object);
        }

        [Fact]
        public void TestAttach()
        {
            SetupCommonMocks();

            _runtimeInstanceEventHandler.Attach(_mockPlugIn.Object);

            _mockDevice.VerifyAdd(_ => _.BatteryLevelChanged += It.IsAny<AsyncEventHandler<BatteryLevelChangedEventArgs>>());
            _mockDevice.VerifyAdd(_ => _.SignalLevelChanged += It.IsAny<AsyncEventHandler<SignalLevelChangedEventArgs>>());
            _mockChildObservableListDevices.VerifyAdd(_ => _.ListChanged += It.IsAny<AsyncEventHandler<ListChangedEventArgs<IDevice>>>());
            _mockObservableListParameters.VerifyAdd(_ => _.ListChanged += It.IsAny<AsyncEventHandler<ListChangedEventArgs<IBaseParameter>>>());

            _mockBaseParameters[0].As<IBaseParameter<bool>>().VerifyAdd(_ => _.ValueChanged += It.IsAny<AsyncEventHandler<ParameterValueChangedEventArgs<bool>>>());
            _mockBaseParameters[1].As<IBaseParameter<float>>().VerifyAdd(_ => _.ValueChanged += It.IsAny<AsyncEventHandler<ParameterValueChangedEventArgs<float>>>());
            _mockBaseParameters[2].As<IBaseParameter<int>>().VerifyAdd(_ => _.ValueChanged += It.IsAny<AsyncEventHandler<ParameterValueChangedEventArgs<int>>>());
            _mockBaseParameters[3].As<IBaseParameter<string>>().VerifyAdd(_ => _.ValueChanged += It.IsAny<AsyncEventHandler<ParameterValueChangedEventArgs<string>>>());
            _mockBaseParameters[4].As<IBaseParameter<uint>>().VerifyAdd(_ => _.ValueChanged += It.IsAny<AsyncEventHandler<ParameterValueChangedEventArgs<uint>>>());
        }

        [Fact]
        public void TestBatteryLevelChanged()
        {
            _runtimeInstanceEventHandler.AttachDevice(_mockDevice.Object);

            _mockDevice.Raise(_ => _.BatteryLevelChanged += null, _mockDevice.Object, new BatteryLevelChangedEventArgs()
            {
                BatteryLevel = 1
            });

            _mockRuntimeDeviceUpdateHandler.Verify(_ => _.DeviceBatteryLevelUpdateAsync(It.IsAny<IDevice>(),
                                                                                        It.IsAny<BatteryLevelChangedEventArgs>()));
        }

        [Fact]
        public void TestDetach()
        {
            SetupCommonMocks();

            _runtimeInstanceEventHandler.Detach(_mockPlugIn.Object);

            _mockDevice.VerifyRemove(_ => _.BatteryLevelChanged -= It.IsAny<AsyncEventHandler<BatteryLevelChangedEventArgs>>());
            _mockDevice.VerifyRemove(_ => _.SignalLevelChanged -= It.IsAny<AsyncEventHandler<SignalLevelChangedEventArgs>>());
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
                           new SdkDevice(Name,
                           DeviceFeatures.BatteryPowered | DeviceFeatures.Wireless,
                           [
                               new BoolParameter(true, Name, Units, ParameterType.Iaq),
                               new FloatParameter(0.2f, 0, 10, Name, Units, ParameterType.Iaq),
                               new IntParameter(-18, -1, 10, Name, Units, ParameterType.Iaq),
                               new StringParameter("string", Name, Units, ParameterType.Iaq),
                               new UIntParameter(7218, 0, 10, Name, Units, ParameterType.Iaq)
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
                           new SdkDevice(Name, DeviceFeatures.BatteryPowered | DeviceFeatures.Wireless, [])
                       ]));

            MockRepository
                .Verify(_ => _.DeleteAsync<ModelDevice>(It.IsAny<int>(),
                                                        It.IsAny<CancellationToken>()));

            MockRepository
                .Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()));
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
                           new BoolParameter(true, Name, Units, ParameterType.Iaq),
                           new FloatParameter(1.0f, 0, 10, Name, Units, ParameterType.Iaq),
                           new IntParameter(1, -1, 20, Name, Units, ParameterType.Iaq),
                           new StringParameter("string", Name, Units, ParameterType.Iaq),
                           new UIntParameter(1, 0, 299, Name, Units, ParameterType.Iaq)
                       ]));

            MockRepository
                .Verify(_ => _.AddAsync(It.IsAny<ModelBaseParameter>(),
                                                  It.IsAny<CancellationToken>()), Times.Exactly(5));
            MockRepository
                .Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()));
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

            MockRepository
                .Verify(_ => _.DeleteAsync<ModelBaseParameter>(It.IsAny<int>(),
                                                               It.IsAny<CancellationToken>()), Times.Exactly(5));
            MockRepository
                .Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()));
        }

        [Fact]
        public void TestParameterValueChangedBoolAsync()
        {
            MockRepository
                .Setup(_ => _.GetByIdAsync<ModelBoolParameter>(It.IsAny<int>(),
                                                               It.IsAny<bool>(),
                                                               It.IsAny<List<string>>(),
                                                               It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ModelBoolParameter() { Name = "name" });

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
            MockRepository
                .Setup(_ => _.GetByIdAsync<ModelFloatParameter>(It.IsAny<int>(),
                                                                It.IsAny<bool>(),
                                                                It.IsAny<List<string>>(),
                                                                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ModelFloatParameter() { Name = "name" });

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
            MockRepository
                .Setup(_ => _.GetByIdAsync<ModelIntParameter>(It.IsAny<int>(),
                                                              It.IsAny<bool>(),
                                                              It.IsAny<List<string>>(),
                                                              It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ModelIntParameter() { Name = "name" });

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
            MockRepository
                .Setup(_ => _.GetByIdAsync<ModelStringParameter>(It.IsAny<int>(),
                                                                 It.IsAny<bool>(),
                                                                 It.IsAny<List<string>>(),
                                                                 It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ModelStringParameter() { Name = "name" });

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
            MockRepository
                .Setup(_ => _.GetByIdAsync<ModelUIntParameter>(It.IsAny<int>(),
                                                                 It.IsAny<bool>(),
                                                                 It.IsAny<List<string>>(),
                                                                 It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ModelUIntParameter() { Name = "name" });

            _mockBaseParameters[0]
                .As<IBaseParameter<uint>>().Object.ValueChanged += _runtimeInstanceEventHandler.ParameterValueChangedAsync;

            _mockBaseParameters[0]
                .As<IBaseParameter<uint>>()
                .RaiseAsync(_ => _.ValueChanged += null,
                            _mockBaseParameters[0].As<IBaseParameter<uint>>().Object,
                            new ParameterValueChangedEventArgs<uint>(10));
        }

        [Fact]
        public void TestSignalLevelChanged()
        {
            _runtimeInstanceEventHandler.AttachDevice(_mockDevice.Object);

            _mockDevice.Raise(_ => _.SignalLevelChanged += null, _mockDevice.Object, new SignalLevelChangedEventArgs()
            {
                SignalLevel = -1
            });

            _mockRuntimeDeviceUpdateHandler.Verify(_ => _.DeviceSignalLevelUpdateAsync(It.IsAny<IDevice>(),
                                                                                       It.IsAny<SignalLevelChangedEventArgs>()));
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