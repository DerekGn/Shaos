using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Shaos.Services.Runtime.Host
{
    public class InstanceConfiguration
    {
        public InstanceConfiguration(bool requiresConfiguration,
                                     string? configuration = default)
        {
            RequiresConfiguration = requiresConfiguration;
            Configuration = configuration;
        }

        /// <summary>
        /// The PlugIn instance configuration
        /// </summary>
        /// <remarks>
        /// The PlugIn configuration JSON string
        /// </remarks>
        public string? Configuration { get; }

        /// <summary>
        /// Indicates if this instance has configuration settings
        /// </summary>
        public bool RequiresConfiguration { get; }

        /// <summary>
        /// Indicates if this instance is configured
        /// </summary>
        public bool IsConfigured => !string.IsNullOrWhiteSpace(Configuration);

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{nameof(Configuration)}: {(Configuration ?? "Empty")} ");
            stringBuilder.AppendLine($"{nameof(IsConfigured)}: {IsConfigured}");
            stringBuilder.AppendLine($"{nameof(RequiresConfiguration)}: {RequiresConfiguration} ");

            return stringBuilder.ToString();
        }
    }
}