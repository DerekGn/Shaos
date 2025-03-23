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

using Shaos.Repository.Models;

namespace Shaos.Services.Runtime
{
    /// <summary>
    /// The runtime service for executing <see cref="PlugInInstance"/>
    /// </summary>
    public interface IRuntimeService
    {
        /// <summary>
        /// Get a <see cref="ExecutingInstance"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="ExecutingInstance"/></param>
        /// <returns>The <see cref="ExecutingInstance"/> if found else null</returns>
        ExecutingInstance? GetExecutingInstance(int id);

        /// <summary>
        /// Get the <see cref="IEnumerable{T}"/> of <see cref="ExecutingInstance"/>
        /// </summary>
        /// <returns>The <see cref="IEnumerable{T}"/> of <see cref="ExecutingInstance"/></returns>
        public IEnumerable<ExecutingInstance> GetExecutingInstances();

        /// <summary>
        /// Start the execution of a <see cref="PlugInInstance"/>
        /// </summary>
        /// <param name="plugIn">The <see cref="PlugIn"/></param>
        /// <param name="plugInInstance">The <see cref="PlugInInstance"/></param>
        /// <returns>An <see cref="ExecutingInstance"/></returns>
        ExecutingInstance StartInstance(
            PlugIn plugIn,
            PlugInInstance plugInInstance);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        void StopInstance(int id);
    }
}