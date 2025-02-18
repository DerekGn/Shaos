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
using Shaos.Api.Model.v1;
using Shaos.Extensions;
using Shaos.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Shaos.Controllers
{
    [Route("api/v{version:apiVersion}/plugins")]
    public class PlugInController : CoreController
    {
        private readonly IPlugInService _plugInService;

        public PlugInController(
            ILogger<PlugInController> logger,
            IPlugInService plugInService) : base(logger)
        {
            _plugInService = plugInService ?? throw new ArgumentNullException(nameof(plugInService));
        }

        [HttpPost]
        [SwaggerResponse(StatusCodes.Status201Created, "The PlugIn was created", Type = typeof(int))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Indicates that the request syntax is invalid", Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A PlugIn with the same name exists", Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Create a new PlugIn", Description = "", OperationId = "CreatePlugIn")]
        public async Task<ActionResult<int>> CreatePlugInAsync(
            PlugInCreate plugInCreate,
            CancellationToken cancellationToken)
        {
            if (await _plugInService.GetPlugInByNameAsync(plugInCreate.Name, cancellationToken) != null)
            {
                return Conflict(new ProblemDetails()
                {
                    Title = "Conflict",
                    Detail = $"A PlugIn with name {plugInCreate.Name} already exists",
                    Status = (int?)HttpStatusCode.Conflict,
                    Type = HttpStatusCode.Conflict.MapToType()
                });
            }
            else
            {
                var id = await _plugInService.CreatePlugInAsync(
                    plugInCreate.Name, plugInCreate.Description, plugInCreate.Code);

                return Ok(id);
            }
        }

        [HttpGet("{id}")]
        [SwaggerResponse(StatusCodes.Status200OK, "", Type = typeof(PlugIn))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The PlugIn could not be found")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Get an existing PlugIn by Identifier", Description = "", OperationId = "GetPlugIn")]
        public async Task<ActionResult<PlugIn>> GetPlugInAsync(
            [FromRoute, SwaggerParameter("The plug in identifier to retrieve", Required = true)] int id,
            CancellationToken cancellationToken)
        {
            var plugin = await _plugInService.GetPlugInByIdAsync(id, cancellationToken);

            if(plugin == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(plugin);
            }
        }

        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, "", Type = typeof(IList<PlugIn>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Get a list of all configured PlugIns", Description = "", OperationId = "GetPlugIns")]
        public IAsyncEnumerable<PlugIn> GetPlugInsAsync(CancellationToken cancellationToken)
        {
             return _plugInService.GetPlugInsAsync();
        }

        [HttpPut("start/{id}")]
        public ActionResult StartPlugIn()
        {
            return new OkResult();
        }

        [HttpPut("stop/{id}")]
        public ActionResult StopPlugIn()
        {
            return new OkResult();
        }

        [HttpPut("{id}")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The PlugIn was updated successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A PlugIn with identifier was not found")]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A PlugIn with the same name exists")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Update a PlugIn", Description = "", OperationId = "UpdatePlugIn")]
        public ActionResult UpdatePlugInAsync(
            [FromRoute, SwaggerParameter("The PlugIn identifier to update", Required = true)] int id,
            [FromBody, SwaggerParameter("The PlugIn update")] PlugInUpdate plugInUpdate,
            CancellationToken cancellationToken)
        {
            return new OkResult();
        }
    }
}