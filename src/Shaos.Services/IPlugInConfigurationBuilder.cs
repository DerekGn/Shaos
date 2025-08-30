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
using System.Reflection;

namespace Shaos.Services
{
    /// <summary>
    /// A <see cref="IPlugIn"/> configuration builder
    /// </summary>
    public interface IPlugInConfigurationBuilder
    {
        /// <summary>
        /// Load a <see cref="IPlugIn"/> configuration instance from an <see cref="Assembly"/>
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to load the configuration instance</param>
        /// <param name="configuration">The optional json configuration to apply to the configuration instance</param>
        object? LoadConfiguration(Assembly assembly,
                                  string? configuration);

        /// <summary>
        /// Load the configuration type from a PlugIn assembly
        /// </summary>
        /// <param name="plugInFolder">The folder where the PlugIn was extracted</param>
        /// <param name="plugInAssemblyFileName">The PlugIn assembly file name</param>
        /// <param name="configuration">The optional json configuration to apply to the configuration instance</param>
        /// <returns>An instance of a configuration type</returns>
        object? LoadConfiguration(string plugInFolder,
                                  string plugInAssemblyFileName,
                                  string? configuration = default);
    }
}