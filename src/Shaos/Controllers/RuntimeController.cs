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
using Shaos.Services.Store;
using Swashbuckle.AspNetCore.Annotations;

using ExecutingInstanceApi = Shaos.Api.Model.v1.ExecutingInstance;

namespace Shaos.Controllers
{
    [Route("api/v{version:apiVersion}/runtime")]
    public class RuntimeController : BasePlugInController
    {
        private const string ExecutingInstanceIdentifier = "The ExecutingInstance identifier";
        private const string PlugInInstanceIdentifier = "The PlugInInstance identifier";
        private readonly IRuntimeService _runtimeService;

        public RuntimeController(
            ILogger<RuntimeController> logger,
            IStore store,
            IPlugInService plugInService,
            IRuntimeService runtimeService) : base(logger, store, plugInService)
        {
            _runtimeService = runtimeService ?? throw new ArgumentNullException(nameof(runtimeService));
        }

        [HttpGet("{id}")]
        [SwaggerResponse(StatusCodes.Status200OK, "The ExecutingInstance", Type = typeof(ExecutingInstanceApi))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The PlugInInstance is not currently executing")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Get an ExecutingInstance",
            Description = "Returns an ExecutingInstance",
            OperationId = "GetExecutingInstance")]
        public ActionResult<ExecutingInstanceApi> GetExecutingInstance(
            [FromRoute, SwaggerParameter(PlugInInstanceIdentifier, Required = true)] int id)
        {
            var executingInstance = _runtimeService.GetExecutingInstance(id);

            return executingInstance == null ? NotFound() : new OkObjectResult(executingInstance);
        }

        [HttpGet()]
        [SwaggerResponse(StatusCodes.Status200OK, "The list of ExecutingInstances", Type = typeof(IEnumerable<ExecutingInstanceApi>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Gets the list of ExecutingInstances",
            Description = "The list of ExecutingInstances",
            OperationId = "GetExecutingInstances")]
        public IEnumerable<ExecutingInstanceApi> GetExecutingInstancesAsync()
        {
            foreach (var item in _runtimeService.GetExecutingInstances())
            {
                yield return item.ToApi();
            }
        }

        [HttpPut("{id}/start")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The PlugInInstance will be started")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The PlugInInstance was not found")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Start a PlugInInstance",
            Description = "Start a PlugInInstance executing",
            OperationId = "StartInstance")]
        public async Task<ActionResult> StartInstanceAsync(
                [FromRoute, SwaggerParameter(PlugInInstanceIdentifier, Required = true)] int id,
                CancellationToken cancellationToken)
        {
            var plugInInstance = await Store
                .GetPlugInInstanceByIdAsync(id, cancellationToken);

            if (plugInInstance == null)
            {
                return NotFound();
            }
            else
            {
                await _runtimeService
                    .StartInstanceAsync(
                        plugInInstance.Id,
                        plugInInstance.Name,
                        plugInInstance.PlugIn.Package.AssemblyFile,
                        cancellationToken);
            }

            return Accepted();
        }

        [HttpPut("{id}/stop")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "A ExecutingInstance will be stopped")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The ExecutingInstance is not currently executing")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Stop an ExecutingInstance",
            Description = "Stop an ExecutingInstance", OperationId = "StopInstance")]
        public async Task<ActionResult> StopInstanceAsync(
            [FromRoute, SwaggerParameter(ExecutingInstanceIdentifier, Required = true)] int id,
            CancellationToken cancellationToken)
        {
            //return await GetPlugInOperationAsync(id, async (plugIn, cancellationToken) =>
            //{
            //    await _plugInRuntime.StopInstanceAsync(plugIn, cancellationToken);

            //    return Accepted();
            //},
            //cancellationToken);

            return Ok();
        }
    }
}