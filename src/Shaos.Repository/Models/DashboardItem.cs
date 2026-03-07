using Shaos.Repository.Models.Devices.Parameters;

namespace Shaos.Repository.Models
{
    /// <summary>
    /// A dashboard item
    /// </summary>
    public class DashboardItem : BaseEntity
    {
        /// <summary>
        /// The parameter dashboard label
        /// </summary>
        public required string Label { get; set; } = string.Empty;

        /// <summary>
        /// The parameter
        /// </summary>
        public BaseParameter? Parameter { get; set; } = null;
    }
}