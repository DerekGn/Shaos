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

namespace Shaos.Services.Runtime.Validation
{
    /// <summary>
    /// Represents <see cref="IPlugIn"/> type information
    /// </summary>
    public class PlugInTypeInformation
    {
        /// <summary>
        /// Create an instance of a <see cref="PlugInTypeInformation"/>
        /// </summary>
        /// <param name="name">The <see cref="IPlugIn"/> derived type name</param>
        /// <param name="hasLogger">Indicates if the <see cref="IPlugIn"/> derived type has a logger</param>
        /// <param name="hasConfiguration">Indicates if the <see cref="IPlugIn"/> derived type has configuration</param>
        /// <param name="assemblyVersion">The assembly version</param>
        public PlugInTypeInformation(string name,
                                     bool hasLogger,
                                     bool hasConfiguration,
                                     string assemblyVersion)
        {
            Name = name;
            HasLogger = hasLogger;
            HasConfiguration = hasConfiguration;
            AssemblyVersion = assemblyVersion;
        }

        /// <summary>
        /// The <see cref="IPlugIn"/> derived type assembly version
        /// </summary>
        public string AssemblyVersion { get; }

        /// <summary>
        /// Indicates if the <see cref="IPlugIn"/> derived type has a configuration type
        /// </summary>
        public bool HasConfiguration { get; }

        /// <summary>
        /// Indicates if the <see cref="IPlugIn"/> derived type has a logger
        /// </summary>
        public bool HasLogger { get; }

        /// <summary>
        /// The <see cref="IPlugIn"/> derived type name
        /// </summary>
        public string Name { get; }
    }
}