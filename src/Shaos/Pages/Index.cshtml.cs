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
using Shaos.Repository;
using Shaos.Repository.Models;
using Shaos.Services.Exceptions;
using Shaos.Services.Parameters;

namespace Shaos.Pages
{
    [Authorize]
    public class IndexModel : BasePageModel
    {
        private readonly IParameterService _parameterService;
        private readonly IRepository _repository;

        public IndexModel(IRepository repository,
                          ILogger<IndexModel> logger,
                          IParameterService parameterService) : base(logger)
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
            await ExecuteParameterWriteAsync(async () =>
            {
                await _parameterService.WriteParameterAsync(id,
                                                            value,
                                                            cancellationToken);
            }, cancellationToken);
        }

        public async Task OnPostWriteFloatAsync(int id,
                                                float value,
                                                CancellationToken cancellationToken = default)
        {
            await ExecuteParameterWriteAsync(async () =>
            {
                await _parameterService.WriteParameterAsync(id,
                                                            value,
                                                            cancellationToken);
            }, cancellationToken);
        }

        public async Task OnPostWriteIntegerAsync(int id,
                                                  int value,
                                                  CancellationToken cancellationToken = default)
        {
            await ExecuteParameterWriteAsync(async () =>
            {
                await _parameterService.WriteParameterAsync(id,
                                                            value,
                                                            cancellationToken);
            }, cancellationToken);
        }

        public async Task OnPostWriteUnsignedIntegerAsync(int id,
                                                          uint value,
                                                          CancellationToken cancellationToken = default)
        {
            await ExecuteParameterWriteAsync(async () =>
            {
                await _parameterService.WriteParameterAsync(id,
                                                            value,
                                                            cancellationToken);
            }, cancellationToken);
        }

        private async Task ExecuteParameterWriteAsync(Func<Task> update,
                                                      CancellationToken cancellationToken = default)
        {
            try
            {
                await LoadItemsAsync(cancellationToken);

                await update();
            }
            catch (ParameterInvalidTypeException exception)
            {
                AlertError($"Parameter [{exception.Id}] is invalid type. Expected Type: [{exception.Expected.Name}] Actual Type: [{exception.Actual.Name}]");
            }
            catch (ParameterNotFoundException exception)
            {
                AlertError($"Parameter [{exception.Id}] was not found");
            }
            catch (ParameterNotWritableException exception)
            {
                AlertError($"Parameter [{exception.Id}] is not writable");
            }
        }

        private async Task LoadItemsAsync(CancellationToken cancellationToken)
        {
            Items = await _repository
                            .GetAsync<DashboardItem>(includeProperties: [nameof(DashboardItem.Parameter)],
                                                     cancellationToken: cancellationToken);
        }
    }
}