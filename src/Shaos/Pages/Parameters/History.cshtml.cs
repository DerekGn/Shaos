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
using Shaos.Extensions;
using Shaos.Pages.Parameters.Types;
using Shaos.Repository;
using Shaos.Repository.Models.Devices;
using Shaos.Repository.Models.Devices.Parameters;

namespace Shaos.Pages.Parameters
{
    public class HistoryModel : PageModel
    {
        private readonly IRepository _repository;

        public HistoryModel(IRepository repository)
        {
            var offsetUtcNow = DateTimeOffset
                .UtcNow
                .Truncate(TimeSpan.FromMinutes(1));

            StartDateTime = offsetUtcNow.Subtract(TimeSpan.FromHours(24));
            EndDateTime = offsetUtcNow;
            _repository = repository;

            Values = [];
        }

        [BindProperty]
        public IList<BaseValue> Values { get; init; }

        [BindProperty]
        public DateTimeOffset EndDateTime { get; init; }

        [BindProperty]
        public DateTimeOffset StartDateTime { get; init; }

        public async Task OnGetAsync(int parameterId,
                                     CancellationToken cancellationToken = default)
        {
            await foreach (var parameterValue in _repository.GetEnumerableAsync<BaseParameterValue>(_ => _.ParameterId == parameterId,
                                                                                                    cancellationToken: cancellationToken))
            {
                Values.Add(parameterValue.ToModel());
            }
        }

        public async Task OnPostApplyAsync(CancellationToken cancellationToken = default)
        {
        }
    }
}