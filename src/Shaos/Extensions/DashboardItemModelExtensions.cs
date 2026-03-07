using Shaos.Pages.System.Dashboard.Model;
using Shaos.Repository.Models;

namespace Shaos.Extensions
{
    public static class DashboardItemModelExtensions
    {
        public static DashboardItem FromModel(this DashboardItemModel dashboardItemModel)
        {
            return new DashboardItem()
            {
                Label = dashboardItemModel.Label
            };
        }
    }
}
