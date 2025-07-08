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
using Shaos.Repository.Models.Devices.Parameters;
using Shaos.Sdk.Devices;
using Shaos.Testing.Shared;
using Shaos.Testing.Shared.Extensions;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;

using DeviceModel = Shaos.Repository.Models.Devices.Device;

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
        public async Task TestDeleteDeviceAsync()
        {
            _mockRepository
                .Setup(_ => _.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DeviceModel, bool>>?>(),
                                                     It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeviceModel());

            await _hostContext.DeleteDeviceAsync(1);

            _mockRepository
                .Verify(_ => _.DeleteAsync<DeviceModel>(It.IsAny<int>(),
                                                        It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetDevicesAsync()
        {
            var deviceModel = new DeviceModel()
            {
                Id = 1,
                BatteryLevel = 2,
                SignalLevel = 3,
            };

            deviceModel.Parameters.Add(new BoolParameter() { Id = 1, Name = "name", Units = "units", Value = true, ParameterType = Sdk.Devices.Parameters.ParameterType.AbsoluteActiveEnergy });
            deviceModel.Parameters.Add(new FloatParameter() { });
            deviceModel.Parameters.Add(new IntParameter());
            deviceModel.Parameters.Add(new StringParameter());
            deviceModel.Parameters.Add(new UIntParameter());

            List<DeviceModel> devices =
            [
                deviceModel
            ];

            _mockRepository.Setup(_ => _.GetEnumerableAsync<DeviceModel>(It.IsAny<Expression<Func<DeviceModel, bool>>?>(),
                                                        It.IsAny<Func<IQueryable<DeviceModel>, IOrderedQueryable<DeviceModel>>?>(),
                                                        It.IsAny<bool>(),
                                                        It.IsAny<List<string>?>(),
                                                        It.IsAny<CancellationToken>()))
            .Returns(devices.ToAsyncEnumerable());

            Device? result = null;

            await foreach(var device in _hostContext.GetDevicesAsync())
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

            Assert.IsType<Sdk.Devices.Parameters.BoolParameter>(result.Parameters[0]);

            Assert.Equal(1, result.Parameters[0].Id);
            Assert.Equal("name", result.Parameters[0].Name);
            Assert.Equal("units", result.Parameters[0].Units);
            Assert.Equal(Sdk.Devices.Parameters.ParameterType.AbsoluteActiveEnergy, result.Parameters[0].ParameterType);
        }
    }
}