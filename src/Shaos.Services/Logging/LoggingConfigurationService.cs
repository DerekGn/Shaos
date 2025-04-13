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
using Shaos.Repository.Models;
using Shaos.Services.Repositories;

namespace Shaos.Services.Logging
{
    public class LoggingConfigurationService : ILoggingConfigurationService
    {
        private readonly ILoggingConfigurationRepository _repository;
        private readonly ILogger<LoggingConfigurationService> _logger;

        public LoggingConfigurationService(
            ILoggingConfigurationRepository repository,
            ILogger<LoggingConfigurationService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InitialiseLoggingConfigurationAsync(
            ILoggingConfiguration loggingConfiguration,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Initialising logging configuration");

            await foreach (var logSwitch in _repository.GetAsync(cancellationToken))
            {
                _logger.LogDebug("Updating LogLevelSwitch [{Name}] Level: [{Level}]", logSwitch.Name, logSwitch.Level);

                loggingConfiguration.LoggingLevelSwitches[logSwitch.Name].MinimumLevel = logSwitch.Level;
            }
        }

        public async Task UpdateLogLevelSwitchAsync(
            string name,
            LogEventLevel level,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating {Type} [{Name}] Level: [{Level}]",
                nameof(LogLevelSwitch),
                name,
                level);

            await _repository.UpsertAsync(name, level, cancellationToken);
        }
    }
}