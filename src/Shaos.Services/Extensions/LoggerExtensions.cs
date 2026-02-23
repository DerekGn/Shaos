using Microsoft.Extensions.Logging;
using Serilog.Events;
using System.Reflection;

namespace Shaos.Services.Extensions
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(Level = LogLevel.Information,
            Message = "Stopping Application")]
        public static partial void LogApplicationStopping(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Debug,
                    Message = "Assembly not resolved from dependency context [{assemblyName}]")]
        public static partial void LogAssemblyNotResolved(this ILogger logger,
                                                          AssemblyName assemblyName);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Assembly not resolved from default context [{assemblyName}]")]
        public static partial void LogAssemblyNotResolved(this ILogger logger,
                                                          AssemblyName assemblyName,
                                                          FileNotFoundException exception);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Resolved Assembly from dependency context [{assemblyName}]")]
        public static partial void LogAssemblyResolved(this ILogger logger,
                                                       AssemblyName assemblyName);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Resolved Assembly from default context [{assemblyName}]")]
        public static partial void LogAssemblyResolvedFromDefault(this ILogger logger,
                                                                  AssemblyName assemblyName);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "Attaching event handlers to PlugIn Instance Id: [{id}] Name: [{name}]")]
        public static partial void LogAttachEventHandlers(this ILogger logger,
                                                          int id,
                                                          string name);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Attaching device signal level and battery level event handler for Device: [{id}] Name: [{name}]")]
        public static partial void LogAttachingDeviceSignalAndBatteryHandlers(this ILogger logger,
                                                                              int id,
                                                                              string name);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Attaching device list event handler for PlugIn Id: [{id}]")]
        public static partial void LogAttachingDevicesListChangedHandler(this ILogger logger,
                                                                         int id);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Attaching event handler for parameter Id: [{id}] Name: [{name}]")]
        public static partial void LogAttachingParameterEventHandler(this ILogger logger,
                                                                     int id,
                                                                     string? name);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Attaching parameter list event handler for PlugIn Id: [{id}] Name: [{name}]")]
        public static partial void LogAttachParametersListChangedHandler(this ILogger logger,
                                                                         int id,
                                                                         string name);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Attempting to resolve assembly for [{assemblyName}]")]
        public static partial void LogAttemptResolveAssembly(this ILogger logger,
                                                             AssemblyName assemblyName);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Creating Runtime Instance for Device [{id}] Name: [{name}]")]
        public static partial void LogCreatingInstance(this ILogger logger,
                                                       int id,
                                                       string name);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Creating package directory [{path}]")]
        public static partial void LogCreatingPackageDirectory(this ILogger logger,
                                                               string path);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Creating packages directory [{path}]")]
        public static partial void LogCreatingPackagesDirectory(this ILogger logger,
                                                                string path);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Creating instance of [{fullName}]")]
        public static partial void LogCreatingPlugIn(this ILogger logger,
                                                     string? fullName);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Creating a new PlugIn package. PlugIn: [{id}] Assembly: [{assemblyFileName}] Version: [{assemblyVersion}]")]
        public static partial void LogCreatingPlugInPackage(this ILogger logger,
                                                            int id,
                                                            string assemblyFileName,
                                                            string assemblyVersion);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Deleting directory [{path}]")]
        public static partial void LogDeletingDirectory(this ILogger logger,
                                                        string path);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Detaching device signal level and battery level event handler for Device: [{id}] Name: [{name}]")]
        public static partial void LogDetachingDeviceSignalAndBatteryHandlers(this ILogger logger,
                                                                              int id,
                                                                              string name);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Detaching event handler for parameter Id: [{id}] Name: [{name}]")]
        public static partial void LogDetachParametersChangedHandler(this ILogger logger,
                                                                     int id,
                                                                     string? name);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Detaching parameter list event handler for PlugIn Id: [{id}] Name: [{name}]")]
        public static partial void LogDetachParametersListChangedHandler(this ILogger logger,
                                                                         int id,
                                                                         string name);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Created Device [{id}] Name: [{name}]")]
        public static partial void LogDeviceCreated(this ILogger logger,
                                                    int id,
                                                    string name);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Created Device [{id}] Name: [{deviceName}] Parameter: [{parameterId}] Name: [{parameterName}]")]
        public static partial void LogDeviceCreated(this ILogger logger,
                                                    int id,
                                                    string deviceName,
                                                    int parameterId,
                                                    string? parameterName);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Deleting Device Id: [{id}]")]
        public static partial void LogDeviceDelete(this ILogger logger,
                                                   int id);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Created Device: [{id}] Name: [{deviceName}] Parameter: [{parameterId}] Name: [{parameterName}]")]
        public static partial void LogDeviceParameterCreated(this ILogger logger,
                                                             int id,
                                                             string deviceName,
                                                             int parameterId,
                                                             string? parameterName);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Executed work item Elapsed: [{elapsed}] remaining WorkItems: [{count}]")]
        public static partial void LogElapsedWorkItem(this ILogger logger,
                                                      TimeSpan elapsed,
                                                      int count);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Emptying directory [{path}]")]
        public static partial void LogEmptyDirectory(this ILogger logger,
                                                     string path);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Emptying package directory [{path}]")]
        public static partial void LogEmptyingPackageDirectory(this ILogger logger,
                                                               string path);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "Event items collection empty")]
        public static partial void LogEventItemsEmpty(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "Runtime execution instance count exceeded. Count: [{count}] Max: [{max}]")]
        public static partial void LogExecutionInstanceCount(this ILogger logger,
                                                             int count,
                                                             int max);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Runtime execution instance Count: [{count}] Max: [{max}]")]
        public static partial void LogExecutionInstanceCountExceeded(this ILogger logger,
                                                                     int count,
                                                                     int max);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Extracting package: [{sourcePath}] to [{targetPath}]")]
        public static partial void LogExtracingPackage(this ILogger logger,
                                                       string sourcePath,
                                                       string targetPath);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "Found running instance [{plugInInstanceId}]")]
        public static partial void LogFoundRunningInstance(this ILogger logger,
                                                           int plugInInstanceId);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Initialising logging configuration")]
        public static partial void LogInitalisingLoggingConfiguration(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "Runtime Instance Id: [{Id}] Name: [{name}] is already running")]
        public static partial void LogInstanceAlreadyRunning(this ILogger logger,
                                                             int id,
                                                             string name);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "Runtime Instance Id: [{id}] Name: [{name}] exists")]
        public static partial void LogInstanceExists(this ILogger logger, int id,
                                               string name);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "PlugIn Instance Id: [{Id}] not found")]
        public static partial void LogInstanceNotFound(this ILogger logger,
                                                       int id);

        [LoggerMessage(Level = LogLevel.Trace,
            Message = "Runtime instance: [{id}] Name: [{name}] Not Running")]
        public static partial void LogInstanceNotRunning(this ILogger logger,
                                                         int id,
                                                         string name);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "Runtime Instance not stopped within timeout. Id: [{id}] Name: [{name}]")]
        public static partial void LogInstanceNotStoppedWithinTimeOut(this ILogger logger,
                                                                      int id,
                                                                      string name);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "Instance [{id}] Running")]
        public static partial void LogInstanceRunning(this ILogger logger,
                                                      int id);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Stopped execution. Id: [{id}] Name: [{name}]")]
        public static partial void LogInstanceStopped(this ILogger logger,
                                                      int id,
                                                      string name);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "PlugIn [{name}] contains invalid number of constructors [{length}]")]
        public static partial void LogInvalidConstructorCount(this ILogger logger,
                                                              string name,
                                                              int length);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "PlugIn [{name}] contains invalid number of constructor parameters [{length}]")]
        public static partial void LogInvalidConstructorParameterCount(this ILogger logger,
                                                                       string name,
                                                                       int length);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "PlugIn [{name}] [{type}] parameter invalid generic type parameter [{argument}]")]
        public static partial void LogInvalidConstuctorGenericParameters(this ILogger logger,
                                                                         string name,
                                                                         string type,
                                                                         string argument);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "PlugIn [{name}] contains an invalid constructor parameters [{parameters}]")]
        public static partial void LogInvalidConstuctorParameters(this ILogger logger,
                                                                  string name,
                                                                  string parameters);

        [LoggerMessage(Level = LogLevel.Warning,
                                            Message = "Sender is invalid type: [{type}]")]
        public static partial void LogInvalidType(this ILogger logger,
                                                  Type type);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "More than one PlugIn type found in assembly [{fullName}]")]
        public static partial void LogMultiplePlugInTypesFound(this ILogger logger,
                                                               string? fullName);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "No assembly file ending with [{plugInNamePostFix}] was found in the package [{packageFileName}] files")]
        public static partial void LogNoAssemblyFound(this ILogger logger,
                                                      string plugInNamePostFix,
                                                      string packageFileName);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "No PlugIn type found in assembly [{fullName}]")]
        public static partial void LogNoPlugInTypeFound(this ILogger logger,
                                                        string? fullName);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "PlugIn [{id}] Name: [{name}] package not changed")]
        public static partial void LogPackageNotChanged(this ILogger logger,
                                                        int id,
                                                        string name);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Deleting Parameter Id: [{id}]")]
        public static partial void LogParameterDelete(this ILogger logger,
                                                      int id);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "Parameter Id: [{Id}] Not Found")]
        public static partial void LogParameterNotFound(this ILogger logger,
                                                        int id);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Completed PlugIn Runtime Instance Task: {newLine}{task}")]
        public static partial void LogPlugInCompleted(this ILogger logger,
                                                      string newLine,
                                                      string task);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "PlugInInstance has no configuration [{id}]")]
        public static partial void LogPlugInHasNoConfiguration(this ILogger logger,
                                                               int id);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Instance completed. Id: [{id}] Name: [{name}] Task Status: [{status}]")]
        public static partial void LogPlugInInstanceCompleted(this ILogger logger,
                                                              int id,
                                                              string name,
                                                              TaskStatus status);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Creating PlugInInstance. PlugIn: [{id}]")]
        public static partial void LogPlugInInstanceCreating(this ILogger logger,
                                                             int id);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Deleting Instance [{id}] from InstanceHost")]
        public static partial void LogPlugInInstanceDeleting(this ILogger logger,
                                                             int id);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Deleting PlugInInstance [{id}]")]
        public static partial void LogPlugInInstanceDeletingInstanceHost(this ILogger logger,
                                                                         int id);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "Instance completed. Id: [{id}] Name: [{name}] Task Status: [{status}]")]
        public static partial void LogPlugInInstanceFaulted(this ILogger logger,
                                                            AggregateException? exception,
                                                            int id,
                                                            string name,
                                                            TaskStatus status);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "PlugIn instance Id: [{id}] Name: [{name}] was not configured.")]
        public static partial void LogPlugInInstanceNotConfigured(this ILogger logger,
                                                                  int id,
                                                                  string name);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "PlugIn instance Id: [{id}] was not found.")]
        public static partial void LogPlugInInstanceNotConfigured(this ILogger logger,
                                                                  int id);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "PlugIn Instance Id: [{id}] Name: [{name}] not enabled for startUp")]
        public static partial void LogPlugInInstanceNotEnabled(this ILogger logger,
                                                               int id,
                                                               string name);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "PlugInInstance package not assigned. Id: [{id}]")]
        public static partial void LogPlugInInstancePackageNotAssigned(this ILogger logger,
                                                                       int id);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "PlugIn: [{id}] not found")]
        public static partial void LogPlugInNotFound(this ILogger logger,
                                                     int id);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "PlugIn [{id}] still running")]
        public static partial void LogPlugInStillRunning(this ILogger logger,
                                                         int id);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "PlugIn Task cancelled")]
        public static partial void LogPlugInTaskCancelled(this ILogger logger,
                                                          OperationCanceledException exception);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Removing Instance [{id}] from instance host")]
        public static partial void LogRemovingInstance(this ILogger logger,
                                                       int id);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Resolved PlugIn: [{name}] from Assembly: [{assembly}]")]
        public static partial void LogResolvedPlugIn(this ILogger logger,
                                                     string name,
                                                     string? assembly);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "Found running PlugIn Instances Id: [{ids}]")]
        public static partial void LogRunningPlugInFound(this ILogger logger,
                                                         List<int> ids);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Starting PlugIn Runtime Instance execution Id: [{id}] Name: [{name}]")]
        public static partial void LogStartingInstance(this ILogger logger,
                                                       int id,
                                                       string name);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "Starting PlugIn instance. Id: [{id} Name: [{name}]]")]
        public static partial void LogStartingPlugInInstance(this ILogger logger,
                                                             int id,
                                                             string name);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Stopping Runtime Instance: [{id}] Name: [{name}]")]
        public static partial void LogStoppingInstance(this ILogger logger,
                                                       int id,
                                                       string name);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "Unable to find Runtime Instance Id: [{id}]")]
        public static partial void LogUnableToFindInstance(this ILogger logger,
                                                           int id);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "Unable to resolve Device for Id: [{id}]")]
        public static partial void LogUnableToResolveDevice(this ILogger logger,
                                                            int id);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "Unable to resolve PlugIn for Id: [{id}]")]
        public static partial void LogUnableToResolvePlugIn(this ILogger logger,
                                                            int id);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "Unable to resolve PlugIn Instance. Id: [{Id}]")]
        public static partial void LogUnableToResolvePlugInInstance(this ILogger logger,
                                                                    int id);

        [LoggerMessage(Level = LogLevel.Warning,
            Message = "Unable to start a PlugIn instance Id: [{id}]. Instance host does not contain instance.")]
        public static partial void LogUnableToStartPlugIn(this ILogger logger,
                                                          int id);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "Unhandled exception occurred")]
        public static partial void LogUnhandledException(this ILogger logger,
                                                         Exception ex);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Unloading instance execution context for instance: [{id}] Name: [{name}]")]
        public static partial void LogUnloadingInstance(this ILogger logger,
                                                        int id,
                                                        string name);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Unloading instance context for PlugIn: [{plugInId}]")]
        public static partial void LogUnloadingInstance(this ILogger logger,
                                                        int plugInId);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Updating LogLevelSwitch [{name}] Level: [{level}]")]
        public static partial void LogUpdatingLogLevelSwitch(this ILogger logger,
                                                             string name,
                                                             LogEventLevel level);

        [LoggerMessage(Level = LogLevel.Trace,
            Message = "Updating parameter Id: [{id}] Name: [{name}] Value: [{value}]")]
        public static partial void LogUpdatingParameter(this ILogger logger,
                                                        int id,
                                                        string name,
                                                        string value);

        [LoggerMessage(Level = LogLevel.Trace,
            Message = "Updating parameter Id: [{id}] Name: [{name}] Value: [{value}]")]
        public static partial void LogUpdatingParameter(this ILogger logger,
                                                        int id,
                                                        string name,
                                                        int value);

        [LoggerMessage(Level = LogLevel.Trace,
            Message = "Updating parameter Id: [{id}] Name: [{name}] Value: [{value}]")]
        public static partial void LogUpdatingParameter(this ILogger logger,
                                                        int id,
                                                        string name,
                                                        uint value);

        [LoggerMessage(Level = LogLevel.Trace,
            Message = "Updating parameter Id: [{id}] Name: [{name}] Value: [{value}]")]
        public static partial void LogUpdatingParameter(this ILogger logger,
                                                        int id,
                                                        string name,
                                                        bool value);

        [LoggerMessage(Level = LogLevel.Trace,
            Message = "Updating parameter Id: [{id}] Name: [{name}] Value: [{value}]")]
        public static partial void LogUpdatingParameter(this ILogger logger,
                                                        int id,
                                                        string name,
                                                        float value);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Updating a PlugIn package. PlugIn: [{id}] Assembly: [{assemblyFileName}] Version: [{assemblyVersion}]")]
        public static partial void LogUpdatingPlugInPackage(this ILogger logger,
                                                            int id,
                                                            string assemblyFileName,
                                                            string assemblyVersion);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Writing Package File: [{file}]")]
        public static partial void LogWritingPackageFile(this ILogger logger,
                                                         string file);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Writing PlugIn Package file [{packageFileName}]")]
        public static partial void LogWrittingPlugInPackageFile(this ILogger logger,
                                                                string packageFileName);

        [LoggerMessage(Level = LogLevel.Error,
            Message = "No assembly file ending with [{plugInNamePostFix}] was found in the package [{packageFileName}] files")]
        public static partial void NoAssemblyFilePackageFileFound(this ILogger logger,
                                                                  string plugInNamePostFix,
                                                                  string packageFileName);
    }
}