using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shaos.Sdk;
using Shaos.Services.Repositories;
using Shaos.Services.Runtime.Host;

namespace Shaos.Pages.PlugInInstances
{
    public class ConfigurationModel : PageModel
    {
        private readonly IPlugInInstanceRepository _repository;

        public ConfigurationModel(
            IInstanceHost instanceHost,
            IPlugInInstanceRepository repository)
        {
            ArgumentNullException.ThrowIfNull(instanceHost);

            _repository = repository;
        }

        [BindProperty]
        public BasePlugInConfiguration Configuration { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var plugInInstance = await _repository.GetByIdAsync(id);

            if (plugInInstance != null)
            {
                ModelState.AddModelError("NotFound", $"PlugInInstance: [{id}] was not found");
            }

            return Page();
        }

        //public async Task<IActionResult> OnPostAsync()
        //{ }
    }
}
