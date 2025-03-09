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


// Ignore Spelling: Nuget

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Shaos.Api.Model.v1;
using Shaos.Extensions;
using Shaos.Services;
using Shaos.Services.Validation;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Runtime.CompilerServices;

using CreatePlugInInstanceApi = Shaos.Api.Model.v1.CreatePlugInInstance;
using UpdatePlugInApi = Shaos.Api.Model.v1.UpdatePlugIn;
using UpdatePlugInInstanceApi = Shaos.Api.Model.v1.UpdatePlugInInstance;

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
            ICodeFileValidationService codeFileValidationService) : base(logger, plugInService)
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
            [FromBody, SwaggerParameter("A PlugIn create", Required = true)] Api.Model.v1.CreatePlugIn create,
            CancellationToken cancellationToken)
        {
            if (await PlugInService.GetPlugInByNameAsync(create.Name, cancellationToken) != null)
            {
                return base.Conflict(CreateProblemDetails(
                    HttpStatusCode.Conflict,
                    $"A PlugIn with name [{create.Name}] already exists"));
            }
            else
            {
                return Ok(await PlugInService.CreatePlugInAsync(
                    create.ToModel(),
                    cancellationToken));
            }
        }

        [HttpPost("{id}/instances")]
        [SwaggerResponse(StatusCodes.Status201Created, "The PlugIn Instance identifier", Type = typeof(int))]
        [SwaggerResponse(StatusCodes.Status404NotFound, PluginNotFound)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Create a new PlugIn instance",
            Description = "A PlugIn instance is created in the default mode of disabled",
            OperationId = "CreatePlugInInstance")]
        public async Task<ActionResult<int>> CreatePlugInInstanceAsync(
            [FromRoute, SwaggerParameter(PlugInIdentifier, Required = true)] int id,
            [FromBody, SwaggerParameter("The PlugIn Instance Create", Required = true)] CreatePlugInInstanceApi create,
            CancellationToken cancellationToken)
        {
            return await GetPlugInOperationAsync(id, async (plugIn, CancellationToken) =>
            {
                if (await PlugInService.GetPlugInInstanceByNameAsync(create.Name, cancellationToken) != null)
                {
                    return base.Conflict(CreateProblemDetails(
                        HttpStatusCode.Conflict,
                        $"A PlugIn Instance with name [{create.Name}] already exists"));
                }
                else
                {
#warning path or url
                    return new CreatedResult
                    (
                        "",
                        await PlugInService.CreatePlugInInstanceAsync(
                            plugIn.Id,
                            create.ToModel(),
                            cancellationToken)
                    );
                }
            },
            cancellationToken);
        }

        [HttpDelete("{id}")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The PlugIn will be deleted")]
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
            await PlugInService.DeletePlugInAsync(id, cancellationToken);

            return Accepted();
        }

        [HttpDelete("instances/{id}")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The PlugIn Instance will be deleted")]
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
            return await GetPlugInOperationAsync(id, async (plugIn, CancellationToken) =>
            {
                await PlugInService.DeletePlugInInstanceAsync(id, cancellationToken);

                return Accepted();
            },
            cancellationToken);
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
            return await GetPlugInOperationAsync(id, (plugIn) =>
            {
                return Ok(plugIn.ToApiModel());
            },
            cancellationToken);
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
            await foreach (var item in PlugInService.GetPlugInsAsync(cancellationToken))
            {
                yield return item.ToApiModel();
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
            await PlugInService.SetPlugInInstanceEnableAsync(
                id,
                state,
                cancellationToken);

            return Ok();
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
            [FromBody, SwaggerParameter("The PlugIn update")] UpdatePlugInApi update,
            CancellationToken cancellationToken)
        {
            var plugIn = await PlugInService.GetPlugInByNameAsync(update.Name, cancellationToken);

            if ((plugIn != null) && plugIn.Id != id)
            {
                return Conflict(CreateProblemDetails(HttpStatusCode.Conflict, $"A PlugIn with name [{update.Name}] already exists"));
            }
            else
            {
                var updated = await PlugInService.UpdatePlugInAsync(
                    id,
                    update.ToModel(),
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
            [FromBody, SwaggerParameter("The PlugIn update")] UpdatePlugInInstanceApi update,
            CancellationToken cancellationToken)
        {
            await PlugInService.UpdatePlugInInstanceAsync(
                id,
                update.ToModel(),
                cancellationToken);

            return Accepted();
        }

        [HttpPut("{id}/nuget")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "The PlugIn nuget was accepted")]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status404NotFound, IdentifierNotFound)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "",
            Description = "",
            OperationId = "UploadPlugInNuget")]
        public async Task<ActionResult> UploadPlugInNugetAsync(
            [FromRoute, SwaggerParameter(PlugInIdentifier, Required = true)] int id,
            IFormFile formFile,
            CancellationToken cancellationToken)
        {
            return await GetPlugInOperationAsync(id, async (plugIn, cancellationToken) =>
            {
                if(ValidateFormFile(formFile,out var problemDetails))
                {
                    var fileName = Path.GetFileName(formFile.FileName);

                    Logger.LogDebug("Uploading File: [{FileName}] to PlugIn Id: [{Id}] Name: [{Name}]", fileName, plugIn.Id, plugIn.Name);

                    await PlugInService.CreatePlugInNugetAsync(
                        plugIn.Id,
                        fileName,
                        formFile.OpenReadStream(),
                        cancellationToken);

                    return Accepted();
                }
                else
                {
                    return BadRequest(problemDetails);
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