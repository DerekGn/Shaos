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
using Microsoft.Extensions.Primitives;
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
        private const string IdentifierNotFound = "A PlugIn with identifier was not found";
        private const string PluginNameExists = "A PlugIn with the same name exists";
        private const string PluginNotFound = "The PlugIn could not be found";

        private readonly IPlugInService _plugInService;

        public PlugInController(
            ILogger<PlugInController> logger,
            IPlugInService plugInService) : base(logger)
        {
            _plugInService = plugInService ?? throw new ArgumentNullException(nameof(plugInService));
        }

        [HttpPost]
        [SwaggerResponse(StatusCodes.Status201Created, "The PlugIn was created", Type = typeof(int))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, InvalidRequest, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, PluginNameExists, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Create a new PlugIn", Description = "Create a new PlugIn instance", OperationId = "CreatePlugIn")]
        public async Task<ActionResult<int>> CreatePlugInAsync(
            PlugInCreate create,
            CancellationToken cancellationToken)
        {
            if (await _plugInService.GetPlugInByNameAsync(create.Name, cancellationToken) != null)
            {
                return base.Conflict(CreateProblemDetails(create.Name));
            }
            else
            {
                return Ok(await _plugInService.CreatePlugInAsync(
                    create.Name,
                    create.Description,
                    create.Code));
            }
        }

        [HttpDelete("{id}")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The PlugIn will be deleted")]
        [SwaggerOperation(Summary = "Delete a new PlugIn", Description = "Delete a PlugIn instance", OperationId = "DeletePlugIn")]
        public async Task<ActionResult> DeletePluginAsync(
            [FromRoute, SwaggerParameter("The PlugIn identifier to delete", Required = true)] int id,
            CancellationToken cancellationToken)
        {
            return new OkResult();
        }

        [HttpGet("{id}")]
        [SwaggerResponse(StatusCodes.Status200OK, "", Type = typeof(PlugIn))]
        [SwaggerResponse(StatusCodes.Status404NotFound, PluginNotFound)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Get an existing PlugIn by Identifier", Description = "", OperationId = "GetPlugIn")]
        public async Task<ActionResult<PlugIn>> GetPlugInAsync(
            [FromRoute, SwaggerParameter("The plug in identifier to retrieve", Required = true)] int id,
            CancellationToken cancellationToken)
        {
            var plugin = await _plugInService.GetPlugInByIdAsync(id, cancellationToken);

            if (plugin == null)
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
        [SwaggerResponse(StatusCodes.Status204NoContent, "The PlugIn was updated successfully")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, InvalidRequest, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status404NotFound, IdentifierNotFound)]
        [SwaggerResponse(StatusCodes.Status409Conflict, PluginNameExists, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Update a PlugIn", Description = "Update the properties of a PlugIn", OperationId = "UpdatePlugIn")]
        public async Task<ActionResult<PlugIn>> UpdatePlugInAsync(
            [FromRoute, SwaggerParameter("The PlugIn identifier to update", Required = true)] int id,
            [FromBody, SwaggerParameter("The PlugIn update")] PlugInUpdate update,
            CancellationToken cancellationToken)
        {
            var plugIn = await _plugInService.GetPlugInByNameAsync(update.Name, cancellationToken);

            if ((plugIn != null) && plugIn.Id != id)
            {
                return Conflict(CreateProblemDetails(update.Name));
            }
            else
            {
                var updated = await _plugInService.UpdatePlugInAsync(
                    id,
                    update.Name,
                    update.Description,
                    update.Code,
                    cancellationToken);

                if (updated == null)
                {
                    return NotFound();
                }
                else
                {
#warning how to map the content location to correct version url
                    Response.Headers.ContentLocation = new StringValues($"/api/v1/plugins/{id}");
                    return NoContent();
                }
            }
        }

        private static ProblemDetails CreateProblemDetails(string name)
        {
            return new ProblemDetails()
            {
                Title = "Conflict",
                Detail = $"A PlugIn with name {name} already exists",
                Status = (int?)HttpStatusCode.Conflict,
                Type = HttpStatusCode.Conflict.MapToType()
            };
        }
    }
}