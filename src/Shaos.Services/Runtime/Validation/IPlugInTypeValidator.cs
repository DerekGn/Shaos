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
using Shaos.Services.Runtime.Exceptions;
using System.Reflection;

namespace Shaos.Services.Runtime.Validation
{
    /// <summary>
    /// A <see cref="IPlugIn"/> type validator
    /// </summary>
    /// <remarks>
    /// Verifies an <see cref="Assembly"/> contains valid <see cref="IPlugIn"/> types
    /// </remarks>
    public interface IPlugInTypeValidator
    {
        /// <summary>
        /// Validate that an assembly contains valid <see cref="IPlugIn"/>
        /// </summary>
        /// <param name="assemblyFile">The fully qualified path to the <see cref="Assembly"/></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="assemblyFile"/> is empty or white space</exception>
        /// <exception cref="FileNotFoundException">Thrown if <paramref name="assemblyFile"/> does not exist</exception>
        /// <exception cref="PlugInTypeNotFoundException">Thrown if no <see cref="IPlugIn"/> derived types where found in the <paramref name="assemblyFile"/></exception>
        /// <exception cref="PlugInTypesFoundException">Thrown if multiple <see cref="IPlugIn"/> derived types where found in the <paramref name="assemblyFile"/></exception>
        /// <exception cref="PlugInDescriptionAttributeNotFoundException">Thrown when no <see cref="PlugInDescriptionAttribute"/> is found on <see cref="IPlugIn"/></exception>
        /// <returns>A <see cref="PlugInInformation"/> which contains information about a <see cref="IPlugIn"/> instance</returns>
        PlugInInformation Validate(string assemblyFile);
    }
}