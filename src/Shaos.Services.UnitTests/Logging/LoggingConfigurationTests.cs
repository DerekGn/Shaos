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

using Microsoft.Extensions.Configuration;
using Serilog;
using Shaos.Services.Logging;
using Xunit;

namespace Shaos.Services.UnitTests.Logging
{
    public class LoggingConfigurationTests
    {
        private readonly LoggingConfiguration _loggingConfiguration;

        public LoggingConfigurationTests()
        {
            _loggingConfiguration = new LoggingConfiguration();
        }

        [Fact]
        public void TestConfigure()
        {
            var settings = new Dictionary<string, string?> 
            {
                { "Serilog:Using", "Serilog.Sinks.Console"},
                { "Serilog:MinimumLevel:Default", "Information"},
                { "Serilog:MinimumLevel:Override:System", "Warning"},
                { "Serilog:MinimumLevel:Override:Microsoft.AspNetCore.Mvc", "Warning"},
                { "Serilog:MinimumLevel:Override:Microsoft.AspNetCore.Routing", "Warning"},
                { "Serilog:MinimumLevel:Override:Microsoft.AspNetCore.Hosting", "Warning"},
                { "Serilog:WriteTo:Name", "Console"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            var loggerConfiguration = new LoggerConfiguration();

            _loggingConfiguration.Configure(configuration, loggerConfiguration);

            Assert.NotNull(_loggingConfiguration.LoggingLevelSwitches);
            Assert.Equal(4, _loggingConfiguration.LoggingLevelSwitches.Count);
        }
    }
}