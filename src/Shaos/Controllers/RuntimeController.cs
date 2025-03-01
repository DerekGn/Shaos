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
using Shaos.Services.Runtime;
using Swashbuckle.AspNetCore.Annotations;

namespace Shaos.Controllers
{
    [Route("api/v{version:apiVersion}/runtime")]
    public class RuntimeController : BasePlugInController
    {
        private readonly IPlugInRuntime _plugInRuntime;

        public RuntimeController(
            ILogger<RuntimeController> logger,
            IPlugInService plugInService,
            IPlugInRuntime plugInRuntime) : base(logger, plugInService)
        {
            _plugInRuntime = plugInRuntime ?? throw new ArgumentNullException(nameof(plugInRuntime));
        }

        [HttpGet("{id}")]
        [SwaggerResponse(StatusCodes.Status200OK, "Get an executing PlugIn by identifier", Type = typeof(Api.Model.v1.ExecutingPlugIn))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The PlugIn is not currently executing")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Get an executing PlugIn",
            Description = "Returns an executing PlugIn",
            OperationId = "GetExecutingPlugIn")]
        public ActionResult<Api.Model.v1.ExecutingPlugIn> GetExecutingPlugIn(
            [FromRoute, SwaggerParameter("The PlugIn identifier of the PlugIn", Required = true)] int id)
        {
            var executingPlugIn = _plugInRuntime.GetExecutingPlugIn(id);

            if (executingPlugIn == null)
            {
                return NotFound();
            }
            else
            {
                return new OkObjectResult(executingPlugIn);
            }
        }

        [HttpGet()]
        [SwaggerResponse(StatusCodes.Status200OK, "The list of executing PlugIns", Type = typeof(IEnumerable<Api.Model.v1.ExecutingPlugIn>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Gets the list of executing PlugIns", 
            Description = "The list of executing PlugIns",
            OperationId = "GetExecutingPlugIns")]
        public IEnumerable<Api.Model.v1.ExecutingPlugIn> GetExecutingPlugInsAsync()
        {
            foreach (var item in _plugInRuntime.GetExecutingPlugIns())
            {
                yield return item.ToApiModel();
            }
        }

        [HttpPut("{id}/start")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The PlugIn will be started")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The PlugIn is not currently executing")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Start a PlugIn",
            Description = "Start a PlugIn executing",
            OperationId = "StartPlugIn")]
        public async Task<ActionResult> StartPlugInAsync(
                [FromRoute, SwaggerParameter("The PlugIn identifier to start", Required = true)] int id,
                CancellationToken cancellationToken)
        {
            return await GetPlugInOperationAsync(id, async (plugIn, cancellationToken) =>
            {
                await _plugInRuntime.StartPlugInAsync(plugIn, cancellationToken);

                return Accepted();
            },
            cancellationToken);
        }

        [HttpPut("{id}/stop")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "A PlugIn will be stopped")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The PlugIn is not currently executing")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Stop an executing PlugIn", Description = "Stop an executing PlugIn", OperationId = "StopPlugIn")]
        public async Task<ActionResult> StopPlugInAsync(
            [FromRoute, SwaggerParameter("The PlugIn identifier to stop", Required = true)] int id,
            CancellationToken cancellationToken)
        {
            return await GetPlugInOperationAsync(id, async (plugIn, cancellationToken) =>
            {
                await _plugInRuntime.StopPlugInAsync(plugIn, cancellationToken);

                return Accepted();
            },
            cancellationToken);
        }
    }
}