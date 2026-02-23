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
using Serilog.Events;
using Shaos.Repository;
using Shaos.Repository.Models;
using Shaos.Services.Extensions;

namespace Shaos.Services.Logging
{
    /// <summary>
    /// A logging configuration service. Used to store and configure logging configuration settings
    /// </summary>
    public class LoggingConfigurationService : ILoggingConfigurationService
    {
        private readonly ILogger<LoggingConfigurationService> _logger;
        private readonly IShaosRepository _repository;

        /// <summary>
        /// Create an instance of a <see cref="LoggingConfigurationService"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> instance</param>
        /// <param name="repository">The <see cref="IShaosRepository"/> instance used for storing logging changes</param>
        /// <exception cref="ArgumentNullException"></exception>
        public LoggingConfigurationService(ILogger<LoggingConfigurationService> logger,
                                           IShaosRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task InitialiseLoggingConfigurationAsync(ILoggingConfiguration loggingConfiguration,
                                                              CancellationToken cancellationToken = default)
        {
            _logger.LogInitalisingLoggingConfiguration();

            await foreach (var logSwitch in _repository.GetEnumerableAsync<LogLevelSwitch>(cancellationToken))
            {
                _logger.LogUpdatingLogLevelSwitch(logSwitch.Name,
                                                  logSwitch.Level);

                loggingConfiguration.LoggingLevelSwitches[logSwitch.Name].MinimumLevel = logSwitch.Level;
            }
        }

        /// <inheritdoc/>
        public async Task UpdateLogLevelSwitchAsync(string name,
                                                    LogEventLevel level,
                                                    CancellationToken cancellationToken = default)
        {
            _logger.LogUpdatingLogLevelSwitch(name,
                                              level);

            await _repository.UpsertLogLevelSwitchAsync(name,
                                                        level,
                                                        cancellationToken);
        }
    }
}