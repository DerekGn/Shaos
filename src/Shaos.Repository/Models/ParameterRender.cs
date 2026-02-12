using Shaos.Repository.Models.Devices.Parameters;

namespace Shaos.Repository.Models
{
    /// <summary>
    /// The <see cref="BaseParameter"/> render
    /// </summary>
    public class ParameterRender : BaseEntity
    {
        /// <summary>
        /// The parameter to render
        /// </summary>
        public BaseParameter Parameter { get; set; } = null!;

        /// <summary>
        /// The parameter identifier
        /// </summary>
        public int ParameterId { get; set; }
    }
}