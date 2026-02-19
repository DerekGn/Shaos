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
using Shaos.Services.Runtime.Exceptions;
using Shaos.Services.Runtime.Host;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;
using InstanceApi = Shaos.Api.Model.v1.Instance;

namespace Shaos.Controllers
{
    [Route("api/v{version:apiVersion}/runtime")]
    public class InstanceHostController : CoreController
    {
        private const string InstanceIdentifier = "The Instance identifier";

        private readonly IRuntimeInstanceHost _instanceHost;
        private readonly IInstanceHostService _instanceHostService;

        public InstanceHostController(ILogger<InstanceHostController> logger,
                                      IRuntimeInstanceHost instanceHost,
                                      IInstanceHostService instanceHostService) : base(logger)
        {
            _instanceHost = instanceHost;
            _instanceHostService = instanceHostService;
        }

        [HttpGet()]
        [EndpointDescription("Get the list of Runtime Instances")]
        [EndpointName("Gets the list of Runtime Instances")]
        [EndpointSummary("GetRuntimeInstances")]
        [ProducesResponseType<IEnumerable<InstanceApi>>(StatusCodes.Status200OK, Description = "The list of Instances")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public IEnumerable<InstanceApi> GetInstances()
        {
            foreach (var item in _instanceHost.Instances)
            {
                yield return item.ToApi();
            }
        }

        [HttpPut("{id}/start")]
        [EndpointDescription("Start a PlugIn Instance executing")]
        [EndpointName("StartPlugInInstance")]
        [EndpointSummary("Start a PlugIn Instance")]
        [ProducesResponseType(StatusCodes.Status202Accepted, Description = "The Instance will be started")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, Description = "The Instance was not found")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public async Task<ActionResult> StartInstance([FromRoute, Required, Description(InstanceIdentifier), Range(1, int.MaxValue)] int id,
                                                CancellationToken cancellationToken = default)
        {
            return await HandleErrorAsync(id, async (id) =>
            {
                await _instanceHostService.StartInstanceAsync(id);

                return Accepted();
            });
        }

        [HttpPut("{id}/stop")]
        [EndpointDescription("Stop an executing PlugIn Instance")]
        [EndpointName("StopPlugInInstance")]
        [EndpointSummary("Stop a PlugIn Instance")]
        [ProducesResponseType(StatusCodes.Status202Accepted, Description = "An executing PlugIn Instance will be stopped")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public ActionResult StopInstance([FromRoute, Required, Description(InstanceIdentifier), Range(1, int.MaxValue)] int id)
        {
            return HandleError(id, (id) =>
            {
                _instanceHost.StopInstance(id);

                return Accepted();
            });
        }

        private ActionResult HandleError(int id,
                                         Func<int, ActionResult> operation)
        {
            try
            {
                operation(id);
            }
            catch (InstanceNotFoundException)
            {
                return NotFound(CreateProblemDetails(HttpStatusCode.NotFound,
                                                       $"Instance [{id}] not found"));
            }
            catch (ArgumentOutOfRangeException)
            {
                return BadRequest(CreateProblemDetails(HttpStatusCode.BadRequest,
                                                       "Id is invalid"));
            }

            return Accepted();
        }

        private async Task<ActionResult> HandleErrorAsync(int id,
                                                          Func<int, Task<AcceptedResult>> operation)
        {
            try
            {
                await operation(id);
            }
            catch (InstanceNotFoundException)
            {
                return NotFound(CreateProblemDetails(HttpStatusCode.NotFound,
                                                       $"Instance [{id}] not found"));
            }
            catch (ArgumentOutOfRangeException)
            {
                return BadRequest(CreateProblemDetails(HttpStatusCode.BadRequest,
                                                       "Id is invalid"));
            }

            return Accepted();
        }
    }
}