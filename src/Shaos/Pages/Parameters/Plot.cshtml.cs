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
using Shaos.Charting;
using Shaos.Exceptions;
using Shaos.Repository;
using Shaos.Repository.Models.Devices.Parameters;
using System.Reflection.Metadata;

namespace Shaos.Pages.Parameters
{
    public class PlotModel : PageModel
    {
        private readonly IRepository _repository;

        public PlotModel(IRepository repository)
        {
            _repository = repository;
        }

        [BindProperty]
        public ChartSettings Settings { get; set; }

        public async Task OnGetAsync(int id, CancellationToken cancellationToken)
        {
            var parameter = await _repository.GetByIdAsync<BaseParameter>(id,
                                                                          cancellationToken: cancellationToken);

            if (parameter != null)
            {
                try
                {
                    Settings = MapParameterToSettings(parameter);


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

        private static ChartSettings MapParameterToSettings(BaseParameter parameter)
        {
            var type = parameter.GetType();

            switch (type)
            {
                case Type _ when type == typeof(BoolParameter):
                    return new BoolChartSettings()
                    {
                        Min = false,
                        Max = true
                    };

                case Type _ when type == typeof(FloatParameter):
                    var floatParameter = (FloatParameter)parameter;
                    return new FloatChartSettings()
                    {
                        Min = floatParameter.Min,
                        Max = floatParameter.Max
                    };

                case Type _ when type == typeof(IntParameter):
                    var intParameter = (IntParameter)parameter;
                    return new IntChartSettings()
                    {
                        Min = intParameter.Min,
                        Max = intParameter.Max
                    };

                case Type _ when type == typeof(UIntParameter):
                    var uintParameter = (UIntParameter)parameter;
                    return new UIntChartSettings()
                    {
                        Min = uintParameter.Min,
                        Max = uintParameter.Max
                    };
            }

            throw new ParameterPlotNotSupportedException();
        }
    }
}