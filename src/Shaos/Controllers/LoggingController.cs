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

using Microsoft.AspNetCore.Mvc;
using Serilog.Events;
using Shaos.DataAnnotations;
using Shaos.Services.Logging;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Shaos.Controllers
{
    [Route("api/v{version:apiVersion}/logging")]
    public class LoggingController : CoreController
    {
        private readonly ILoggingConfiguration _loggingConfiguration;
        private readonly ILoggingConfigurationService _loggingConfigurationService;

        public LoggingController(ILogger<LoggingController> logger,
                                 ILoggingConfiguration loggingConfiguration,
                                 ILoggingConfigurationService loggingConfigurationService) : base(logger)
        {
            _loggingConfiguration = loggingConfiguration;
            _loggingConfigurationService = loggingConfigurationService;
        }

        [HttpGet("levels")]
        [EndpointDescription("Get the currently configured set of the log level switches")]
        [EndpointName("GetLogLevelSwitches")]
        [EndpointSummary("Gets the set of configured log level switches")]
        [ProducesResponseType<IDictionary<string, LogEventLevel>>(StatusCodes.Status200OK, Description = "The log level switches")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public IDictionary<string, LogEventLevel> GetLogLevelSwitches()
            => _loggingConfiguration.LoggingLevelSwitches.ToDictionary(_ => _.Key, _ => _.Value.MinimumLevel);

        [HttpPost("levels")]
        [EndpointDescription("Set the current log level switch level")]
        [EndpointName("SetLogLevelSwitch")]
        [EndpointSummary("Set a log level switch level")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public async Task<IActionResult> SetLogLevelSwitches([LogLevelSwitchName, Description("The log level switch name")] string name,
                                                             [Required, Description("The log level")] LogEventLevel level,
                                                             CancellationToken cancellationToken = default)
        {
            _loggingConfiguration.LoggingLevelSwitches[name].MinimumLevel = level;

            await _loggingConfigurationService.UpdateLogLevelSwitchAsync(name, level, cancellationToken);

            return Accepted();
        }
    }
}