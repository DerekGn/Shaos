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
using Serilog.Core;
using Serilog.Settings.Configuration;
using System.Collections.Frozen;
using System.Reflection;

namespace Shaos.Services.Logging
{
    /// <summary>
    /// The logger configuration settings
    /// </summary>
    public class LoggingConfiguration : ILoggingConfiguration
    {
        private FrozenDictionary<string, LoggingLevelSwitch> _logLevelSwitches;
        private readonly Assembly[] _loggerConfigurationAssemblies;

        public LoggingConfiguration()
        {
            _loggerConfigurationAssemblies = new[]
            {
                typeof(ILogger).Assembly,
                typeof(ConsoleLoggerConfigurationExtensions).Assembly,
                typeof(FileLoggerConfigurationExtensions).Assembly,
            };

            _logLevelSwitches = FrozenDictionary<string, LoggingLevelSwitch>.Empty;
        }

        /// <summary>
        /// Configure this instance
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/> instance</param>
        /// <param name="loggerConfiguration">The <see cref="LoggerConfiguration"/> instance</param>
        public void Configure(IConfiguration configuration, LoggerConfiguration loggerConfiguration)
        {
            var logLevelSwitches = new Dictionary<string, LoggingLevelSwitch>();
            var readerOptions = new ConfigurationReaderOptions(_loggerConfigurationAssemblies)
            {
                OnLevelSwitchCreated = logLevelSwitches.Add
            };
            loggerConfiguration.ReadFrom.Configuration(configuration, readerOptions);

            _logLevelSwitches = logLevelSwitches.ToFrozenDictionary();
        }

        /// </<inheritdoc/>
        public IReadOnlyDictionary<string, LoggingLevelSwitch> LogLevelSwitches => _logLevelSwitches;
    }
}
