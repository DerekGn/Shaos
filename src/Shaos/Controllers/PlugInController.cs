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
        private const string GetListDescription = "Get a list of all PlugIns";
        private const string IdentifierNotFound = "A PlugIn with identifier was not found";
        private const string PluginNameExists = "A PlugIn with the same name exists";
        private const string PluginNotFound = "The PlugIn could not be found";
        private const string PlugInRetrieve = "The PlugIn identifier to retrieve";

        private readonly ICodeFileValidationService _codeFileValidationService;
        private readonly IPlugInService _plugInService;

        public PlugInController(
            ILogger<PlugInController> logger,
            IPlugInService plugInService,
            ICodeFileValidationService codeFileValidationService) : base(logger)
        {
            _plugInService = plugInService ?? throw new ArgumentNullException(nameof(plugInService));
            _codeFileValidationService = codeFileValidationService ?? throw new ArgumentNullException(nameof(codeFileValidationService));
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
                return base.Conflict(CreateProblemDetails(
                    HttpStatusCode.Conflict,
                    $"A PlugIn with name [{create.Name}] already exists"));
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
            return await GetPlugInOperationAsync(id, async (plugIn) =>
            {
                return Ok(plugIn);
            },
             cancellationToken);
        }

        [HttpGet("{id}/codefiles")]
        [SwaggerResponse(StatusCodes.Status200OK, "The set of PlugIn code files in the response", Type = typeof(PlugIn))]
        [SwaggerResponse(StatusCodes.Status404NotFound, PluginNotFound)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Get an existing PlugIn's code files", Description = "", OperationId = "GetPlugInCodeFiles")]
        public async Task<ActionResult<IAsyncEnumerator<CodeFile>>> GetPlugInCodeFilesAsync(
            [FromRoute, SwaggerParameter("The PlugIn identifier to get its code files", Required = true)] int id,
            CancellationToken cancellationToken)
        {
            return await GetPlugInOperationAsync(id, async (plugIn) =>
            {
                return Ok(plugIn);
            },
            cancellationToken);
        }

        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, "The list of PlugIns in the response", Type = typeof(IList<PlugIn>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Get a list of PlugIns", Description = GetListDescription, OperationId = "GetPlugIns")]
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

            if (pluginStatus == null)
            {
                return NotFound();
            }
            else
            {
                return new OkObjectResult(pluginStatus);
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
            return await GetPlugInOperationAsync(id, async (plugIn) =>
            {
                await _plugInService.SetPlugInEnabledStateAsync(
                    id,
                    state,
                    cancellationToken);

                return Ok(plugIn);
            },
            cancellationToken);
        }

        [HttpPut("{id}/start")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The PlugIn will be started", Type = typeof(PlugIn))]
        [SwaggerResponse(StatusCodes.Status404NotFound, PluginNotFound)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Start a PlugIn", Description = "Start a PlugIn running", OperationId = "StartPlugIn")]
        public async Task<ActionResult> StartPlugIn(
            [FromRoute, SwaggerParameter("The PlugIn identifier to start", Required = true)] int id,
            CancellationToken cancellationToken)
        {
            return await GetPlugInOperationAsync(id, async (plugIn) =>
            {
                await _plugInService.StartPlugInAsync(id, cancellationToken);

                return Accepted();
            },
            cancellationToken);
        }

        [HttpPut("{id}/stop")]
        [SwaggerResponse(StatusCodes.Status200OK, "A PlugIn will be stopped")]
        [SwaggerResponse(StatusCodes.Status404NotFound, PluginNotFound)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(Summary = "Stop a PlugIn", Description = "Stop a PlugIn running", OperationId = "StopPlugIn")]
        public async Task<ActionResult> StopPlugIn(
            [FromRoute, SwaggerParameter("The PlugIn identifier to stop", Required = true)] int id,
            CancellationToken cancellationToken)
        {
            return await GetPlugInOperationAsync(id, async (plugIn) =>
            {
                await _plugInService.StopPlugInAsync(id, cancellationToken);

                return Accepted();
            },
            cancellationToken);
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
                return Conflict(CreateProblemDetails(HttpStatusCode.Conflict, $"A PlugIn with name [{update.Name}] already exists"));
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

        [HttpPut("{id}/codefiles")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The PlugIn code files where accepted")]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status404NotFound, IdentifierNotFound)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Upload a set of PlugIn code files",
            Description = "Upload a set of PlugIn code files. Existing files will be over written, new files will be created",
            OperationId = "UploadPlugInCodeFiles")]
        public async Task<ActionResult> UploadPlugInCodesFileAsync(
            [FromRoute, SwaggerParameter("The PlugIn identifier to upload code files", Required = true)] int id,
            List<IFormFile> files,
            CancellationToken cancellationToken)
        {
            return await GetPlugInOperationAsync(id, async (plugIn) =>
            {
                ProblemDetails? problemDetails = null;

                foreach (var formFile in files)
                {
                    var validationResult = _codeFileValidationService.ValidateFile(formFile);

                    if (validationResult == FileValidationResult.FileNameEmpty)
                    {
                        problemDetails = CreateProblemDetails(HttpStatusCode.BadRequest, $"File name is empty");
                    }
                    else if (validationResult == FileValidationResult.InvalidContentType)
                    {
                        problemDetails = CreateProblemDetails(HttpStatusCode.BadRequest, $"File: [{formFile.Name}] invalid content type");
                    }
                    else if (validationResult == FileValidationResult.InvalidFileLength)
                    {
                        problemDetails = CreateProblemDetails(HttpStatusCode.BadRequest, $"File: [{formFile.Name}] has invalid length");
                    }
                    else if (validationResult == FileValidationResult.InvalidFileName)
                    {
                        problemDetails = CreateProblemDetails(HttpStatusCode.BadRequest, $"File: [{formFile.Name}] has invalid type");
                    }
                    else
                    {
                        var fileName = Path.GetFileName(formFile.FileName);

                        Logger.LogDebug("Uploading File: [{FileName}] to PlugIn Id: [{Id}] Name: [{Name}]", fileName, plugIn.Id, plugIn.Name);

                        await _plugInService.UploadPlugInCodeFileAsync(
                            plugIn.Id,
                            fileName,
                            formFile.OpenReadStream(),
                            cancellationToken);
                    }
                }

                if (problemDetails != null)
                {
                    return BadRequest(problemDetails);
                }
                else
                {
                    return Accepted();
                }
            },
            cancellationToken);
        }

        private static ProblemDetails CreateProblemDetails(
            HttpStatusCode statusCode,
            string details)
        {
            return new ProblemDetails()
            {
                Title = statusCode.ToString(),
                Detail = details,
                Status = (int?)statusCode,
                Type = statusCode.MapToType()
            };
        }

        private async Task<ActionResult> GetPlugInOperationAsync(
            int id,
            Func<PlugIn, Task<ActionResult>> operation,
            CancellationToken cancellationToken)
        {
            var plugIn = await _plugInService.GetPlugInByIdAsync(
                id,
                cancellationToken);

            if (plugIn == null)
            {
                return NotFound();
            }
            else
            {
                return await operation(plugIn);
            }
        }
    }
}