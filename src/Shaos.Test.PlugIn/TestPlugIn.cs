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

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Shaos.Sdk;
using Shaos.Sdk.Devices;
using Shaos.Sdk.Devices.Parameters;

[assembly: ExcludeFromCodeCoverage]

namespace Shaos.Test.PlugIn
{
    [ExcludeFromCodeCoverage]
    [PlugInDescription("Name", "Description")]
    public class TestPlugIn : PlugInBase
    {
        private const string Units = "units";
        private readonly TestPlugInConfiguration _configuration;
        private readonly ILogger<TestPlugIn> _logger;

        public TestPlugIn(ILogger<TestPlugIn> logger,
                          TestPlugInConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(configuration);

            _logger = logger;
            _configuration = configuration;
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting [{Name}].[{Operation}]",
                                   nameof(TestPlugIn),
                                   nameof(ExecuteAsync));

            await CreateDevicesAsync();

            float floatValue = 1;
            int intValue = 1;
            uint uintValue = 1;

            var boolParameter = (BoolParameter)Devices.First().Parameters.First();
            var boolWriteParameter = (BoolParameter)Devices.First().Parameters.Skip(1).First();
            var floatParameter = (FloatParameter)Devices.First().Parameters.Skip(2).First();
            var floatWriteParameter = (FloatParameter)Devices.First().Parameters.Skip(3).First();
            var intParameter = (IntParameter)Devices.First().Parameters.Skip(4).First();
            var intWriteParameter = (IntParameter)Devices.First().Parameters.Skip(5).First();
            var stringParameter = (StringParameter)Devices.First().Parameters.Skip(6).First();
            var stringWriteParameter = (StringParameter)Devices.First().Parameters.Skip(7).First();
            var uintParameter = (UIntParameter)Devices.First().Parameters.Skip(8).First();
            var uintWriteParameter = (UIntParameter)Devices.First().Parameters.Skip(9).First();

            do
            {
                await Task.Delay(_configuration.Delay, cancellationToken);
                _logger.LogInformation("Executing [{Name}].[{Operation}]",
                                       nameof(TestPlugIn),
                                       nameof(ExecuteAsync));

                await boolParameter.NotifyValueChangedAsync(!boolParameter.Value);
                await boolWriteParameter.NotifyValueChangedAsync(!boolParameter.Value);
                await floatParameter.NotifyValueChangedAsync(IncrementLimit(ref floatValue, 1.0f, 10.0f));
                await floatWriteParameter.NotifyValueChangedAsync(IncrementLimit(ref floatValue, 1.0f, 10.0f));
                await intParameter.NotifyValueChangedAsync(IncrementLimit(ref intValue, 1, 10));
                await intWriteParameter.NotifyValueChangedAsync(IncrementLimit(ref intValue, 1, 10));
                await stringParameter.NotifyValueChangedAsync(intValue.ToString());
                await stringWriteParameter.NotifyValueChangedAsync(intValue.ToString());
                await uintParameter.NotifyValueChangedAsync(IncrementLimit(ref uintValue, 1, 10));
                await uintWriteParameter.NotifyValueChangedAsync(IncrementLimit(ref uintValue, 1, 10));

            } while (!cancellationToken.IsCancellationRequested);

            _logger.LogInformation("Completed [{Name}].[{Operation}]",
                                   nameof(TestPlugIn),
                                   nameof(ExecuteAsync));
        }

        private static int IncrementLimit(ref int value,
                                          int lower,
                                          int upper)
        {
            if (++value > upper)
            {
                value = lower;
            }

            return value;
        }

        private static uint IncrementLimit(ref uint value,
                                           uint lower,
                                           uint upper)
        {
            if (++value > upper)
            {
                value = lower;
            }

            return value;
        }

        private static float IncrementLimit(ref float value,
                                            float lower,
                                            float upper)
        {
            if (++value > upper)
            {
                value = lower;
            }

            return value;
        }

        private async Task CreateDevicesAsync()
        {
            if (!Devices.Any(_ => _.Name == "TestDevice"))
            {
                List<IBaseParameter> baseParameters =
                [
                    new BoolParameter(1,
                                      false,
                                      "Test bool parameter",
                                      Units),
                    new BoolParameter(2,
                                      false,
                                      "Test write bool parameter",
                                      Units,
                                      WriteBool),
                    new FloatParameter(3,
                                       1.0f,
                                       0,
                                       10,
                                       0.1f,
                                       "Test float parameter",
                                       Units),
                    new FloatParameter(4,
                                       1.0f,
                                       0,
                                       10,
                                       0.1f,
                                       "Test write float parameter",
                                       Units,
                                       WriteFloat),
                    new IntParameter(5,
                                     0,
                                     0,
                                     10,
                                     1,
                                     "Test int parameter",
                                     Units),
                    new IntParameter(6,
                                     0,
                                     0,
                                     10,
                                     1,
                                     "Test write int parameter",
                                     Units,
                                     WriteInt),
                    new StringParameter(7,
                                     "",
                                     "Test string parameter",
                                     Units),
                    new StringParameter(8,
                                     "",
                                     "Test write string parameter",
                                     Units,
                                     WriteString),
                    new UIntParameter(9,
                                     0,
                                     0,
                                     10,
                                     1,
                                     "Test uint parameter",
                                     Units),
                    new UIntParameter(10,
                                     0,
                                     0,
                                     10,
                                     1,
                                     "Test write uint parameter",
                                     Units,
                                     WriteUInt),
                ];

                var device = new Device(10,
                                        "TestDevice",
                                        baseParameters);

                await Devices.AddAsync(device);
            }
        }

        private async Task WriteBool(int id, bool value)
        {
        }

        private async Task WriteFloat(int id, float value)
        {
        }

        private async Task WriteInt(int id, int value)
        {
        }

        private async Task WriteString(int id, string value)
        {
        }

        private async Task WriteUInt(int id, uint value)
        {
        }
    }
}