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
using Shaos.Extensions;
using Shaos.Services;
using Shaos.Services.SystemInformation;

namespace Shaos.Controllers
{
    [Route("api/v{version:apiVersion}/system")]
    public class SystemController : CoreController
    {
        private readonly IAppVersionService _appVersionService;
        private readonly ISystemService _systemService;

        public SystemController(ILogger<SystemController> logger,
                                ISystemService systemService,
                                IAppVersionService appVersionService) : base(logger)
        {
            _systemService = systemService ?? throw new ArgumentNullException(nameof(systemService));
            _appVersionService = appVersionService ?? throw new ArgumentNullException(nameof(appVersionService));
        }

        [HttpGet("environment")]
        [EndpointDescription("Get the application environment")]
        [EndpointName("GetApplicationEnvironment")]
        [EndpointSummary("Get the application environment")]
        [ProducesResponseType(StatusCodes.Status200OK, Description = "Get the operating system environment information")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public ActionResult GetEnvironment()
        {
            return Ok(_systemService.GetEnvironment().ToApi());
        }

        [HttpGet("os")]
        [EndpointDescription("Get the host system OS Information details")]
        [EndpointName("GetOsInformation")]
        [EndpointSummary("Get the OS information")]
        [ProducesResponseType(StatusCodes.Status200OK, Description = "Get the operating system information")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public ActionResult GetOsInformation()
        {
            return Ok(_systemService.GetOsInformation().ToApi());
        }

        [HttpGet("process")]
        [EndpointDescription("Get the Application Process Information")]
        [EndpointName("GetProcessInformation")]
        [EndpointSummary("Get the Application Process Information")]
        [ProducesResponseType(StatusCodes.Status200OK, Description = "Get the application process information")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public ActionResult GetProcessInformation()
        {
            return Ok(_systemService.GetProcessInformation().ToApi());
        }

        [HttpGet("version")]
        [EndpointDescription("Get the Application Version Number")]
        [EndpointName("GetApplicationVersion")]
        [EndpointSummary("Get the Application Version Number")]
        [ProducesResponseType(StatusCodes.Status200OK, Description = "Returns the application version")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public ActionResult GetVersion()
        {
            return Ok(_appVersionService.Version);
        }

        [HttpPost("shutdown")]
        [EndpointDescription("Shuts down the Application Host. The process terminates")]
        [EndpointName("ShutdownApplication")]
        [EndpointSummary("Shutdown the Application Host")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public void ShutdownApplication()
        {
            _systemService.ShutdownApplication();
        }
    }
}