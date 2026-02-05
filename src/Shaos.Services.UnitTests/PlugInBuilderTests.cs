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
using Shaos.Repository.Models;
using Shaos.Sdk.Devices.Parameters;
using Shaos.Services.Exceptions;
using Shaos.Services.UnitTests.Fixtures;
using Shaos.Test.PlugIn;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests
{
    public class PlugInBuilderTests : PlugInBuilderBaseTests
    {
        private readonly PlugInBuilder _builder;

        public PlugInBuilderTests(ITestOutputHelper output,
                                  TestFixture fixture) : base(output, fixture)
        {
            _builder = new PlugInBuilder(LoggerFactory!,
                                         LoggerFactory!.CreateLogger<PlugInBuilder>());
        }

        [Fact]
        public void TestLoad()
        {
            var context = _unloadingWeakReference.Target;
            var assembly = context.LoadFromAssemblyPath(_fixture.AssemblyFilePath);

            _builder.Load(assembly,
                          "{\"Delay\": \"00:00:00\"}");

            Assert.NotNull(_builder.PlugIn);
        }

        [Fact(Skip = "TODO")]
        public void TestRestore()
        {
            PlugInInstance plugInInstance = SetupPlugInInstance();

            _builder.PlugIn = new TestPlugIn(LoggerFactory!.CreateLogger<TestPlugIn>(),
                                             new TestPlugInConfiguration());

            _builder.Restore(plugInInstance);

            var plugIn = _builder.PlugIn;

            Assert.Single(plugIn.Devices);
            Assert.Equal(plugInInstance.Id, plugIn.Id);
        }

        [Fact]
        public void TestRestoreInstanceNotLoaded()
        {
            PlugInInstance plugInInstance = SetupPlugInInstance();

            Assert.Throws<PlugInInstanceNotLoadedException>(() => _builder.Restore(plugInInstance));
        }

        private static PlugInInstance SetupPlugInInstance()
        {
            var device = new Repository.Models.Devices.Device()
            {
                BatteryLevel = 100,
                Id = 2,
                Name = "name",
                SignalLevel = -10
            };

            device.Parameters.Add(new Repository.Models.Devices.Parameters.BoolParameter()
            {
                Id = 1,
                Name = "name",
                ParameterType = ParameterType.Voltage,
                Units = "units",
                Value = true
            });

            device.Parameters.Add(new Repository.Models.Devices.Parameters.FloatParameter()
            {
                Id = 1,
                Name = "name",
                ParameterType = ParameterType.Voltage,
                Units = "units",
                Value = 100
            });

            device.Parameters.Add(new Repository.Models.Devices.Parameters.IntParameter()
            {
                Id = 1,
                Name = "name",
                ParameterType = ParameterType.Voltage,
                Units = "units",
                Value = -10
            });

            device.Parameters.Add(new Repository.Models.Devices.Parameters.StringParameter()
            {
                Id = 1,
                Name = "name",
                ParameterType = ParameterType.Voltage,
                Units = "units",
                Value = "string"
            });

            device.Parameters.Add(new Repository.Models.Devices.Parameters.UIntParameter()
            {
                Id = 1,
                Name = "name",
                ParameterType = ParameterType.Voltage,
                Units = "units",
                Value = 7896
            });

            var plugInInstance = new PlugInInstance()
            {
                Id = 3
            };

            plugInInstance.Devices.Add(device);

            return plugInInstance;
        }
    }
}