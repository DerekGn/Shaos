
using System.ComponentModel.DataAnnotations;

namespace Shaos.Api.Model.v1
{
    public abstract record Base
    {
        /// <summary>
        /// The identifier
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// The created date
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The updated date
        /// </summary>
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }
}
