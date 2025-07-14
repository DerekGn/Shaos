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
using Shaos.Repository.Models;
using Shaos.Sdk.Devices.Parameters;
using Shaos.Testing.Shared;
using Shaos.Testing.Shared.Extensions;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;

using ModelDevice = Shaos.Repository.Models.Devices.Device;
using SdkDevice = Shaos.Sdk.Devices.Device;

using ModelBoolParameter = Shaos.Repository.Models.Devices.Parameters.BoolParameter;
using ModelFloatParameter = Shaos.Repository.Models.Devices.Parameters.FloatParameter;
using ModelIntParameter = Shaos.Repository.Models.Devices.Parameters.IntParameter;
using ModelStringParameter = Shaos.Repository.Models.Devices.Parameters.StringParameter;
using ModelUIntParameter = Shaos.Repository.Models.Devices.Parameters.UIntParameter;
using SdkFloatParameter = Shaos.Sdk.Devices.Parameters.FloatParameter;
using Shaos.Sdk.Exceptions;

namespace Shaos.Services.UnitTests
{
    public class HostContextTests : BaseTests
    {
        private readonly HostContext _hostContext;
        private readonly Mock<IShaosRepository> _mockRepository;

        public HostContextTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            _mockRepository = new Mock<IShaosRepository>();
            _hostContext = new HostContext(LoggerFactory!.CreateLogger<HostContext>(), _mockRepository.Object, 10);
        }

        [Fact]
        public async Task TestCreateDeviceParameterAsync()
        {
            _mockRepository
                .Setup(_ => _.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<ModelDevice, bool>>?>(),
                                                     It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ModelDevice());

            var result = await _hostContext.CreateDeviceParameterAsync(1, new SdkFloatParameter(1,
                                                                                                3.0f,
                                                                                                "name",
                                                                                                "units",
                                                                                                ParameterType.Current));

            Assert.NotNull(result);
            Assert.NotNull(result.Parameters);
            Assert.Single(result.Parameters);
        }

        [Fact]
        public async Task TestCreateDeviceAsync()
        {
            _mockRepository
                .Setup(_ => _.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<PlugInInstance, bool>>?>(),
                                                     It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlugInInstance());

            SdkDevice device = CreateDevice();

            var result = await _hostContext.CreateDeviceAsync(device);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task TestCreateDeviceDeviceParentNotFoundExceptionAsync()
        {
            SdkDevice device = CreateDevice();

            var exception = await Assert.ThrowsAsync<DeviceParentNotFoundException>(async () => await _hostContext.CreateDeviceAsync(device));

            Assert.NotNull(exception);
            Assert.Equal(10, exception.Id);
        }

        [Fact]
        public async Task TestDeleteDeviceAsync()
        {
            _mockRepository
                .Setup(_ => _.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<ModelDevice, bool>>?>(),
                                                     It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ModelDevice());

            await _hostContext.DeleteDeviceAsync(1);

            _mockRepository
                .Verify(_ => _.DeleteAsync<ModelDevice>(It.IsAny<int>(),
                                                        It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetDevicesAsync()
        {
            var deviceModel = new ModelDevice()
            {
                Id = 1,
                BatteryLevel = 2,
                SignalLevel = 3,
            };

            deviceModel.Parameters.Add(new ModelBoolParameter() { Id = 1, Name = "name", Units = "units", Value = true, ParameterType = ParameterType.AbsoluteActiveEnergy });
            deviceModel.Parameters.Add(new ModelFloatParameter() { Id = 1, Value = 2.0f, Name = "name", Units = "units", ParameterType = ParameterType.TVOC });
            deviceModel.Parameters.Add(new ModelIntParameter() { Id = 1, Value = 20, Name = "name", Units = "units", ParameterType = ParameterType.ActivePower });
            deviceModel.Parameters.Add(new ModelStringParameter() { Id = 1, Value = "value", Name = "name", Units = "units", ParameterType = ParameterType.ActivePower });
            deviceModel.Parameters.Add(new ModelUIntParameter() { Id = 1, Value = 30, Name = "name", Units = "units", ParameterType = ParameterType.ForwardReactiveEnergy });

            List<ModelDevice> devices =
            [
                deviceModel
            ];

            _mockRepository.Setup(_ => _.GetEnumerableAsync(It.IsAny<Expression<Func<ModelDevice, bool>>?>(),
                                                            It.IsAny<Func<IQueryable<ModelDevice>, IOrderedQueryable<ModelDevice>>?>(),
                                                            It.IsAny<bool>(),
                                                            It.IsAny<List<string>?>(),
                                                            It.IsAny<CancellationToken>()))
            .Returns(devices.ToAsyncEnumerable());

            SdkDevice? result = null;

            await foreach (var device in _hostContext.GetDevicesAsync())
            {
                result = device;
            }

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.NotNull(result.Name);
            Assert.NotNull(result.Parameters);
            Assert.NotEmpty(result.Parameters);
            Assert.Equal(5, result.Parameters.Count);
            Assert.NotNull(result.BatteryLevel);
            Assert.NotNull(result.SignalLevel);

            Assert.IsType<BoolParameter>(result.Parameters[0]);

            Assert.Equal(1, result.Parameters[0].Id);
            Assert.Equal("name", result.Parameters[0].Name);
            Assert.Equal("units", result.Parameters[0].Units);
            Assert.Equal(ParameterType.AbsoluteActiveEnergy, result.Parameters[0].ParameterType);
        }

        private static SdkDevice CreateDevice()
        {
            List<BaseParameter> parameters = [
                new BoolParameter(1,true,"name","units",ParameterType.Pressure),
                new FloatParameter(1,2.0f,"name","units",ParameterType.Pressure),
                new IntParameter(1,-3,"name","units",ParameterType.Pressure),
                new StringParameter(1,"value","name","units",ParameterType.Pressure),
                new UIntParameter(1,4,"name","units",ParameterType.Pressure)
                ];

            return new SdkDevice(1, "name", parameters, 1, -2);
        }
    }
}