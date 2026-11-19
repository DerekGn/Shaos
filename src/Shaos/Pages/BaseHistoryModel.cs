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
using Microsoft.Extensions.Options;
using Shaos.Extensions;
using Shaos.Graphing;
using Shaos.Options;
using Shaos.Pages.Parameters.Types;
using Shaos.Repository;
using Shaos.Repository.Models.Devices.Parameters;

namespace Shaos.Pages
{
    public abstract class BaseHistoryModel : BaseDateRangePageModel
    {
        public const string ViewDataKey = "history";
        protected IOptions<PlotOptions> Options;

        protected BaseHistoryModel(IRepository repository,
                                   IOptions<PlotOptions> options,
                                   ILogger<BaseHistoryModel> logger) : base(repository, logger)
        {
            Options = options;
        }

        [BindProperty]
        public ParameterHistory History { get; set; }

        [BindProperty]
        public GraphSettings Settings { get; set; }

        public async Task OnGetAsync(int id,
                                     int deviceId,
                                     CancellationToken cancellationToken = default)
        {
            Id = id;
            DeviceId = deviceId;
            History = await QueryParameterValueDataAsync(id,
                                                         cancellationToken);
        }

        public async Task OnPostApplyAsync(int id,
                                           int deviceId,
                                           CancellationToken cancellationToken = default)
        {
            Id = id;
            DeviceId = deviceId;
            History = await QueryParameterValueDataAsync(id,
                                                         cancellationToken);
        }

        private async Task<ParameterHistory> QueryParameterValueDataAsync(int parameterId,
                                                                CancellationToken cancellationToken)
        {
            ParameterHistory? parameterHistory = new();

            var parameter = await Repository.GetByIdAsync<BaseParameter>(parameterId,
                                                                         true,
                                                                         cancellationToken: cancellationToken);

            if (parameter is not null)
            {
                Settings = parameter.ToSettings(DeviceId,
                                                Options.Value.Duration);

                var values = await Repository.GetEnumerableAsync<BaseParameterValue>(_ => _.ParameterId == parameterId && (_.TimeStamp >= StartDateTime.UtcDateTime && _.TimeStamp <= EndDateTime.UtcDateTime),
                                                                                     cancellationToken: cancellationToken)
                                                                                     .Select(_ => _.ToModel())
                                                                                     .ToListAsync(cancellationToken: cancellationToken);
                parameterHistory = new ParameterHistory()
                {
                    Label = parameter.Name,
                    Units = parameter.Units,
                    Values = values
                };
            }
            else
            {
                AlertError($"Parameter Id [{parameterId}] was not found");
            }

            return parameterHistory;
        }
    }
}