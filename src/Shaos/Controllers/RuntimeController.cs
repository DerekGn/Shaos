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
using Shaos.Services.Repositories;
using Shaos.Services.Runtime;
using Swashbuckle.AspNetCore.Annotations;

using InstanceApi = Shaos.Api.Model.v1.Instance;

namespace Shaos.Controllers
{
    [Route("api/v{version:apiVersion}/runtime")]
    public class RuntimeController : BasePlugInController
    {
        private const string InstanceIdentifier = "The Instance identifier";

        private readonly IRuntimeService _runtimeService;

        public RuntimeController(
            ILogger<RuntimeController> logger,
            IPlugInService plugInService,
            IRuntimeService runtimeService,
            IPlugInRepository plugInRepository,
            IPlugInInstanceRepository plugInInstanceRepository) : base(logger, plugInService, plugInRepository, plugInInstanceRepository)
        {
            _runtimeService = runtimeService ?? throw new ArgumentNullException(nameof(runtimeService));
        }

        [HttpGet("{id}")]
        [SwaggerResponse(StatusCodes.Status200OK, "The Instance", Type = typeof(InstanceApi))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The Instance is not currently executing")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Get an Instance",
            Description = "Returns an Instance",
            OperationId = "GetInstance")]
        public ActionResult<InstanceApi> GetInstance(
            [FromRoute, SwaggerParameter(InstanceIdentifier, Required = true)] int id)
        {
#warning TODO
            //var executingInstance = _runtimeService.GetInstance(id);

            //return executingInstance == null ? NotFound() : new OkObjectResult(executingInstance.ToApi());

            return NotFound();
        }

        [HttpGet()]
        [SwaggerResponse(StatusCodes.Status200OK, "The list of Instances", Type = typeof(IEnumerable<InstanceApi>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Gets the list of Instances",
            Description = "The list of Instances",
            OperationId = "GetInstances")]
        public IEnumerable<InstanceApi> GetInstancesAsync()
        {
#warning TODO
            //foreach (var item in _runtimeService.GetInstances())
            //{
            //    yield return item.ToApi();
            //}

            return null;
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
                [FromRoute, SwaggerParameter(InstanceIdentifier, Required = true)] int id,
                CancellationToken cancellationToken)
        {
            var plugInInstance = await PlugInInstanceRepository
                .GetByIdAsync(id, 
                includeProperties: ["PlugIn"],
                cancellationToken: cancellationToken);

            if (plugInInstance == null)
            {
                return NotFound();
            }
            else
            {
#warning TODO
                //_runtimeService
                //    .StartInstance(
                //        plugInInstance.PlugIn!,
                //        plugInInstance);
            }

            return Accepted();
        }

        [HttpPut("{id}/stop")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "A ExecutingInstance will be stopped")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Stop an Instance",
            Description = "Stop an Instance",
            OperationId = "StopInstance")]
        public ActionResult StopInstance(
            [FromRoute, SwaggerParameter(InstanceIdentifier, Required = true)] int id)
        {
#warning TODO
            //_runtimeService.StopInstance(id);

            return Accepted();
        }
    }
}