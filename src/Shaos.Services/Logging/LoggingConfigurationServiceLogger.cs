
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Shaos.Services.Logging
{
    internal static partial class LoggingConfigurationServiceLogger
    {
        [LoggerMessage(Level = LogLevel.Information,
            Message = "Initialising logging configuration")]
        public static partial void LogInitalisingLoggingConfiguration(this ILogger<LoggingConfigurationService> logger);

        [LoggerMessage(Level = LogLevel.Debug,
            Message = "Updating LogLevelSwitch [{name}] Level: [{level}]")]
        public static partial void LogUpdatingLogLevelSwitch(this ILogger<LoggingConfigurationService> logger,
                                                             string name,
                                                             LogEventLevel level);
    }
}
