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

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shaos.Extensions;
using System.Net;

namespace Shaos.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [ApiVersion(ApiContractVersions.VersionOne)]
    //[Authorize(AuthenticationSchemes = ApiAuthenticationScheme.AuthenticationSchemes)]
    public abstract class CoreController : ControllerBase
    {
        internal const string Status401UnauthorizedText = "The bear token is invalid";
        internal const string Status500InternalServerErrorText = "Indicates that the server was unable to process the request";
        internal const string InvalidRequest = "Indicates that the request syntax is invalid";

        internal readonly ILogger<CoreController> Logger;

        protected CoreController(ILogger<CoreController> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        internal static ProblemDetails CreateProblemDetails(
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
    }
}
