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
using Shaos.Sdk;
using Shaos.Services.Runtime.Exceptions;

namespace Shaos.Services.Runtime.Host
{
    /// <summary>
    /// A <see cref="RuntimeInstance"/> host
    /// </summary>
    public interface IRuntimeInstanceHost
    {
        /// <summary>
        /// An event that is raised when an <see cref="RuntimeInstance"/> state changes
        /// </summary>
        event EventHandler<RuntimeInstanceStateEventArgs> InstanceStateChanged;

        /// <summary>
        /// The <see cref="IReadOnlyList{T}"/> of <see cref="RuntimeInstance"/>
        /// </summary>
        IReadOnlyList<RuntimeInstance> Instances { get; }

        /// <summary>
        /// Create an <see cref="RuntimeInstance"/> in the <see cref="RuntimeInstanceHost"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="RuntimeInstance"/></param>
        /// <param name="plugInId"></param>
        /// <param name="instanceName">The name of the <see cref="RuntimeInstance"/></param>
        /// <param name="assemblyPath">The path of the assembly file for the <see cref="RuntimeInstance"/></param>
        /// <param name="configurable"></param>
        /// <returns>The <see cref="RuntimeInstance"/> that was added</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="id"/> is zero</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="instanceName"/> or <paramref name="assemblyPath"/> is null of empty</exception>
        /// <exception cref="InstanceExistsException">Thrown if the <see cref="RuntimeInstanceHost"/> already contains an <see cref="RuntimeInstance"/> with <paramref name="id"/></exception>
        /// <exception cref="MaxInstancesRunningException">Thrown if the maximum number of instances are loaded</exception>
        RuntimeInstance CreateInstance(int id,
                                       int plugInId,
                                       string instanceName,
                                       string assemblyPath,
                                       bool configurable = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        RuntimeInstanceLoadContext GetInstanceLoadContext(int id);

        /// <summary>
        /// Indicates if an <see cref="RuntimeInstance"/> exists in the runtime
        /// </summary>
        /// <param name="id">The identifier of the <see cref="RuntimeInstance"/> to check</param>
        /// <returns>true if the <see cref="RuntimeInstance"/> exists in the <see cref="RuntimeInstanceHost"/></returns>
        bool InstanceExists(int id);

        /// <summary>
        /// Remove an <see cref="RuntimeInstance"/> from the <see cref="RuntimeInstanceHost"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="RuntimeInstance"/> to remove</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="id"/> is zero</exception>
        /// <exception cref="InstanceRunningException">Thrown if the <see cref="RuntimeInstance"/> is still running</exception>
        /// <exception cref="InstanceNotFoundException">Thrown if the <see cref="RuntimeInstance"/> is not found</exception>
        void RemoveInstance(int id);

        /// <summary>
        /// Start a <see cref="IPlugIn"/> instance
        /// </summary>
        /// <param name="id">The <see cref="PlugInInstance"/> identifier</param>
        /// <param name="plugIn">The <see cref="IPlugIn"/> instance</param>
        /// <returns>The <see cref="RuntimeInstance"/></returns>
        RuntimeInstance StartInstance(int id,
                                      IPlugIn plugIn);

        /// <summary>
        /// Stop a running <see cref="RuntimeInstance"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="RuntimeInstance"/> that is to be stopped</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="id"/> is zero</exception>
        /// <exception cref="InstanceNotFoundException">Thrown if the <see cref="RuntimeInstance"/> is not found</exception>
        /// <remarks>
        /// The <see cref="RuntimeInstance"/> is not synchronously stopped
        /// </remarks>
        RuntimeInstance StopInstance(int id);
    }
}