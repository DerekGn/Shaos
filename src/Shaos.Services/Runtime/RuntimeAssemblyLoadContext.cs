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
    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("'{Name}' ({_plugInAssemblyPath})")]
    public class RuntimeAssemblyLoadContext : AssemblyLoadContext, IRuntimeAssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _assemblyDependencyResolver;
        private readonly AssemblyLoadContext _assemblyLoadContext;
        private readonly string _plugInAssemblyPath;

        public RuntimeAssemblyLoadContext(
           string plugInAssemblyPath) : this(GetAssemblyLoadContext(), plugInAssemblyPath, true)
        {
        }

        public RuntimeAssemblyLoadContext(
           string plugInAssemblyPath,
           bool isCollectable) : this(GetAssemblyLoadContext(), plugInAssemblyPath, isCollectable)
        {
        }

        public RuntimeAssemblyLoadContext(
            AssemblyLoadContext assemblyLoadContext,
            string plugInAssemblyPath,
            bool isCollectable) : base(Path.GetFileNameWithoutExtension(plugInAssemblyPath), isCollectable)
        {
            ArgumentNullException.ThrowIfNull(assemblyLoadContext);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(plugInAssemblyPath);

            _assemblyLoadContext = assemblyLoadContext;
            _plugInAssemblyPath = plugInAssemblyPath;
            _assemblyDependencyResolver = new AssemblyDependencyResolver(plugInAssemblyPath);
        }

#warning TODO load plugin folder local assemblies. Include executing runtime assemblies look up

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            ArgumentNullException.ThrowIfNull(assemblyName);

            return _assemblyLoadContext.LoadFromAssemblyName(assemblyName);
        }

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