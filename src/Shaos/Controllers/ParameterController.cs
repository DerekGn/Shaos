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
using Shaos.Services.Exceptions;
using Shaos.Services.Parameters;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Shaos.Controllers
{
    [Route("api/v{version:apiVersion}/parameters")]
    public class ParameterController : CoreController
    {
        private readonly IParameterService _parameterService;

        public ParameterController(ILogger<ParameterController> logger,
                                   IParameterService parameterService) : base(logger)
        {
            _parameterService = parameterService;
        }

        [HttpPut("{parameterId}/bool/{value}")]
        [EndpointDescription("Writes a boolean value to a parameter")]
        [EndpointName("WriteBool")]
        [EndpointSummary("Writes a boolean value to a parameter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, Description = Status400BadRequestText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, Description = "The parameter was not found")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public async Task<ActionResult> WriteAsync([FromRoute, Required, Range(1, int.MaxValue), Description("The parameter identifier")] int parameterId,
                                                   [FromRoute, Required, Description("The value to write")] bool value,
                                                   CancellationToken cancellationToken = default)
        {
            return await ExecuteParameterWriteAsync(async () =>
            {
                await _parameterService.WriteParameterAsync(parameterId,
                                                            value,
                                                            cancellationToken);
                return Ok();
            });
        }

        [HttpPut("{parameterId}/float/{value}")]
        [EndpointDescription("Writes a float value to a parameter")]
        [EndpointName("WriteFloat")]
        [EndpointSummary("Writes a float value to a parameter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, Description = Status400BadRequestText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, Description = "The parameter was not found")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public async Task<ActionResult> WriteAsync([FromRoute, Required, Range(1, int.MaxValue), Description("The parameter identifier")] int parameterId,
                                                   [FromRoute, Required, Range(float.MinValue, float.MaxValue), Description("The value to write")] float value,
                                                   CancellationToken cancellationToken = default)
        {
            return await ExecuteParameterWriteAsync(async () =>
            {
                await _parameterService.WriteParameterAsync(parameterId,
                                                            value,
                                                            cancellationToken);
                return Ok();
            });
        }

        [HttpPut("{parameterId}/int/{value}")]
        [EndpointDescription("Writes a int value to a parameter")]
        [EndpointName("WriteInt")]
        [EndpointSummary("Writes a int value to a parameter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, Description = Status400BadRequestText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, Description = "The parameter was not found")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public async Task<ActionResult> WriteAsync([FromRoute, Required, Range(1, int.MaxValue), Description("The parameter identifier")] int parameterId,
                                                   [FromRoute, Required, Range(int.MinValue, int.MaxValue), Description("The value to write")] int value,
                                                   CancellationToken cancellationToken = default)
        {
            return await ExecuteParameterWriteAsync(async () =>
            {
                await _parameterService.WriteParameterAsync(parameterId,
                                                            value,
                                                            cancellationToken);
                return Ok();
            });
        }

        [HttpPut("{parameterId}/uint/{value}")]
        [EndpointDescription("Writes a unit value to a parameter")]
        [EndpointName("WriteUInt")]
        [EndpointSummary("Writes a uint value to a parameter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, Description = Status400BadRequestText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, Description = "The parameter was not found")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, Description = Status401UnauthorizedText)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, Description = Status500InternalServerErrorText)]
        public async Task<ActionResult> WriteAsync([FromRoute, Required, Range(1, int.MaxValue), Description("The parameter identifier")] int parameterId,
                                                   [FromRoute, Required, Range(uint.MinValue, uint.MaxValue), Description("The value to write")] uint value,
                                                   CancellationToken cancellationToken = default)
        {
            return await ExecuteParameterWriteAsync(async () =>
            {
                await _parameterService.WriteParameterAsync(parameterId,
                                                            value,
                                                            cancellationToken);
                return Ok();
            });
        }

        private async Task<ActionResult> ExecuteParameterWriteAsync(Func<Task<ActionResult>> update)
        {
            try
            {
                return await update();
            }
            catch (ParameterInvalidTypeException invalidType)
            {
                return NotFound(CreateProblemDetails(HttpStatusCode.NotFound,
                                                     $"Parameter [{invalidType.Id}] is invalid type. Expected Type: [{invalidType.Expected}] Actual Type: [{invalidType.ActualType}]"));
            }
            catch (ParameterNotFoundException notFound)
            {
                return NotFound(CreateProblemDetails(HttpStatusCode.NotFound,
                                                     $"Parameter [{notFound.Id}] not found"));
            }
            catch (ParameterNotWritableException notWritable)
            {
                return NotFound(CreateProblemDetails(HttpStatusCode.NotFound,
                                                     $"Parameter [{notWritable.Id}] is not writable"));
            }
        }
    }
}