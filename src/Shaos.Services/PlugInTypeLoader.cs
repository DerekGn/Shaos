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
using Shaos.Sdk;
using Shaos.Services.Exceptions;
using Shaos.Services.Runtime;
using Shaos.Services.Runtime.Extensions;
using System.Reflection;

namespace Shaos.Services
{
    public class PlugInTypeLoader : IPlugInTypeLoader
    {
        private readonly ILogger<PlugInTypeLoader> _logger;
        private readonly IRuntimeAssemblyLoadContextFactory _runtimeAssemblyLoadContextFactory;

        public PlugInTypeLoader(
            ILogger<PlugInTypeLoader> logger,
            IRuntimeAssemblyLoadContextFactory runtimeAssemblyLoadContextFactory)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(runtimeAssemblyLoadContextFactory);

            _logger = logger;
            _runtimeAssemblyLoadContextFactory = runtimeAssemblyLoadContextFactory;
        }

        public void Validate(string assemblyFile, out string version)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(assemblyFile);

            if(!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("Assembly file not found", assemblyFile);
            }

            ValidateAssemblyContainsPlugIn(assemblyFile, out version, out var count).Dispose();

            if(count == 0)
            {
                throw new PlugInTypeNotFoundException();
            }

            if(count > 1)
            {
                throw new PlugInTypesFoundException(count);
            }
        }

        private UnloadingWeakReference<IRuntimeAssemblyLoadContext> ValidateAssemblyContainsPlugIn(
            string assemblyFile,
            out string version,
            out int count)
        {
            var runtimeAssemblyLoadContext = _runtimeAssemblyLoadContextFactory.Create(assemblyFile);
            var unloadingWeakReference = new UnloadingWeakReference<IRuntimeAssemblyLoadContext>(runtimeAssemblyLoadContext);

            var plugInAssembly = runtimeAssemblyLoadContext.LoadFromAssemblyPath(assemblyFile);

            version = plugInAssembly.GetName().Version!.ToString();

            var resolvedPlugIns = plugInAssembly.ResolveAssemblyDerivedTypes(typeof(IPlugIn));

            count = resolvedPlugIns.Count();

            runtimeAssemblyLoadContext.Unload();

            _logger.LogDebug("Assembly file: [{Assembly}] contains [{Count}] PlugIns",
                assemblyFile,
                count);

            return unloadingWeakReference;
        }
    }
}