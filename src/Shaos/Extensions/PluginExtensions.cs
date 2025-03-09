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

using PlugInApi = Shaos.Api.Model.v1.PlugIn;
using ModelPlugIn = Shaos.Repository.Models.PlugIn;

namespace Shaos.Extensions
{
    internal static class PlugInExtensions
    {
        public static PlugInApi ToApiModel(this ModelPlugIn plugIn)
        {
            return new PlugInApi()
            {
                CreatedDate = plugIn.CreatedDate,
                Description = plugIn.Description,
                Id = plugIn.Id,
                Instances = plugIn.Instances.Select(_ => _.ToApiModel()).ToList(),
                Name = plugIn.Name,
                NuGetPackage = plugIn.NuGetPackage?.ToApiModel(),
                UpdatedDate = plugIn.UpdatedDate,
            };
        }
    }
}
