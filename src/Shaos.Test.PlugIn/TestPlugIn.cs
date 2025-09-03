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
            _logger.LogInformation("Starting [{Name}].[{Operation}]", nameof(TestPlugIn), nameof(ExecuteAsync));

            await CreateDevicesAsync();

            var freqParameter = (IntParameter)Devices.First().Parameters.First();

            Random random = new Random();

            do
            {
                await Task.Delay(_configuration.Delay, cancellationToken);
                _logger.LogInformation("Executing [{Name}].[{Operation}]",
                                       nameof(TestPlugIn),
                                       nameof(ExecuteAsync));

                Devices.First().SignalLevel!.Level = random.Next(-100, 0);
                Devices.First().BatteryLevel!.Level = (uint)random.Next(0, 100);

                await freqParameter.WriteValueAsync(random.Next(0, 50));
            } while (!cancellationToken.IsCancellationRequested);

            _logger.LogInformation("Completed [{Name}].[{Operation}]", nameof(TestPlugIn), nameof(ExecuteAsync));
        }

        private async Task CreateDevicesAsync()
        {
            if (!Devices.Any(_ => _.Name == "TestDevice"))
            {
                List<IBaseParameter> baseParameters =
                [
                    new IntParameter(0, "Test Parameter", "Units", ParameterType.Frequency)
                ];

                await Devices.AddAsync(new Device("TestDevice", baseParameters, 100, -1));
            }
        }
    }
}