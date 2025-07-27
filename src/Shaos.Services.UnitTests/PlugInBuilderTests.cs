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
using Shaos.Repository.Exceptions;
using Shaos.Sdk;
using Shaos.Sdk.Devices;
using Shaos.Sdk.Devices.Parameters;
using Shaos.Services.Exceptions;
using Shaos.Services.UnitTests.Fixtures;
using System.Collections.ObjectModel;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests
{
    public class PlugInBuilderTests : PlugInBuilderBaseTests
    {
        private readonly PlugInBuilder _builder;
        private readonly Mock<IPlugIn> _plugIn;

        public PlugInBuilderTests(ITestOutputHelper output,
                                  TestFixture fixture) : base(output, fixture)
        {
            _plugIn = new Mock<IPlugIn>();

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

        [Fact]
        public void TestRestore()
        {
            _builder.PlugIn = _plugIn.Object;

            var plugInDevices = new ObservableCollection<Device>();

            _plugIn
                .Setup(_ => _.Devices)
                .Returns(plugInDevices);

            var parameter = new FloatParameter(1,
                                               1.0f,
                                               "volts",
                                               "v",
                                               ParameterType.Voltage);

            var device = new Device(1,
                                    "name",
                                    [parameter],
                                    100,
                                    0);

            _builder.Restore([device]);

            Assert.Single(plugInDevices);
        }

        [Fact]
        public void TestRestoreInstanceNotLoaded()
        {
            var parameter = new FloatParameter(1,
                                               1.0f,
                                               "volts",
                                               "v",
                                               ParameterType.Voltage);

            var device = new Device(1,
                                    "name",
                                    [parameter],
                                    100,
                                    0);

            Assert.Throws<PlugInInstanceNotLoadedException>(() => _builder.Restore([device]));
        }
    }
}