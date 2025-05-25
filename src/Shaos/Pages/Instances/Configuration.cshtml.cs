/*
* MIT License
*
* Copyright (c) 2025 Derek Goslin https://github.com/DerekGn
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shaos.Services;

namespace Shaos.Pages.Instances
{
    public class ConfigurationModel : PageModel
    {
        private readonly IInstanceHostService _instanceHostService;

        public ConfigurationModel(
            IInstanceHostService instanceHostService)
        {
            ArgumentNullException.ThrowIfNull(instanceHostService);

            _instanceHostService = instanceHostService;
        }

        [BindProperty]
        public object? Configuration { get; set; }

        public async Task<IActionResult> OnGetAsync(
            int id,
            CancellationToken cancellationToken)
        {
            Configuration = await _instanceHostService
                .LoadInstanceConfigurationAsync(id, cancellationToken);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(
            int id,
            CancellationToken cancellationToken)
        {
            var configurationSettings = Request.Form
                .Where(_ => _.Key.StartsWith(nameof(Configuration)))
                .Select(_ => new KeyValuePair<string, string>(_.Key.Replace($"{nameof(Configuration)}.", string.Empty), _.Value.First()));

            await _instanceHostService.UpdateInstanceConfigurationAsync(
                id,
                configurationSettings,
                cancellationToken);

            return RedirectToPage("./Index");
        }
    }
}
