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

using System.Reflection;

namespace Shaos.Api.Model.v1
{
    /// <summary>
    /// The execution state of a <see cref="PlugIn"/>
    /// </summary>
    public enum ExecutionState
    {
        /// <summary>
        /// The initial <see cref="ExecutingInstance"/> state
        /// </summary>
        None,
        /// <summary>
        /// The <see cref="ExecutingInstance"/> is activating
        /// </summary>
        Activating,
        /// <summary>
        /// The <see cref="ExecutingInstance"/> is activation faulted
        /// </summary>
        ActivationFaulted,
        /// <summary>
        /// The <see cref="ExecutingInstance"/> is activate
        /// </summary>
        Active,
        /// <summary>
        /// The <see cref="ExecutingInstance"/> is complete.
        /// </summary>
        /// <remarks>
        /// This is the result of the <see cref="ExecutingInstance"/> self terminating or <see cref="ExecutingInstance"/> being stopped.
        /// </remarks>
        Complete,
        /// <summary>
        /// The <see cref="ExecutingInstance"/> is faulted
        /// </summary>
        /// <remarks>
        /// This is the result of the <see cref="ExecutingInstance"/> self terminating due to an exception.
        /// </remarks>
        Faulted,
        /// <summary>
        /// The <see cref="ExecutingInstance"/> <see cref="PlugIn"/> was loaded from its <see cref="Assembly"/>
        /// </summary>
        PlugInLoaded,
        /// <summary>
        /// The <see cref="ExecutingInstance"/> <see cref="PlugIn"/> load failed.
        /// </summary>
        PlugInLoadFailure,
        /// <summary>
        /// The <see cref="ExecutingInstance"/> <see cref="PlugIn"/> is loading from its <see cref="Assembly"/>
        /// </summary>
        PlugInLoading
    }
}