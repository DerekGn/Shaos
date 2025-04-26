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

using Shaos.Sdk;

namespace Shaos.Services.Runtime.Host
{
    /// <summary>
    /// The execution state of a <see cref="Instance"/>
    /// </summary>
    public enum InstanceState
    {
        /// <summary>
        /// The initial <see cref="Instance"/> state
        /// </summary>
        None,
        /// <summary>
        /// The <see cref="Instance"/> is starting
        /// </summary>
        Starting,
        /// <summary>
        /// The <see cref="Instance"/> is running
        /// </summary>
        Running,
        /// <summary>
        /// The <see cref="Instance"/> is complete.
        /// </summary>
        /// <remarks>
        /// This is the result of the <see cref="Instance"/> self terminating or <see cref="Instance"/> being stopped.
        /// </remarks>
        Complete,
        /// <summary>
        /// The <see cref="Instance"/> is faulted
        /// </summary>
        /// <remarks>
        /// This is the result of the <see cref="Instance"/> self terminating due to an exception.
        /// </remarks>
        Faulted,
        /// <summary>
        /// The <see cref="Instance"/> <see cref="IPlugIn"/> was loaded from its <see cref="Assembly"/>
        /// </summary>
        PlugInLoaded,
        /// <summary>
        /// The <see cref="Instance"/> <see cref="IPlugIn"/> load failed.
        /// </summary>
        PlugInLoadFailure,
        /// <summary>
        /// The <see cref="Instance"/> <see cref="IPlugIn"/> is loading from its <see cref="Assembly"/>
        /// </summary>
        AwaitingInitialisation
    }
}