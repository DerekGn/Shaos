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

using Microsoft.Extensions.Logging;
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
        private readonly ILogger<RuntimeAssemblyLoadContext> _logger;

        /// <summary>
        /// Create an instance of a <see cref="RuntimeAssemblyLoadContext"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="assemblyPath">The assembly path</param>
        public RuntimeAssemblyLoadContext(ILogger<RuntimeAssemblyLoadContext> logger,
                                          string assemblyPath) : this(logger, assemblyPath, true)
        {
        }

        /// <summary>
        /// Create an instance of a <see cref="RuntimeAssemblyLoadContext"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="assemblyPath">The assembly path</param>
        /// <param name="isCollectable">A flag to indicate is this instance is collectable</param>
        public RuntimeAssemblyLoadContext(ILogger<RuntimeAssemblyLoadContext> logger,
                                          string assemblyPath,
                                          bool isCollectable) : base(Path.GetFileNameWithoutExtension(assemblyPath), isCollectable)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(assemblyPath);

            _logger = logger;
            AssemblyPath = assemblyPath;
            _assemblyDependencyResolver = new AssemblyDependencyResolver(assemblyPath);
        }

        /// <inheritdoc/>
        public string AssemblyPath { get; }

        /// <inheritdoc/>
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            ArgumentNullException.ThrowIfNull(assemblyName);

            _logger.LogDebug("");

            Assembly? assembly = null;

            try
            {
                assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
            }
            catch (Exception ex)
            {
            }

            if (assembly == null)
            {
                var assemblyPath = _assemblyDependencyResolver.ResolveAssemblyToPath(assemblyName);

                assembly = LoadFromAssemblyPath(assemblyPath);
            }

            return assembly;
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