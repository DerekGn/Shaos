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

using Shaos.Api.Model.v1;
using ApiPlugIn = Shaos.Api.Model.v1.PlugIn;

namespace Shaos.Services
{
    public interface IPlugInService
    {
        Task<int> CreatePlugInAsync(
            string name,
            string? description,
            string code,
            CancellationToken cancellationToken);

        Task DeletePlugInAsync(
            int id,
            CancellationToken cancellationToken);

        Task<ApiPlugIn?> GetPlugInByIdAsync(
            int id,
            CancellationToken cancellationToken);

        Task<ApiPlugIn?> GetPlugInByNameAsync(
            string name,
            CancellationToken cancellationToken);

        IAsyncEnumerable<ApiPlugIn> GetPlugInsAsync(
            CancellationToken cancellationToken);
        
        Task<PlugInStatus> GetPlugInStatusAsync(
            int id,
            CancellationToken cancellationToken);
        
        IAsyncEnumerable<PlugInStatus> GetPlugInStatusesAsync(
            CancellationToken cancellationToken);

        Task SetPlugInIsEnabledStateAsync(
            int id,
            bool isEnabled,
            CancellationToken cancellationToken);
        Task StartPlugInAsync(
            int id,
            CancellationToken cancellationToken);
        
        Task StopPlugInAsync(
            int id,
            CancellationToken cancellationToken);
        
        Task<ApiPlugIn?> UpdatePlugInAsync(
            int id, 
            string name,
            string? description,
            string code,
            CancellationToken cancellationToken);
    }
}