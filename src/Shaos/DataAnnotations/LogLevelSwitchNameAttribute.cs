using Shaos.Services.Logging;

namespace Shaos.DataAnnotations
{
    public class LogLevelSwitchNameAttribute : LogSwitchAttribute
    {
        protected override string SwitchTypeName => throw new NotImplementedException();

        protected override IEnumerable<string> GetAvailableNames(ILoggingConfiguration loggingConfiguration)
        {
            return loggingConfiguration.LoggingLevelSwitches.Keys;
        }
    }
}
