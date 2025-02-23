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
        private const string CreateDescription = "Create a new PlugIn instance in the default states of disabled and inactive";
        private const string DeleteDescription = "Delete a PlugIn instance, the PlugIn is stopped if its currently running";
        private const string EnableDescription = "Set the state of a PlugIn, setting enabled to false prevents a PlugIn from being started at start up";
        private const string GetDescription = "Get the details of a PlugIn by its identifier";
        private const string GetListDescription = "Get the details of all configured PlugIns";
        private const string IdentifierNotFound = "A PlugIn with identifier was not found";
        private const string PluginNameExists = "A PlugIn with the same name exists";
        private const string PluginNotFound = "The PlugIn could not be found";
        private const string PlugInRetrieve = "The PlugIn identifier to retrieve";

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
        [SwaggerOperation(Summary = "Create a new PlugIn", Description = CreateDescription, OperationId = "CreatePlugIn")]
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
                    cancellationToken));
            }
        }

        [HttpDelete("{id}")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The PlugIn will be deleted")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Delete an existing PlugIn", Description = DeleteDescription, OperationId = "DeletePlugIn")]
        public async Task<ActionResult> DeletePlugInAsync(
            [FromRoute, SwaggerParameter("The PlugIn identifier to delete", Required = true)] int id,
            CancellationToken cancellationToken)
        {
            await _plugInService.DeletePlugInAsync(id, cancellationToken);

            return Accepted();
        }

        [HttpGet("{id}")]
        [SwaggerResponse(StatusCodes.Status200OK, "The PlugIn details in the response", Type = typeof(PlugIn))]
        [SwaggerResponse(StatusCodes.Status404NotFound, PluginNotFound)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Get an existing PlugIn by Identifier", Description = GetDescription, OperationId = "GetPlugIn")]
        public async Task<ActionResult<PlugIn>> GetPlugInAsync(
            [FromRoute, SwaggerParameter(PlugInRetrieve, Required = true)] int id,
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
        [SwaggerResponse(StatusCodes.Status200OK, "The list of PlugIns in the response", Type = typeof(IList<PlugIn>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Get a list of all configured PlugIns", Description = GetListDescription, OperationId = "GetPlugIns")]
        public IAsyncEnumerable<PlugIn> GetPlugInsAsync(CancellationToken cancellationToken)
        {
            return _plugInService.GetPlugInsAsync(cancellationToken);
        }

        [HttpGet("{id}/status")]
        [SwaggerResponse(StatusCodes.Status200OK, "The PlugIn status", Type = typeof(PlugInStatus))]
        [SwaggerResponse(StatusCodes.Status404NotFound, PluginNotFound)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Get a PlugIn status", Description = GetListDescription, OperationId = "GetPlugInStatus")]
        public async Task<ActionResult<PlugInStatus>> GetPlugInStatusAsync(
            [FromRoute, SwaggerParameter(PlugInRetrieve, Required = true)] int id,
            CancellationToken cancellationToken)
        {
            var pluginStatus = await _plugInService.GetPlugInStatusAsync(id, cancellationToken);

            if(pluginStatus == null)
            {
                return NotFound();
            }
            else
            {
                return new OkObjectResult(pluginStatus);
            }
        }

        [HttpPut("{id}/enable/{state}")]
        [SwaggerResponse(StatusCodes.Status200OK, "The PlugIn enabled state will be set to state", Type = typeof(PlugIn))]
        [SwaggerResponse(StatusCodes.Status404NotFound, PluginNotFound)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Set the state of an existing PlugIn by identifier", Description = EnableDescription, OperationId = "SetPlugInState")]
        public async Task<ActionResult> SetEnablePlugInAsync(
            [FromRoute, SwaggerParameter("The PlugIn identifier to set its state", Required = true)] int id,
            [FromRoute, SwaggerParameter("The PlugIn state", Required = true)] bool state,
            CancellationToken cancellationToken)
        {
            var plugIn = await _plugInService.GetPlugInByIdAsync(id, cancellationToken);

            if(plugIn == null)
            {
                return NotFound();
            }
            else
            {
                await _plugInService.SetPlugInEnabledStateAsync(
                    id,
                    state,
                    cancellationToken);

                return Ok(plugIn);
            }
        }

        [HttpPut("{id}/start")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The PlugIn will be started", Type = typeof(PlugIn))]
        [SwaggerResponse(StatusCodes.Status404NotFound, PluginNotFound)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Start a PlugIn", Description = EnableDescription, OperationId = "StartPlugIn")]
        public async Task<ActionResult> StartPlugIn(
            [FromRoute, SwaggerParameter("The PlugIn identifier to start", Required = true)] int id,
            CancellationToken cancellationToken)
        {
            var plugin = await _plugInService.GetPlugInByIdAsync(id, cancellationToken);

            if(plugin == null)
            {
                return NotFound();
            }
            else
            {
                await _plugInService.StartPlugInAsync(id, cancellationToken);

                return Accepted();
            }
        }

        [HttpPut("{id}/stop")]
        [SwaggerResponse(StatusCodes.Status200OK, "A PlugIn will be stopped")]
        [SwaggerResponse(StatusCodes.Status404NotFound, PluginNotFound)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Stop a PlugIn", Description = EnableDescription, OperationId = "StopPlugInState")]
        public async Task<ActionResult> StopPlugIn(
            [FromRoute, SwaggerParameter("The PlugIn identifier to stop", Required = true)] int id,
            CancellationToken cancellationToken)
        {
            var plugin = await _plugInService.GetPlugInByIdAsync(id, cancellationToken);

            if(plugin == null)
            {
                return NotFound();
            }
            else
            {
                await _plugInService.StopPlugInAsync(id, cancellationToken);

                return Accepted();
            }
        }

        [HttpGet("statuses")]
        [SwaggerResponse(StatusCodes.Status200OK, "The list of PlugInStatuses for loaded PlugIns", Type = typeof(IList<PlugInStatus>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "The list of PlugInStatus for all loaded and running PlugIn", Description = EnableDescription, OperationId = "SetPlugInState")]
        public IAsyncEnumerable<PlugInStatus> GetPlugInStatuses(CancellationToken cancellationToken)
        {
            return _plugInService.GetPlugInStatusesAsync(cancellationToken);
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

        [HttpPut("{id}/files/{fileId}")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The PlugIn code file updated was accepted")]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Update a PlugIn code file", Description = "Update a PlugIn code file", OperationId = "UpdatePlugInCodeFile")]
        public async Task<ActionResult> UpdatePlugInCodeFileAsync(
            [FromRoute, SwaggerParameter("The PlugIn identifier to update", Required = true)] int id,
            [FromRoute, SwaggerParameter("The PlugIn file identifier to update", Required = true)] int fileId,
            IFormFile file,
            CancellationToken cancellationToken)
        {
            var plugIn = await _plugInService.GetPlugInByIdAsync(id, cancellationToken);

            if(plugIn == null)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpPost("{id}/files")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The PlugIn code file updated was accepted")]
        [SwaggerResponse(StatusCodes.Status404NotFound, IdentifierNotFound)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Create a PlugIn code file", Description = "Create a PlugIn code file", OperationId = "CreatePlugInCodeFile")]
        public async Task<ActionResult> CreatePlugInCodeFileAsync(
            [FromRoute, SwaggerParameter("The PlugIn identifier to create a code file", Required = true)] int id,
            IFormFile file,
            CancellationToken cancellationToken)
        {
            var plugIn = await _plugInService.GetPlugInByIdAsync(id, cancellationToken);

            if(plugIn == null)
            {
                return NotFound();
            }
            else
            {

            }

            return Ok();
        }

        public async Task<ActionResult> GetPlugInCodeFileAsync(
            [FromRoute, SwaggerParameter("The PlugIn identifier to create a code file", Required = true)] int id,
            [FromRoute, SwaggerParameter("The PlugIn code file identifier", Required = true)] int fileId,
            CancellationToken cancellationToken)
        {

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