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
using Shaos.Services.Exceptions;
using Shaos.Services.Repositories;
using Shaos.Services.Store;
using Shaos.Services.Validation;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Runtime.CompilerServices;

namespace Shaos.Controllers
{
    [Route("api/v{version:apiVersion}/plugins")]
    public class PlugInController : BasePlugInController
    {
        private const string CreateDescription = "Create a new PlugIn instance in the default state of disabled and inactive";
        private const string DeleteDescription = "Delete a PlugIn instance, the PlugIn is stopped if its currently running";
        private const string EnableDescription = "Set the state of a PlugIn, setting enabled to false prevents a PlugIn from being started at start up";
        private const string GetDescription = "Get the details of a PlugIn by its identifier";
        private const string GetListDescription = "Get a list of all PlugIns";
        private const string PlugInIdentifier = "The PlugIn identifier";
        private const string PlugInInstanceIdentifier = "The PlugIn Instance Identifier";
        private const string PluginNameExists = "A PlugIn with the same name exists";

        private readonly ICodeFileValidationService _codeFileValidationService;

        public PlugInController(
            ILogger<PlugInController> logger,
            IPlugInService plugInService,
            IPlugInRepository plugInRepository,
            IPlugInInstanceRepository plugInInstanceRepository,
            ICodeFileValidationService codeFileValidationService) : base(logger, plugInService, plugInRepository, plugInInstanceRepository)
        {
            _codeFileValidationService = codeFileValidationService ?? throw new ArgumentNullException(nameof(codeFileValidationService));
        }

        [HttpPost]
        [SwaggerResponse(StatusCodes.Status201Created, PlugInIdentifier, Type = typeof(int))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, InvalidRequest, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status409Conflict, PluginNameExists, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Create a new PlugIn",
            Description = CreateDescription,
            OperationId = "CreatePlugIn")]
        public async Task<ActionResult<int>> CreatePlugInAsync(
            [FromBody, SwaggerParameter("A PlugIn create", Required = true)] CreatePlugIn create,
            CancellationToken cancellationToken)
        {
            var id = 0;

            try
            {
                var plugInId = await PlugInRepository.CreatePlugInAsync(
                    create.ToModel(),
                    cancellationToken);

                return Ok(plugInId);
            }
            catch (PlugInNameExistsException)
            {
                return base.Conflict(CreateProblemDetails(
                    HttpStatusCode.Conflict,
                    $"A PlugIn with name [{create.Name}] already exists"));
            }
        }

        [HttpPost("{id}/instances")]
        [SwaggerResponse(StatusCodes.Status201Created, "The PlugIn Instance identifier", Type = typeof(int))]
        [SwaggerResponse(StatusCodes.Status404NotFound, PluginNotFound, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Create a new PlugIn instance",
            Description = "A PlugIn instance is created in the default mode of disabled",
            OperationId = "CreatePlugInInstance")]
        public async Task<ActionResult<int>> CreatePlugInInstanceAsync(
            [FromRoute, SwaggerParameter(PlugInIdentifier, Required = true)] int id,
            [FromBody, SwaggerParameter("The PlugIn Instance Create", Required = true)] CreatePlugInInstance create,
            CancellationToken cancellationToken)
        {
            try
            {
                var plugInId = await PlugInService.CreatePlugInInstanceAsync(
                    id,
                    create.ToModel(),
                    cancellationToken);

                return Ok(plugInId);
            }
            catch (PlugInNotFoundException ex)
            {
                return NotFound(
                    CreateProblemDetails(
                        HttpStatusCode.NotFound, ex.Message));
            }
            catch (PlugInInstanceNameExistsException)
            {
                return Conflict(
                    CreateProblemDetails(
                        HttpStatusCode.Conflict,
                        $"A PlugInInstance with name [{create.Name}] already exists"));
            }
        }

        [HttpDelete("{id}")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The PlugIn will be deleted")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "An instance of the PlugIn is running", Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Delete an existing PlugIn",
            Description = DeleteDescription,
            OperationId = "DeletePlugIn")]
        public async Task<ActionResult> DeletePlugInAsync(
            [FromRoute, SwaggerParameter(PlugInIdentifier, Required = true)] int id,
            CancellationToken cancellationToken)
        {
            try
            {
                await PlugInService.DeletePlugInAsync(id, cancellationToken);

                return Accepted();
            }
            catch (PlugInInstanceRunningException ex)
            {
                return BadRequest(
                    CreateProblemDetails(
                        HttpStatusCode.BadRequest, ex.Message));
            }
        }

        [HttpDelete("instances/{id}")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The PlugIn Instance will be deleted")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The PlugIn Instance is still running", Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Deletes a PlugIn Instance",
            Description = "Deletes a PlugIn Instance for the PlugIn",
            OperationId = "DeletePlugInInstance")]
        public async Task<ActionResult> DeletePlugInInstanceAsync(
            [FromRoute, SwaggerParameter(PlugInIdentifier, Required = true)] int id,
            CancellationToken cancellationToken)
        {
            try
            {
                await PlugInService.DeletePlugInInstanceAsync(id, cancellationToken);

                return Accepted();
            }
            catch (PlugInInstanceRunningException ex)
            {
                return BadRequest(
                    CreateProblemDetails(
                        HttpStatusCode.BadRequest, ex.Message));
            }
        }

        [HttpGet("{id}")]
        [SwaggerResponse(StatusCodes.Status200OK, "The PlugIn details", Type = typeof(PlugIn))]
        [SwaggerResponse(StatusCodes.Status404NotFound, PluginNotFound)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Get an existing PlugIn by Identifier",
            Description = GetDescription,
            OperationId = "GetPlugIn")]
        public async Task<ActionResult<PlugIn>> GetPlugInAsync(
            [FromRoute, SwaggerParameter(PlugInIdentifier, Required = true)] int id,
            CancellationToken cancellationToken)
        {
            var plugIn = await PlugInInstanceRepository.GetByIdAsync(id, includeProperties: [], cancellationToken: cancellationToken);

            if (plugIn != null)
            {
                return Ok(plugIn.ToApi());
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, "The list of PlugIns in the response", Type = typeof(IList<PlugIn>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Get a list of PlugIns",
            Description = GetListDescription,
            OperationId = "GetPlugIns")]
        public async IAsyncEnumerable<PlugIn> GetPlugInsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var plugIns = PlugInRepository
                .GetAsync(
                    includeProperties: [nameof(PlugIn.Instances), nameof(PlugIn.Package)],
                    cancellationToken: cancellationToken);

            await foreach (var item in plugIns)
            {
                yield return item.ToApi();
            }
        }

        [HttpPut("instances/{id}/enable/{state}")]
        [SwaggerResponse(StatusCodes.Status200OK, "")]
        [SwaggerResponse(StatusCodes.Status404NotFound, PluginNotFound)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Set a PlugIn Instance enabled state",
            Description = EnableDescription,
            OperationId = "SetPlugInInstanceEnableState")]
        public async Task<ActionResult> SetPlugInInstanceEnableStateAsync(
            [FromRoute, SwaggerParameter(PlugInInstanceIdentifier, Required = true)] int id,
            [FromRoute, SwaggerParameter("The PlugIn Instance state", Required = true)] bool state,
            CancellationToken cancellationToken)
        {
            try
            {
                await PlugInService.SetPlugInInstanceEnableAsync(
                    id,
                    state,
                    cancellationToken);

                return Ok();
            }
            catch (PlugInInstanceNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{id}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "The PlugIn was updated successfully")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, InvalidRequest, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status404NotFound, IdentifierNotFound)]
        [SwaggerResponse(StatusCodes.Status409Conflict, PluginNameExists, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Update a PlugIn",
            Description = "Update the properties of a PlugIn",
            OperationId = "UpdatePlugIn")]
        public async Task<ActionResult<PlugIn>> UpdatePlugInAsync(
            [FromRoute, SwaggerParameter(PlugInIdentifier, Required = true)] int id,
            [FromBody, SwaggerParameter("The PlugIn update")] UpdatePlugIn update,
            CancellationToken cancellationToken)
        {
            try
            {
                await PlugInRepository.UpdatePlugInAsync(
                    id,
                    update.Name,
                    update.Description,
                    cancellationToken);

#warning how to map the content location to correct version url
                Response.Headers.ContentLocation = new StringValues($"/api/v1/plugins/{id}");
                return NoContent();
            }
            catch (PlugInNameExistsException)
            {
                return Conflict(CreateProblemDetails(HttpStatusCode.Conflict, $"A PlugIn with name [{update.Name}] already exists"));
            }
            catch (PlugInNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("instances/{id}")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The update was accepted")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Updates a PlugIn Instance",
            Description = "Update a PlugIn Instance",
            OperationId = "UpdatePlugInInstance")]
        public async Task<ActionResult> UpdatePlugInInstanceAsync(
            [FromRoute, SwaggerParameter(PlugInInstanceIdentifier, Required = true)] int id,
            [FromBody, SwaggerParameter("The PlugIn update")] UpdatePlugInInstance update,
            CancellationToken cancellationToken)
        {
            try
            {
                await PlugInInstanceRepository.UpdatePlugInInstanceAsync(
                    id,
                    update.Name,
                    update.Description,
                    cancellationToken);

                return Accepted();
            }
            catch (PlugInInstanceNameExistsException)
            {
                return Conflict(
                    CreateProblemDetails(
                        HttpStatusCode.Conflict, $"A PlugInInstance with name [{update.Name}] already exists"));
            }
            catch (PlugInInstanceNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{id}/upload")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The PlugIn package is uploaded, extracted and verified", Type = typeof(UploadPackageResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Indicates if there was a problem with the upload file", Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status404NotFound, IdentifierNotFound, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Upload a Package for a PlugIn",
            Description = "Upload a Package file for a PlugIn. A Package is a zip file that contains the executables and content for a PlugIn",
            OperationId = "UploadPlugInPackage")]
        public async Task<ActionResult> UploadPlugInPackageAsync(
            [FromRoute, SwaggerParameter(PlugInIdentifier, Required = true)] int id,
            IFormFile formFile,
            CancellationToken cancellationToken)
        {
            if (ValidateFormFile(formFile, out var problemDetails))
            {
                var fileName = Path.GetFileName(formFile.FileName);

                Logger.LogDebug("Uploading File: [{FileName}] to PlugIn Id: [{Id}]", fileName, id);

                try
                {
                    return Accepted(await PlugInService.UploadPlugInPackageAsync(
                        id,
                        fileName,
                        formFile.OpenReadStream(),
                        cancellationToken));
                }
                catch (PlugInNotFoundException ex)
                {
                    return NotFound(
                        CreateProblemDetails(
                            HttpStatusCode.NotFound,
                            ex.Message));
                }
            }
            else
            {
                return BadRequest(problemDetails);
            }
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

        private bool ValidateFormFile(IFormFile formFile, out ProblemDetails? problemDetails)
        {
            problemDetails = null;

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

            return problemDetails == null;
        }
    }
}