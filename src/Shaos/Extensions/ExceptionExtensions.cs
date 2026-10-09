using Microsoft.AspNetCore.Mvc;
using Shaos.Repository.Exceptions;
using Shaos.Sdk;
using Shaos.Services.Exceptions;
using Shaos.Services.Runtime.Exceptions;
using System.Net;

namespace Shaos.Extensions
{
    public static class ExceptionExtensions
    {
        public static ProblemDetails ToProblemDetails(this ParameterInvalidTypeException exception)
        {
            var statusCode = HttpStatusCode.BadRequest;

            return new ProblemDetails()
            {
                Detail = $"Parameter [{exception.Id}] is invalid type. Expected Type: [{exception.Expected}] Actual Type: [{exception.ActualType}]",
                Status = (int?)statusCode,
                Title = statusCode.ToString(),
                Type = statusCode.MapToType()
            };
        }

        public static ProblemDetails ToProblemDetails(this PlugInInstanceRunningException exception)
        {
            var statusCode = HttpStatusCode.BadRequest;

            return new ProblemDetails()
            {
                Detail = exception.Message,
                Status = (int?)statusCode,
                Title = statusCode.ToString(),
                Type = statusCode.MapToType()
            };
        }

        public static ProblemDetails ToProblemDetails(this NoValidPlugInAssemblyFoundException exception)
        {
            var statusCode = HttpStatusCode.BadRequest;

            return new ProblemDetails()
            {
                Detail = exception.Message,
                Status = (int?)statusCode,
                Title = statusCode.ToString(),
                Type = statusCode.MapToType()
            };
        }

        public static ProblemDetails ToProblemDetails(this PlugInTypeNotFoundException exception)
        {
            return CreateProblemDetails(HttpStatusCode.BadRequest,
                                        exception);
        }

        public static ProblemDetails ToProblemDetails(this PlugInTypesFoundException exception)
        {
            var statusCode = HttpStatusCode.BadRequest;

            return new ProblemDetails()
            {
                Detail = $"Multiple [{nameof(IPlugIn)}] implementations found in package file [{fileName}]",
                Status = (int?)statusCode,
                Title = statusCode.ToString(),
                Type = statusCode.MapToType()
            };
        }

        public static ProblemDetails ToProblemDetails(this FileContentInvalidException exception)
        {
            var statusCode = HttpStatusCode.BadRequest;

            return new ProblemDetails()
            {
                Detail = $"Multiple [{nameof(IPlugIn)}] implementations found in package file [{fileName}]",
                Status = (int?)statusCode,
                Title = statusCode.ToString(),
                Type = statusCode.MapToType()
            };
        }

        public static ProblemDetails ToProblemDetails(this FileLengthInvalidException exception)
        {
            var statusCode = HttpStatusCode.BadRequest;

            return new ProblemDetails()
            {
                Detail = $"File: [{formFile.Name}] has invalid length",
                Status = (int?)statusCode,
                Title = statusCode.ToString(),
                Type = statusCode.MapToType()
            };
        }

        public static ProblemDetails ToProblemDetails(this FileNameEmptyException exception)
        {
            var statusCode = HttpStatusCode.BadRequest;

            return new ProblemDetails()
            {
                Detail = "The file name is empty",
                Status = (int?)statusCode,
                Title = statusCode.ToString(),
                Type = statusCode.MapToType()
            };
        }

        public static ProblemDetails ToProblemDetails(this FileNameInvalidExtensionException exception)
        {
            var statusCode = HttpStatusCode.BadRequest;

            return new ProblemDetails()
            {
                Detail = $"File: [{formFile.Name}] has invalid file extension",
                Status = (int?)statusCode,
                Title = statusCode.ToString(),
                Type = statusCode.MapToType()
            };
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

        public static ProblemDetails ToProblemDetails(this NameExistsException exception)
        {
            var statusCode = HttpStatusCode.Conflict;

            return new ProblemDetails()
            {
                Detail = $"A PlugIn with name [{exception.Name}] already exists",
                Status = (int?)statusCode,
                Title = statusCode.ToString(),
                Type = statusCode.MapToType()
            };
        }

        public static ProblemDetails ToProblemDetails(this ParameterNotFoundException exception)
        {
            var statusCode = HttpStatusCode.NotFound;

            return new ProblemDetails()
            {
                Detail = $"Parameter [{exception.Id}] not found",
                Status = (int?)statusCode,
                Title = statusCode.ToString(),
                Type = statusCode.MapToType()
            };
        }

        public static ProblemDetails ToProblemDetails(this ParameterNotWritableException exception)
        {
            var statusCode = HttpStatusCode.BadRequest;

            return new ProblemDetails()
            {
                Detail = $"Parameter [{exception.Id}] is not writable",
                Status = (int?)statusCode,
                Title = statusCode.ToString(),
                Type = statusCode.MapToType()
            };
        }

        public static ProblemDetails ToProblemDetails(this NotFoundException exception)
        {
            var statusCode = HttpStatusCode.NotFound;

            return new ProblemDetails()
            {
                Detail = ,
                Status = (int?)statusCode,
                Title = statusCode.ToString(),
                Type = statusCode.MapToType()
            };
        }

        private static ProblemDetails CreateProblemDetails(HttpStatusCode httpStatusCode,
                                                                                                                                           Exception exception)
        {
            return new ProblemDetails()
            {
                Detail = exception.Message,
                Status = (int?)httpStatusCode,
                Title = httpStatusCode.ToString(),
                Type = httpStatusCode.MapToType()
            };
        }
    }
}