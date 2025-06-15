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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;

namespace Shaos.Services.Runtime
{
    /// <summary>
    /// An unloadable runtime assembly context
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("'{Name}' ({AssemblyPath})")]
    public class RuntimeAssemblyLoadContext : AssemblyLoadContext, IRuntimeAssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _assemblyDependencyResolver;
        private readonly AssemblyLoadContext _assemblyLoadContext;

        /// <summary>
        /// Create an instance of a <see cref="RuntimeAssemblyLoadContext"/>
        /// </summary>
        /// <param name="assemblyPath">The assembly path</param>
        public RuntimeAssemblyLoadContext(string assemblyPath)
            : this(GetAssemblyLoadContext(), assemblyPath, true)
        {
        }

        /// <summary>
        /// Create an instance of a <see cref="RuntimeAssemblyLoadContext"/>
        /// </summary>
        /// <param name="assemblyPath">The assembly path</param>
        /// <param name="isCollectable">A flag to indicate is this instance is collectable</param>
        public RuntimeAssemblyLoadContext(string assemblyPath,
                                          bool isCollectable) : this(GetAssemblyLoadContext(), assemblyPath, isCollectable)
        {
        }

        /// <summary>
        /// Create an instance of a <see cref="RuntimeAssemblyLoadContext"/>
        /// </summary>
        /// <param name="assemblyLoadContext">The assembly load context of the assembly</param>
        /// <param name="assemblyPath">The assembly path</param>
        /// <param name="isCollectable">A flag to indicate is this instance is collectable</param>
        public RuntimeAssemblyLoadContext(AssemblyLoadContext assemblyLoadContext,
                                          string assemblyPath,
                                          bool isCollectable) : base(Path.GetFileNameWithoutExtension(assemblyPath), isCollectable)
        {
            ArgumentNullException.ThrowIfNull(assemblyLoadContext);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(assemblyPath);

            _assemblyLoadContext = assemblyLoadContext;
            AssemblyPath = assemblyPath;
            _assemblyDependencyResolver = new AssemblyDependencyResolver(assemblyPath);
        }

        /// <inheritdoc/>
        public string AssemblyPath { get; }

        /// <inheritdoc/>
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            ArgumentNullException.ThrowIfNull(assemblyName);

            return _assemblyLoadContext.LoadFromAssemblyName(assemblyName);
        }

        /// <inheritdoc/>
        protected override nint LoadUnmanagedDll(string unmanagedDllName)
        {
            string? libraryPath = _assemblyDependencyResolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            IntPtr result = IntPtr.Zero;

            if (libraryPath != null)
            {
                result = LoadUnmanagedDllFromPath(libraryPath);
            }

            return result;
        }

        private static AssemblyLoadContext GetAssemblyLoadContext()
        {
            return AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly()) ?? AssemblyLoadContext.Default;
        }
    }
}