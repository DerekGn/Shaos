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
using Shaos.Repository.Models;
using Shaos.Services.Logging;
using Shaos.Services.Repositories;

namespace Shaos.Pages.Logging
{
    public class OverrideModel : PageModel
    {
        private readonly ILoggingConfiguration _loggingConfiguration;
        private readonly ILoggingConfigurationRepository _loggingConfigurationRepository;

        public OverrideModel(
            ILoggingConfiguration loggingConfiguration,
            ILoggingConfigurationRepository loggingConfigurationRepository)
        {
            _loggingConfiguration = loggingConfiguration;
            _loggingConfigurationRepository = loggingConfigurationRepository ?? throw new ArgumentNullException(nameof(loggingConfigurationRepository));
        }

        [BindProperty]
        public LogLevelSwitch LogLevelSwitch { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return NotFound();
            }

            var logLevelSwitch = await _loggingConfigurationRepository.GetByNameAsync(name, cancellationToken);

            if (logLevelSwitch == null)
            {
                LogLevelSwitch = new LogLevelSwitch()
                {
                    Name = name
                };
            }
            else
            {
                LogLevelSwitch = logLevelSwitch;
            }

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _loggingConfiguration.LoggingLevelSwitches[LogLevelSwitch.Name].MinimumLevel = LogLevelSwitch.Level;

            await _loggingConfigurationRepository.UpsertAsync(LogLevelSwitch.Name, LogLevelSwitch.Level, cancellationToken);

            return RedirectToPage("./Index");
        }
    }
}