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
using Swashbuckle.AspNetCore.Annotations;

namespace Shaos.Controllers
{
    [Route("api/v{version:apiVersion}/system")]
    public class SystemController : CoreController
    {
        private readonly ISystemService _systemService;
        private readonly IAppVersionService _appVersionService;

        public SystemController(
            ILogger<SystemController> logger,
            ISystemService systemService,
            IAppVersionService appVersionService) : base(logger)
        {
            _systemService = systemService?? throw new ArgumentNullException(nameof(systemService));
            _appVersionService = appVersionService ?? throw new ArgumentNullException(nameof(appVersionService));
        }

        [HttpGet("version")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns the application version")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Get the application version number",
            Description = "Get the application version number",
            OperationId = "GetVersion")]
        public ActionResult GetVersion()
        {
            return Ok(_appVersionService.Version);
        }

        [HttpPost("shutdown")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Shutdown the application host",
            Description = "Shuts down the application host. The process terminates",
            OperationId = "ShutdownApplication")]
        public void ShutdownApplication()
        {
            _systemService.ShutdownApplication();
        }

        [HttpGet("os")]
        [SwaggerResponse(StatusCodes.Status200OK, "Get the operating system information")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Get the Os information",
            Description = "The Os information details",
            OperationId = "GetOsInformation")]
        public ActionResult GetOsInformation()
        {
            return Ok(_systemService.GetOsInformation().ToApi());
        }

        [HttpGet("environment")]
        [SwaggerResponse(StatusCodes.Status200OK, "Get the operating system environment information")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Get the application environment",
            Description = "The application environment",
            OperationId = "GetEnvironment")]
        public ActionResult GetEnvironment()
        {
            return Ok(_systemService.GetEnvironment().ToApi());
        }

        [HttpGet("process")]
        [SwaggerResponse(StatusCodes.Status200OK, "Get the application process information")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Get the application process information",
            Description = "The application process information",
            OperationId = "GetProcessInformation")]
        public ActionResult GetProcessInformation()
        {
            return Ok(_systemService.GetProcessInformation().ToApi());
        }
    }
}
