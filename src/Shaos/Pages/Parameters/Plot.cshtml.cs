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
using Shaos.Exceptions;
using Shaos.Plotting;
using Shaos.Repository;
using Shaos.Repository.Models.Devices.Parameters;

namespace Shaos.Pages.Parameters
{
    public class PlotModel : PageModel
    {
        private readonly IShaosRepository _repository;

        public PlotModel(IShaosRepository repository)
        {
            _repository = repository;
        }

        [BindProperty]
        public PlotSettings Settings { get; set; }

        public async Task OnGetAsync(int id, CancellationToken cancellationToken)
        {
            var parameter = await _repository.GetFirstOrDefaultAsync<BaseParameter>(_ => _.InstanceId == id,
                                                                                    cancellationToken: cancellationToken);

            if (parameter != null)
            {
                try
                {
                    Settings = new PlotSettings()
                    {
                        Id = id,
                        Label = parameter.Name
                    };
                }
                catch (ParameterPlotNotSupportedException)
                {
                    ModelState.AddModelError(string.Empty, $"Parameter [{id}] does not support plotting.");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, $"Parameter [{id}] was not found.");
            }
        }
    }
}