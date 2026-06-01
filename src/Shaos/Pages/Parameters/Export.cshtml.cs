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
using Shaos.Extensions;
using Shaos.Repository;
using Shaos.Repository.Models.Devices.Parameters;

namespace Shaos.Pages.Parameters
{
    public class ExportModel : BaseDateRangePageModel
    {
        public ExportModel(IRepository repository) : base(repository)
        {
        }

        public void OnGet(int id,
                          int deviceId)
        {
            Id = id;
            DeviceId = deviceId;
        }

        public async Task<IActionResult> OnPostExportAsync(int id,
                                                           int deviceId,
                                                           CancellationToken cancellationToken = default)
        {
            Id = id;
            DeviceId = deviceId;

            var parameter = await Repository.GetByIdAsync<BaseParameter>(id,
                                                                         cancellationToken: cancellationToken);

            if(parameter is null)
            {
                ModelState.AddModelError(string.Empty,
                                         $"Parameter Id [{id}] was not found.");

                return new EmptyResult();
            }

            Response.ContentType = "text/csv";
            Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{parameter.Name}.csv\"");

            var stream = Response.BodyWriter.AsStream();

            var streamWriter = new StreamWriter(stream);

            await streamWriter.WriteLineAsync($"value,timestamp(utc){Environment.NewLine}");

            await foreach (var item in Repository.GetEnumerableAsync<BaseParameterValue>(_ => _.ParameterId == id && (_.TimeStamp >= StartDateTime.UtcDateTime && _.TimeStamp <= EndDateTime.UtcDateTime),
                                                                                         cancellationToken: cancellationToken))
            {
                await streamWriter.WriteAsync(item.ToCsv());
            }

            return new EmptyResult();
        }
    }
}