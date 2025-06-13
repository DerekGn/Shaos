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

namespace Shaos.Services.Runtime
{
    /// <summary>
    /// Represents an un-loadable assembly load context
    /// </summary>
    public interface IRuntimeAssemblyLoadContext
    {
        /// <summary>
        /// The path to the loaded assembly
        /// </summary>
        string AssemblyPath { get; }

        /// <summary>
        /// Load an <see cref="Assembly"/> from the <paramref name="assemblyPath"/>
        /// </summary>
        /// <param name="assemblyPath">The path to the <see cref="Assembly"/> to load</param>
        /// <returns>The loaded <see cref="Assembly"/> if found</returns>
        Assembly LoadFromAssemblyPath(string assemblyPath);

        /// <summary>
        /// Unload the <see cref="IRuntimeAssemblyLoadContext"/> instance
        /// </summary>
        void Unload();
    }
}