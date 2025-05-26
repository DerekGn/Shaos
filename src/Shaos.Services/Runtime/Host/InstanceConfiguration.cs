using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Shaos.Services.Runtime.Host
{
    public class InstanceConfiguration
    {
        public InstanceConfiguration(bool hasConfiguration,
                                     string? configuration = default)
        {
            HasConfiguration = hasConfiguration;
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
        /// Indicates if this instance has configuration
        /// </summary>
        public bool HasConfiguration { get; }

        /// <summary>
        /// Indicates if configuration required
        /// </summary>
        public bool IsConfigured => !string.IsNullOrWhiteSpace(Configuration);

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{nameof(Configuration)}: {(Configuration ?? "Empty")}");
            stringBuilder.AppendLine($"{nameof(HasConfiguration)}: {HasConfiguration}");
            stringBuilder.AppendLine($"{nameof(IsConfigured)}: {IsConfigured}");

            return stringBuilder.ToString();
        }
    }
}