
using System.ComponentModel.DataAnnotations;

namespace Shaos.Repository.Models
{
    /// <summary>
    /// Represents a PlugIn entity
    /// </summary>
    public class PlugIn : Base
    {
        /// <summary>
        /// The name of this <see cref="PlugIn"/>
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// The description of this <see cref="PlugIn"/>
        /// </summary>
        public string? Description { get; init; } = string.Empty;

        /// <summary>
        /// Indicates if the <see cref="PlugIn"/> is enabled
        /// </summary>
        public bool IsEnabled { get; init; }

        /// <summary>
        /// The code of this <see cref="PlugIn"/>
        /// </summary>
        public string Code { get; init; } = string.Empty;
    }
}
