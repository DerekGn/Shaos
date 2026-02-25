using Shaos.Repository.Models.Devices.Parameters;

namespace Shaos.Repository.Models
{
    /// <summary>
    /// A dashboard parameter
    /// </summary>
    public class DashboardParameter : BaseEntity
    {
        /// <summary>
        /// The parameter dashboard label
        /// </summary>
        public required string Label { get; set; } = string.Empty;

        /// <summary>
        /// The parameter data to display
        /// </summary>
        public BaseParameter? Parameter { get; set; }
    }
}