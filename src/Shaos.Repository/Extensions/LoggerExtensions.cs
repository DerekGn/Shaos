using Microsoft.Extensions.Logging;
using Shaos.Repository.Models;

namespace Shaos.Repository.Extensions
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(Level = LogLevel.Debug, Message = "Creating new package for PlugIn: [{id}] Package: [{plugInInformation}]")]
        public static partial void LogCreatingPackage(this ILogger logger,
                                                      int id,
                                                      PlugInInformation plugInInformation);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Duplicate PlugIn Instance Name: [{name}] exists")]
        public static partial void LogDuplicatePlugInInstanceName(this ILogger logger,
                                                                  string name,
                                                                  Exception exception);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Duplicate PlugIn Name: [{name}] exists")]
        public static partial void LogDuplicatePlugInName(this ILogger logger, string name,
                                                   Exception exception);

        [LoggerMessage(Level = LogLevel.Information, Message = "PlugIn [{id}] [{name}] Created")]
        public static partial void LogPlugInCreated(this ILogger logger,
                                                    int id,
                                                    string name);
    }
}