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
        /// An event that is raised when an <see cref="Instance"/> state changes
        /// </summary>
        event EventHandler<InstanceStateEventArgs> InstanceStateChanged;

        /// <summary>
        /// Get a <see cref="Instance"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="Instance"/></param>
        /// <returns>The <see cref="Instance"/> if found else null</returns>
        Instance? GetInstance(int id);

        /// <summary>
        /// Get the <see cref="IReadOnlyList{T}"/> of <see cref="Instance"/>
        /// </summary>
        /// <returns>The <see cref="IReadOnlyList{T}"/> of <see cref="Instance"/></returns>
        public IReadOnlyList<Instance> GetInstances();

        /// <summary>
        /// Start the execution of a <see cref="PlugInInstance"/>
        /// </summary>
        /// <param name="plugIn">The <see cref="PlugIn"/></param>
        /// <param name="plugInInstance">The <see cref="PlugInInstance"/></param>
        /// <returns>An <see cref="Instance"/></returns>
        Instance StartInstance(
            PlugIn plugIn,
            PlugInInstance plugInInstance);

        /// <summary>
        /// Stop a running instance
        /// </summary>
        /// <param name="id">The identifier of the instance that is to be stopped</param>
        void StopInstance(int id);
    }
}