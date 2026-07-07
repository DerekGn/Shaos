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

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.Json;

namespace Shaos.Filters
{
    public class SerializeModelStatePageFilter : IPageFilter
    {
        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            if (context.HandlerInstance is not PageModel page)
                return;
            if (page.ModelState.IsValid)
                return;
            if (context.Result is IKeepTempDataResult)
            {
                var modelState = SerializeModelState(page.ModelState);
                page.TempData[nameof(SerializeModelStatePageFilter)] = modelState;
            }
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        { }

        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
            if (context.HandlerInstance is not PageModel page)
                return;

            var serializedModelState = page.TempData[nameof(SerializeModelStatePageFilter)] as string;

            if (string.IsNullOrWhiteSpace(serializedModelState))
                return;

            var modelState = DeserializeModelState(serializedModelState);

            page.ModelState.Merge(modelState);
        }

        private static ModelStateDictionary DeserializeModelState(string serialisedErrorList)
        {
            var modelState = new ModelStateDictionary();
            var errorList = JsonSerializer.Deserialize<List<ModelStateTransferValue>>(serialisedErrorList);

            if (errorList != null)
            {
                foreach (var item in errorList)
                {
                    modelState.SetModelValue(item.Key, item.RawValue, item.AttemptedValue);
                    foreach (var error in item.ErrorMessages)
                    {
                        modelState.AddModelError(item.Key, error);
                    }
                }
            }

            return modelState;
        }

        private static string SerializeModelState(ModelStateDictionary modelState)
        {
            List<ModelStateTransferValue> values = [];

            foreach (var kvp in modelState)
            {
                var attemptedValue = kvp.Value?.AttemptedValue;
                var rawValue = kvp.Value?.RawValue;

                values.Add(new ModelStateTransferValue()
                {
                    Key = kvp.Key,
                    AttemptedValue = attemptedValue,
                    RawValue = rawValue,
                    ErrorMessages = kvp.Value == null ? [] : kvp.Value.Errors.Select(_ => _.ErrorMessage).ToList()
                });
            }

            return JsonSerializer.Serialize(values);
        }

        internal class ModelStateTransferValue
        {
            public string? AttemptedValue { get; set; }
            public ICollection<string> ErrorMessages { get; set; } = [];
            public required string Key { get; set; }
            public object? RawValue { get; set; }
        }
    }
}