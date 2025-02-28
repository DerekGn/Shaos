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
using Shaos.Services.Compiler;
using Swashbuckle.AspNetCore.Annotations;

namespace Shaos.Controllers
{
    [Route("api/v{version:apiVersion}/compiler")]
    public class CompilerController : BasePlugInController
    {
        private readonly IPlugInCompilerService _plugInCompilerService;

        public CompilerController(
            ILogger<CompilerController> logger,
            IPlugInService plugInService,
            IPlugInCompilerService plugInCompilerService) : base(logger, plugInService)
        {
            _plugInCompilerService = plugInCompilerService ?? throw new ArgumentNullException(nameof(plugInCompilerService));
        }

        [HttpGet("{id}")]
        [SwaggerResponse(StatusCodes.Status200OK, "The PlugIn compilation status if one is available", typeof(CompilationStatus))]
        [SwaggerResponse(StatusCodes.Status404NotFound, IdentifierNotFound)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Get the PlugIn compilation status",
            Description = "Get the PlugIn compilation status if one exists otherwise empty response",
            OperationId = "GetCompilationStatus")]
        public async Task<ActionResult> GetCompilationStatus(
            [FromRoute, SwaggerParameter("The PlugIn identifier to get the compilation status", Required = true)] int id,
            CancellationToken cancellationToken)
        {
            return await GetPlugInOperationAsync(id, async (plugIn) =>
            {
                var compilationResult = _plugInCompilerService.GetCompilationStatus(id);

                return compilationResult != null ? Ok(compilationResult?.ToApiModel()) : Ok();
            }, cancellationToken);
        }

        [HttpPut("{id}")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Start a PlugIn CodeFiles compilation")]
        [SwaggerResponse(StatusCodes.Status404NotFound, IdentifierNotFound)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Status401UnauthorizedText, Type = typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Status500InternalServerErrorText, Type = typeof(ProblemDetails))]
        [SwaggerOperation(
            Summary = "Start the compilation of a PlugIns CodeFiles",
            Description = "Starts the compilation of a PlugIns CodeFiles. Compilation is asynchronously executed.",
            OperationId = "StartPlugInCompile")]
        public async Task<ActionResult> StartPlugInCompileAsync(
            [FromRoute, SwaggerParameter("The PlugIn identifier to compile", Required = true)] int id,
            CancellationToken cancellationToken)
        {
            return await GetPlugInOperationAsync(id, async (plugIn) =>
            {
                 _plugInCompilerService.StartCompilation(plugIn);

                return Accepted();
            },
            cancellationToken);
        }
    }
}
