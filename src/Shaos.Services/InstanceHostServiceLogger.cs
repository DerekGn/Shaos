using Microsoft.Extensions.Logging;

namespace Shaos.Services
{
    internal static partial class InstanceHostServiceLogger
    {
        [LoggerMessage(Level = LogLevel.Warning,
            Message = "Attaching event handlers to PlugIn Instance Id: [{id}] Name: [{name}]")]
        public static partial void LogAttachEventHandlers(this ILogger<InstanceHostService> logger,
                                                          int id,
                                                          string name);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "PlugInInstance has no configuration [{id}]")]
        public static partial void LogPlugInHasNoConfiguration(this ILogger<InstanceHostService> logger,
                                                               int id);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "PlugIn instance Id: [{id}] Name: [{name}] was not configured.")]
        public static partial void LogPlugInInstanceNotConfigured(this ILogger<InstanceHostService> logger,
                                                                  int id,
                                                                  string name);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "PlugIn instance Id: [{id}] was not found.")]
        public static partial void LogPlugInInstanceNotConfigured(this ILogger<InstanceHostService> logger,
                                                                  int id);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "PlugIn Instance Id: [{id}] Name: [{name}] not enabled for startUp")]
        public static partial void LogPlugInInstanceNotEnabled(this ILogger<InstanceHostService> logger,
                                                               int id,
                                                               string name);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "PlugInInstance package not assigned. Id: [{id}]")]
        public static partial void LogPlugInInstancePackageNotAssigned(this ILogger<InstanceHostService> logger,
                                                                       int id);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Starting PlugIn instance. Id: [{id} Name: [{name}]]")]
        public static partial void LogStartingPlugInInstance(this ILogger<InstanceHostService> logger,
                                                             int id,
                                                             string name);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "Unable to resolve PlugIn Instance. Id: [{Id}]")]
        public static partial void LogUnableToResolvePlugInInstance(this ILogger<InstanceHostService> logger,
                                                                    int id);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "Unable to start a PlugIn instance Id: [{id}]. Instance host does not contain instance.")]
        public static partial void LogUnableToStartPlugIn(this ILogger<InstanceHostService> logger,
                                                          int id);
    }
}