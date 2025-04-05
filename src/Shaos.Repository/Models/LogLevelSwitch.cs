
using Serilog.Events;

namespace Shaos.Repository.Models
{
    /// <summary>
    /// A log level switch entity
    /// </summary>
    public class LogLevelSwitch : BaseEntity
    {
        /// <summary>
        /// The name of this <see cref="LogLevelSwitch"/>
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The current <see cref="LogEventLevel"/>
        /// </summary>
        public LogEventLevel Level { get; set; }
    }
}
