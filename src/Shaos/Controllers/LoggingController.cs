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
using Swashbuckle.AspNetCore.Annotations;
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
            _loggingConfiguration = loggingConfiguration ?? throw new ArgumentNullException(nameof(loggingConfiguration));
            _loggingConfigurationService = loggingConfigurationService ?? throw new ArgumentNullException(nameof(loggingConfigurationService));
        }

        [HttpGet("levels")]
        [SwaggerResponse(StatusCodes.Status200OK, "The log level switches", Type = typeof(IDictionary<string, LogEventLevel>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Gets the set of configured log level switches",
            Description = "Get the currently configured set of the log level switches",
            OperationId = "GetLogLevelSwitches")]
        public IDictionary<string, LogEventLevel> GetLogLevelSwitches()
            => _loggingConfiguration.LoggingLevelSwitches.ToDictionary(_ => _.Key, _ => _.Value.MinimumLevel);

        [HttpPost("levels")]
        [SwaggerResponse(StatusCodes.Status202Accepted)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Set a log level switch level",
            Description = "Set the current log level switch level",
            OperationId = "SetLogLevelSwitch")]
        public async Task<IActionResult> SetLogLevelSwitches([LogLevelSwitchName] string name,
                                                             [Required] LogEventLevel level,
                                                             CancellationToken cancellationToken)
        {
            _loggingConfiguration.LoggingLevelSwitches[name].MinimumLevel = level;

            await _loggingConfigurationService.UpdateLogLevelSwitchAsync(name, level, cancellationToken);

            return Accepted();
        }
    }
}