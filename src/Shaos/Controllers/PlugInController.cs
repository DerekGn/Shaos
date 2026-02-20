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
using Shaos.Repository;
using Shaos.Repository.Exceptions;
using Shaos.Sdk;
using Shaos.Services;
using Shaos.Services.Exceptions;
using Shaos.Services.Runtime.Exceptions;
using Shaos.Services.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.CompilerServices;

using ModelPlugIn = Shaos.Repository.Models.PlugIn;

namespace Shaos.Controllers
{
    [Route("api/v{version:apiVersion}/plugins")]
    public class PlugInController : BasePlugInController
    {
        private const string CreatePlugInDescription = "Create a new PlugIn in the default state of disabled and inactive";
        private const string CreatePlugInInstanceDescription = "A PlugIn instance is created in the default mode of disabled";
        private const string DeletePlugInDescription = "Delete a PlugIn instance, the PlugIn is stopped if its currently running";
        private const string DeletePlugInInstanceDescription = "Deletes a PlugIn Instance for the PlugIn";
        private const string EnablePlugInInstanceDescription = "Set the state of a PlugIn Instance, setting enabled to false prevents a PlugIn from being started at start up";
        private const string GetListPlugInDescription = "Get a list of all PlugIns";
        private const string GetPlugInDescription = "Get the details of a PlugIn by its identifier";
        private const string PlugInIdentifier = "The PlugIn identifier";
        private const string PlugInInstanceIdentifier = "The PlugIn Instance Identifier";
        private const string PluginNameExists = "A PlugIn with the same name exists";
        private const string UpdatePlugInDescription = "Update a PlugIn";
        private const string UpdatePlugInInstanceDescription = "Update a PlugIn Instance";
        private const string UploadPackageDescription = "Upload a Package file for a PlugIn. A Package is a zip file that contains the executable and content for a PlugIn";
        private readonly IZipFileValidationService _codeFileValidationService;

        public PlugInController(ILogger<PlugInController> logger,
                                IShaosRepository repository,
                                IPlugInService plugInService,
                                IZipFileValidationService codeFileValidationService) : base(logger, repository, plugInService)
        {
            _codeFileValidationService = codeFileValidationService ?? throw new ArgumentNullException(nameof(codeFileValidationService));
        }

        [HttpPost]
        [EndpointDescription(CreatePlugInDescription)]
        [EndpointName("CreatePlugIn")]
        [EndpointSummary("Create a new PlugIn")]
        [ProducesResponseType<int>(StatusCodes.Status201Created, Description = PlugInIdentifier)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, Description = InvalidRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, Description = PluginNameExists)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public async Task<ActionResult<int>> CreatePlugInAsync([FromBody, Required, Description("A PlugIn create")] CreatePlugIn create,
                                                               CancellationToken cancellationToken = default)
        {
            try
            {
                var plugInId = await Repository.CreatePlugInAsync(create.ToModel(),
                                                                  cancellationToken);

                return Ok(plugInId);
            }
            catch (NameExistsException)
            {
                return base.Conflict(CreateProblemDetails(HttpStatusCode.Conflict,
                                                          $"A PlugIn with name [{create.Name}] already exists"));
            }
        }

        [HttpPost("{id}/instances")]
        [EndpointDescription(CreatePlugInInstanceDescription)]
        [EndpointName("CreatePlugInInstance")]
        [EndpointSummary("Create a new PlugIn Instance")]
        [ProducesResponseType<int>(StatusCodes.Status201Created, Description = "The PlugIn Instance identifier")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, Description = PluginNotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public async Task<ActionResult<int>> CreatePlugInInstanceAsync([FromRoute, Required, Description(PlugInIdentifier), Range(1, int.MaxValue)] int id,
                                                                       [FromBody, Required, Description("The PlugIn Instance Create")] CreatePlugInInstance create,
                                                                       CancellationToken cancellationToken = default)
        {
            try
            {
                var plugInId = await PlugInService.CreatePlugInInstanceAsync(id,
                                                                             create.ToModel(),
                                                                             cancellationToken);

                return Ok(plugInId);
            }
            catch (NotFoundException ex)
            {
                return NotFound(CreateProblemDetails(HttpStatusCode.NotFound,
                                                     ex.Message));
            }
            catch (NameExistsException)
            {
                return Conflict(CreateProblemDetails(HttpStatusCode.Conflict,
                                                     $"A PlugInInstance with name [{create.Name}] already exists"));
            }
        }

        [HttpDelete("{id}")]
        [EndpointDescription(DeletePlugInDescription)]
        [EndpointName("DeletePlugIn")]
        [EndpointSummary("Delete an existing PlugIn")]
        [ProducesResponseType(StatusCodes.Status202Accepted, Description = "The PlugIn will be deleted")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, Description = "An instance of the PlugIn is running")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public async Task<ActionResult> DeletePlugInAsync([FromRoute, Required, Description(PlugInIdentifier), Range(1, int.MaxValue)] int id,
                                                          CancellationToken cancellationToken = default)
        {
            try
            {
                await PlugInService.DeletePlugInAsync(id, cancellationToken);

                return Accepted();
            }
            catch (PlugInInstanceRunningException ex)
            {
                return BadRequest(CreateProblemDetails(HttpStatusCode.BadRequest,
                                                       ex.Message));
            }
        }

        [HttpDelete("instances/{id}")]
        [EndpointDescription(DeletePlugInInstanceDescription)]
        [EndpointName("DeletePlugInInstance")]
        [EndpointSummary("Deletes a PlugIn Instance")]
        [ProducesResponseType(StatusCodes.Status202Accepted, Description = "The PlugIn Instance will be deleted")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, Description = "The PlugIn Instance is still running")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public async Task<ActionResult> DeletePlugInInstanceAsync([FromRoute, Required, Description(PlugInIdentifier), Range(1, int.MaxValue)] int id,
                                                                  CancellationToken cancellationToken = default)
        {
            try
            {
                await PlugInService.DeletePlugInInstanceAsync(id, cancellationToken);

                return Accepted();
            }
            catch (PlugInInstanceRunningException ex)
            {
                return BadRequest(CreateProblemDetails(HttpStatusCode.BadRequest,
                                                       ex.Message));
            }
        }

        [HttpGet("{id}")]
        [EndpointDescription(GetPlugInDescription)]
        [EndpointName("GetPlugIn")]
        [EndpointSummary("Get an existing PlugIn by Identifier")]
        [ProducesResponseType<PlugIn>(StatusCodes.Status200OK, Description = "The PlugIn details")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, Description = PluginNotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public async Task<ActionResult<PlugIn>> GetPlugInAsync([FromRoute, Required, Description(PlugInIdentifier), Range(1, int.MaxValue)] int id,
                                                               CancellationToken cancellationToken = default)
        {
            var plugIn = await Repository.GetByIdAsync<ModelPlugIn>(id,
                                                                    includeProperties: [],
                                                                    cancellationToken: cancellationToken);

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
        [EndpointDescription(GetListPlugInDescription)]
        [EndpointName("GetPlugIns")]
        [EndpointSummary("Get a list of PlugIns")]
        [ProducesResponseType<IList<PlugIn>>(StatusCodes.Status200OK, Description = "The list of PlugIns in the response")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public async IAsyncEnumerable<PlugIn> GetPlugInsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var plugIns = Repository.GetEnumerableAsync<ModelPlugIn>(includeProperties: [nameof(PlugIn.Instances), nameof(PlugIn.Package)],
                                                                     cancellationToken: cancellationToken);

            await foreach (var item in plugIns)
            {
                yield return item.ToApi();
            }
        }

        [HttpPut("instances/{id}/enable/{state}")]
        [EndpointDescription(EnablePlugInInstanceDescription)]
        [EndpointName("SetPlugInInstanceEnableState")]
        [EndpointSummary("Set a PlugIn Instance enabled state")]
        [ProducesResponseType(StatusCodes.Status200OK, Description = "")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, Description = PluginNotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public async Task<ActionResult> SetPlugInInstanceEnableStateAsync([FromRoute, Required, Description(PlugInInstanceIdentifier), Range(1, int.MaxValue)] int id,
                                                                          [FromRoute, Required, Description("The PlugIn Instance state")] bool state,
                                                                          CancellationToken cancellationToken = default)
        {
            try
            {
                await PlugInService.SetPlugInInstanceEnableAsync(id,
                                                                 state,
                                                                 cancellationToken);

                return Ok();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{id}")]
        [EndpointDescription(UpdatePlugInDescription)]
        [EndpointName("UpdatePlugIn")]
        [EndpointSummary("Update a PlugIn")]
        [ProducesResponseType(StatusCodes.Status204NoContent, Description = "The PlugIn was updated successfully")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, Description = InvalidRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, Description = IdentifierNotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, Description = PluginNameExists)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public async Task<ActionResult<PlugIn>> UpdatePlugInAsync([FromRoute, Required, Description(PlugInIdentifier), Range(1, int.MaxValue)] int id,
                                                                  [FromBody, Required, Description("The PlugIn update")] UpdatePlugIn update,
                                                                  CancellationToken cancellationToken = default)
        {
            try
            {
                await Repository.UpdatePlugInAsync(id,
                                                   update.Name,
                                                   update.Description,
                                                   cancellationToken);

#warning how to map the content location to correct version url
                Response.Headers.ContentLocation = new StringValues($"/api/v1/plugins/{id}");
                return NoContent();
            }
            catch (NameExistsException)
            {
                return Conflict(CreateProblemDetails(HttpStatusCode.Conflict,
                                                     $"A PlugIn with name [{update.Name}] already exists"));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPatch("instances/{id}")]
        [EndpointDescription(UpdatePlugInInstanceDescription)]
        [EndpointName("UpdatePlugInInstance")]
        [EndpointSummary("Update a PlugIn Instance")]
        [ProducesResponseType(StatusCodes.Status202Accepted, Description = "The update was accepted")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public async Task<ActionResult> UpdatePlugInInstanceAsync([FromRoute, Required, Description(PlugInInstanceIdentifier), Range(1, int.MaxValue)] int id,
                                                                  [FromBody, Required, Description("The PlugIn update")] UpdatePlugInInstance update,
                                                                  CancellationToken cancellationToken = default)
        {
            try
            {
                await Repository.UpdatePlugInInstanceAsync(id,
                                                           update.Enabled,
                                                           update.Name,
                                                           update.Description,
                                                           cancellationToken);

                return Accepted();
            }
            catch (NameExistsException)
            {
                return Conflict(CreateProblemDetails(HttpStatusCode.Conflict,
                                                     $"A PlugInInstance with name [{update.Name}] already exists"));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{id}/upload")]
        [EndpointDescription(UploadPackageDescription)]
        [EndpointName("UploadPlugInPackage")]
        [EndpointSummary("Upload a Package for a PlugIn")]
        [ProducesResponseType(StatusCodes.Status202Accepted, Description = "The PlugIn package is uploaded, extracted and verified")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, Description = "Indicates if there was a problem with the upload file")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, Description = IdentifierNotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public async Task<ActionResult> UploadPlugInPackageAsync([FromRoute, Required, Description(PlugInIdentifier), Range(1, int.MaxValue)] int id,
                                                                 IFormFile formFile,
                                                                 CancellationToken cancellationToken = default)
        {
            if (ValidateFormFile(formFile, out var problemDetails))
            {
                var fileName = Path.GetFileName(formFile.FileName);

                Logger.LogDebug("Uploading File: [{FileName}] to PlugIn Id: [{Id}]", fileName, id);

                try
                {
                    await PlugInService.UploadPlugInPackageAsync(
                        id,
                        fileName,
                        formFile.OpenReadStream(),
                        cancellationToken);

                    return Accepted();
                }
                catch (NotFoundException ex)
                {
                    return NotFound(CreateProblemDetails(HttpStatusCode.NotFound,
                                                         ex.Message));
                }
                catch (PlugInInstanceRunningException ex)
                {
                    return BadRequest(CreateProblemDetails(HttpStatusCode.BadRequest,
                                                           $"The PlugIn: [{id}] currently has running instances Id: [{ex.Id}]"));
                }
                catch (NoValidPlugInAssemblyFoundException)
                {
                    return BadRequest(CreateProblemDetails(HttpStatusCode.NotFound,
                                                           $"No valid assembly file found [{fileName}]"));
                }
                catch (PlugInTypeNotFoundException)
                {
                    return BadRequest(CreateProblemDetails(HttpStatusCode.NotFound,
                                                           $"No valid [{nameof(IPlugIn)}] implementation found in package file [{fileName}]"));
                }
                catch (PlugInTypesFoundException)
                {
                    return BadRequest(CreateProblemDetails(HttpStatusCode.NotFound,
                                                           $"Multiple [{nameof(IPlugIn)}] implementations found in package file [{fileName}]"));
                }
            }
            else
            {
                return BadRequest(problemDetails);
            }
        }

        private bool ValidateFormFile(IFormFile formFile,
                                      out ProblemDetails? problemDetails)
        {
            problemDetails = null;

            try
            {
                _codeFileValidationService.ValidateFile(formFile);
            }
            catch (FileContentInvalidException)
            {
                problemDetails = CreateProblemDetails(HttpStatusCode.BadRequest,
                                                      $"File: [{formFile.Name}] invalid content type");
            }
            catch (FileLengthInvalidException)
            {
                problemDetails = CreateProblemDetails(HttpStatusCode.BadRequest,
                                                      $"File: [{formFile.Name}] has invalid length");
            }
            catch (FileNameEmptyException)
            {
                problemDetails = CreateProblemDetails(HttpStatusCode.BadRequest,
                                                      "File name is empty");
            }
            catch (FileNameInvalidExtensionException)
            {
                problemDetails = CreateProblemDetails(HttpStatusCode.BadRequest,
                                                      $"File: [{formFile.Name}] has invalid file extension");
            }
            catch (Exception exception)
            {
                problemDetails = CreateProblemDetails(HttpStatusCode.BadRequest,
                                                      $"Exception occurred check the logs");

                Logger.LogError(exception, "Exception occurred");
            }

            return problemDetails == null;
        }
    }
}