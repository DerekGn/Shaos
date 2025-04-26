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

using Microsoft.Extensions.Options;
using Shaos.Sdk;

namespace Shaos.Services.Runtime.Host
{
    /// <summary>
    /// A <see cref="Instance"/> host
    /// </summary>
    public interface IInstanceHost
    {
        /// <summary>
        /// An event that is raised when an <see cref="Instance"/> state changes
        /// </summary>
        event EventHandler<InstanceStateEventArgs> InstanceStateChanged;

        /// <summary>
        /// The <see cref="IReadOnlyList{T}"/> of <see cref="Instance"/>
        /// </summary>
        IReadOnlyList<Instance> Instances { get; }

        /// <summary>
        /// Create an <see cref="Instance"/> in the <see cref="InstanceHost"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="Instance"/></param>
        /// <param name="name">The name of the <see cref="Instance"/></param>
        /// <param name="assemblyFilePath">The path of the assembly file for the <see cref="Instance"/></param>
        /// <returns>The <see cref="Instance"/> that was added</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="id"/> is zero</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="assemblyFilePath"/> is null of empty</exception>
        /// <exception cref="InstanceExistsException">Thrown if the <see cref="InstanceHost"/> already contains an <see cref="Instance"/> with <paramref name="id"/></exception>
        Instance CreateInstance(
            int id,
            string name,
            string assemblyFilePath);

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <param name="plugIn"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="plugIn"/> is null</exception>
        /// <exception cref="InstanceNotFoundException">Thrown if an <see cref="Instance"/> </exception>
        Instance InitialiseInstance(
            int id,
            IPlugIn plugIn,
            IOptions<object>? options = default);

        /// <summary>
        /// Indicates if an <see cref="Instance"/> exists in the runtime
        /// </summary>
        /// <param name="id">The identifier of the <see cref="Instance"/> to check</param>
        /// <returns>true if the <see cref="Instance"/> exists in the <see cref="InstanceHost"/></returns>
        bool InstanceExists(int id);

        /// <summary>
        /// Remove an <see cref="Instance"/> from the <see cref="InstanceHost"/>
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="id"/> is zero</exception>
        /// <exception cref="InstanceRunningException">Thrown if the <see cref="Instance"/> is still running</exception>
        /// <exception cref="InstanceNotFoundException">Thrown if the <see cref="Instance"/> is not found</exception>
        void RemoveInstance(int id);

        /// <summary>
        /// Start the execution of a <see cref="Instance"/>
        /// </summary>
        /// <returns>An <see cref="Instance"/></returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="id"/> is zero</exception>
        /// <exception cref="InstanceNotFoundException">Thrown if the <see cref="Instance"/> is not found</exception>
        /// <remarks>
        /// The <see cref="Instance"/> is not synchronously started
        /// </remarks>
        Instance StartInstance(int id);

        /// <summary>
        /// Stop a running <see cref="Instance"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="Instance"/> that is to be stopped</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="id"/> is zero</exception>
        /// <exception cref="InstanceNotFoundException">Thrown if the <see cref="Instance"/> is not found</exception>
        /// <remarks>
        /// The <see cref="Instance"/> is not synchronously stopped
        /// </remarks>
        Instance StopInstance(int id);
    }
}