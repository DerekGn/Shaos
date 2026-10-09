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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shaos.Repository;
using Shaos.Repository.Models;
using Shaos.Services.Exceptions;
using Shaos.Services.Parameters;
using System.Net;

namespace Shaos.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IRepository _repository;
        private readonly IParameterService _parameterService;

        public IndexModel(IRepository repository,
                          IParameterService parameterService)
        {
            _repository = repository;
            _parameterService = parameterService;
        }

        public IList<DashboardItem> Items { get; set; } = default!;

        public async Task OnGetAsync(CancellationToken cancellationToken = default)
        {
            await LoadItemsAsync(cancellationToken);
        }

        public async Task OnPostWriteBooleanAsync(int id,
                                                  bool value,
                                                  CancellationToken cancellationToken = default)
        {
            await LoadItemsAsync(cancellationToken);
        }

        public async Task OnPostWriteFloatAsync(int id,
                                                float value,
                                                CancellationToken cancellationToken = default)
        {
            await LoadItemsAsync(cancellationToken);
        }

        public async Task OnPostWriteIntegerAsync(int id,
                                                  int value,
                                                  CancellationToken cancellationToken = default)
        {
            await LoadItemsAsync(cancellationToken);
        }

        public async Task OnPostWriteUnsignedIntegerAsync(int id,
                                                          uint value,
                                                          CancellationToken cancellationToken = default)
        {
            await LoadItemsAsync(cancellationToken);
        }

        private async Task LoadItemsAsync(CancellationToken cancellationToken)
        {
            Items = await _repository
                            .GetAsync<DashboardItem>(includeProperties: [nameof(DashboardItem.Parameter)],
                                                     cancellationToken: cancellationToken);
        }

        private async Task<ActionResult> ExecuteParameterWriteAsync(Func<Task<ActionResult>> update)
        {
            try
            {
                return await update();
            }
            catch (ParameterInvalidTypeException invalidType)
            {
                return NotFound(CreateProblemDetails(HttpStatusCode.NotFound,
                                                     $"Parameter [{invalidType.Id}] is invalid type. Expected Type: [{invalidType.Expected}] Actual Type: [{invalidType.ActualType}]"));
            }
            catch (ParameterNotFoundException notFound)
            {
                return NotFound(CreateProblemDetails(HttpStatusCode.NotFound,
                                                     $"Parameter [{notFound.Id}] not found"));
            }
            catch (ParameterNotWritableException notWritable)
            {
                return NotFound(CreateProblemDetails(HttpStatusCode.NotFound,
                                                     $"Parameter [{notWritable.Id}] is not writable"));
            }
        }
    }
}