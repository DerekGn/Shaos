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
using Shaos.Repository.Exceptions;
using Shaos.Services.Exceptions;
using Shaos.Services.Runtime.Exceptions;
using System.Net;

namespace Shaos.Extensions
{
    public static class ExceptionExtensions
    {
        public static ProblemDetails ToProblemDetails(this PlugInInstanceRunningException exception)
        {
            return CreateProblemDetails(exception,
                                        HttpStatusCode.BadRequest);
        }

        public static ProblemDetails ToProblemDetails(this NoValidPlugInAssemblyFoundException exception)
        {
            return CreateProblemDetails(exception,
                                        HttpStatusCode.NotFound);
        }

        public static ProblemDetails ToProblemDetails(this PlugInTypeNotFoundException exception)
        {
            return CreateProblemDetails(exception,
                                        HttpStatusCode.NotFound);
        }

        public static ProblemDetails ToProblemDetails(this PlugInTypesFoundException exception)
        {
            return CreateProblemDetails(exception,
                                        HttpStatusCode.BadRequest);
        }

        public static ProblemDetails ToProblemDetails(this FileContentInvalidException exception)
        {
            return CreateProblemDetails(exception,
                                        HttpStatusCode.BadRequest);
        }

        public static ProblemDetails ToProblemDetails(this FileLengthInvalidException exception)
        {
            return CreateProblemDetails(exception,
                                        HttpStatusCode.BadRequest);
        }

        public static ProblemDetails ToProblemDetails(this FileNameEmptyException exception)
        {
            return CreateProblemDetails(exception,
                                        HttpStatusCode.BadRequest);
        }

        public static ProblemDetails ToProblemDetails(this FileNameInvalidExtensionException exception)
        {
            return CreateProblemDetails(exception,
                                        HttpStatusCode.BadRequest);
        }

        public static ProblemDetails ToProblemDetails(this Exception exception)
        {
            var statusCode = HttpStatusCode.InternalServerError;

            return new ProblemDetails()
            {
                Detail = "Exception occurred check the logs",
                Status = (int?)statusCode,
                Title = statusCode.ToString(),
                Type = statusCode.MapToType()
            };
        }

        public static ProblemDetails ToProblemDetails(this PlugInNameExistsException exception)
        {
            return CreateProblemDetails(exception,
                                        HttpStatusCode.Conflict);
        }

        private static ProblemDetails CreateProblemDetails(Exception exception,
                                                           HttpStatusCode statusCode)
        {
            return new ProblemDetails()
            {
                Detail = exception.Message,
                Status = (int?)statusCode,
                Title = statusCode.ToString(),
                Type = statusCode.MapToType()
            };
        }
    }
}