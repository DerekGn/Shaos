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
using Shaos.Services.Eventing;
using System.Net;
using System.Net.ServerSentEvents;

namespace Shaos.Controllers
{
    [Route("api/v{version:apiVersion}/events")]
    public class EventsController : CoreController
    {
        private readonly ILogger<EventsController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServerSideEventsService _serverSideEventsService;

        public EventsController(ILogger<EventsController> logger,
                                IHttpContextAccessor httpContextAccessor,
                                IServerSideEventsService serverSideEventsService) : base(logger)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _serverSideEventsService = serverSideEventsService;
        }

        [HttpGet()]
        [EndpointDescription("Gets a stream of application events")]
        [EndpointName("StreamEvents")]
        [EndpointSummary("Access application event stream")]
        [ProducesResponseType(typeof(SseItem<BaseEvent>), StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, Description = Status400BadRequestText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public IResult StreamEventsAsync(CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;

            try
            {
                if (context is not null)
                {
                    _logger.EventStreamingStarted(context.Connection.Id);

                    return TypedResults.ServerSentEvents<BaseEvent>(_serverSideEventsService.StreamEventsAsync(cancellationToken));
                }
                else
                {
                    _logger.HttpContextNull();

                    return TypedResults.BadRequest(new ProblemDetails()
                    {
                        Status = (int)HttpStatusCode.BadRequest,
                        Title = "HttpContext is null",
                        Detail = "The request cannot be processed because the HttpContext is null."
                    });
                }
            }
            catch (Exception exception)
            {
                _logger.LogUnhandledException(exception);

                return TypedResults.Problem(new ProblemDetails()
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "An error occurred while processing the events stream",
                    Detail = "An unexpected error occurred while processing the events stream. Please try again later."
                });
            }
        }
    }
}