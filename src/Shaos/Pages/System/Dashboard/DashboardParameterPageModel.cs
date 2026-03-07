using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shaos.Repository;
using Shaos.Repository.Models.Devices.Parameters;

namespace Shaos.Pages.System.Dashboard
{
    public class DashboardParameterPageModel : PageModel
    {
        internal protected readonly IShaosRepository Repository;

        public DashboardParameterPageModel(IShaosRepository repository)
        {
            Repository = repository;
        }

        public SelectList? ParametersList { get; set; } = default;

        public void PopulateParametersDropDownList(object selectedParameter = null!)
        {
            var parametersQuery = Repository.GetQueryable<BaseParameter>().OrderBy(_ => _.Name);

            ParametersList = new SelectList(parametersQuery.AsNoTracking(),
                                            nameof(BaseParameter.Id),
                                            nameof(BaseParameter.Name),
                                            selectedParameter);
        }
    }
}
