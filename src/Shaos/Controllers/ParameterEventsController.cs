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
using Shaos.Services.Eventing.Parameter;
using System.Net.ServerSentEvents;

namespace Shaos.Controllers
{
    [Route("api/v{version:apiVersion}/parameter-events")]
    public class ParameterEventsController : BaseEventsController
    {
        public ParameterEventsController(ILogger<ParameterEventsController> logger,
                                         IHttpContextAccessor httpContextAccessor,
                                         IServerSentEventsService serverSentEventsService) : base(logger,
                                                                                                  httpContextAccessor,
                                                                                                  serverSentEventsService)
        {
        }

        [HttpGet]
        [EndpointDescription("Gets a stream of application parameter events")]
        [EndpointName("StreamParameters")]
        [EndpointSummary("Access application parameter event stream")]
        [ProducesResponseType(typeof(SseItem<BaseParameterUpdatedEvent>), StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, Description = Status400BadRequestText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public IResult StreamParameterEventsAsync(CancellationToken cancellationToken)
        {
            return ExecuteSubscription((context, cancellationToken) =>
            {
                Logger.EventParameterStreamingStarted(context.Connection.Id);

                return TypedResults.ServerSentEvents<BaseParameterUpdatedEvent>(ServerSentEventsService.StreamParameterEventsAsync(cancellationToken));
            },
            cancellationToken);
        }

        [HttpGet("{id:int}")]
        [EndpointDescription("Gets a stream of application parameter events for a specific parameter")]
        [EndpointName("StreamParametersById")]
        [EndpointSummary("Access application parameter event stream by identifier")]
        [ProducesResponseType(typeof(SseItem<BaseParameterUpdatedEvent>), StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, Description = Status400BadRequestText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public IResult StreamByIdAsync(int id,
                                       CancellationToken cancellationToken)
        {
            return ExecuteSubscription((context, cancellationToken) =>
            {
                Logger.EventParameterByIdStreamingStarted(context.Connection.Id,
                                                          id);

                return TypedResults.ServerSentEvents<BaseParameterUpdatedEvent>(ServerSentEventsService.StreamParameterEventsByIdAsync(id,
                                                                                                                                       cancellationToken));
            },
            cancellationToken);
        }
    }
}