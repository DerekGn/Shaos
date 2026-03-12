using System.ComponentModel.DataAnnotations;

namespace Shaos.Pages.System.Dashboard.Model
{
    public class DashboardItemModel
    {
        public int Id { get; set; }

        [Required]
        public required string Label { get; set; }

        public ParameterModel? Parameter { get; set; } = null;
    }
}
