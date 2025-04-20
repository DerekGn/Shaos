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
using Shaos.Services.Runtime;
using Shaos.Services.Runtime.Exceptions;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using InstanceApi = Shaos.Api.Model.v1.Instance;

namespace Shaos.Controllers
{
    [Route("api/v{version:apiVersion}/runtime")]
    public class InstanceHostController : CoreController
    {
        private const string InstanceIdentifier = "The Instance identifier";

        private readonly IInstanceHost _instanceHost;

        public InstanceHostController(
            ILogger<InstanceHostController> logger,
            IInstanceHost instanceHost) : base(logger)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(instanceHost);

            _instanceHost = instanceHost;
        }

        [HttpGet()]
        [SwaggerResponse(StatusCodes.Status200OK, "The list of Instances", Type = typeof(IEnumerable<InstanceApi>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Gets the list of Instances",
            Description = "The list of Instances",
            OperationId = "GetInstances")]
        public IEnumerable<InstanceApi> GetInstances()
        {
            foreach (var item in _instanceHost.Instances)
            {
                yield return item.ToApi();
            }
        }

        [HttpPut("{id}/start")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The Instance will be started")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The Instance was not found", Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Start a Instance",
            Description = "Start a Instance executing",
            OperationId = "StartInstance")]
        public ActionResult StartInstance(
                [FromRoute, SwaggerParameter(InstanceIdentifier, Required = true)] int id)
        {
            return HandleError(id, (id) =>
            {
                _instanceHost.StartInstance(id);

                return Accepted();
            });
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
            return HandleError(id, (id) =>
            {
                _instanceHost.StopInstance(id);

                return Accepted();
            });
        }

        private ActionResult HandleError(int id, Func<int, ActionResult> operation)
        {
            try
            {
                operation(id);
            }
            catch (InstanceNotFoundException)
            {
                return BadRequest(
                    CreateProblemDetails(HttpStatusCode.NotFound,
                        $"Instance [{id}] not found"));
            }
            catch (ArgumentOutOfRangeException)
            {
                return BadRequest(
                    CreateProblemDetails(HttpStatusCode.BadRequest,
                        "Id is invalid"));
            }

            return Accepted();
        }
    }
}