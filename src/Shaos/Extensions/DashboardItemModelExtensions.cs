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
                Id = dashboardItemModel.Id,
                Label = dashboardItemModel.Label
            };
        }

        public static DashboardItemModel ToModel(this DashboardItem dashboardItem)
        {
            return new DashboardItemModel()
            {
                Id = dashboardItem.Id,
                Label = dashboardItem.Label,
                Parameter = new ParameterModel()
                {
                    Id = dashboardItem.Parameter!.Id
                }
            };
        }
    }
}
