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
using Shaos.Services.Runtime.Exceptions;
using Shaos.Services.Runtime.Extensions;

namespace Shaos.Services.Runtime.Validation
{
    public class PlugInTypeValidator : IPlugInTypeValidator
    {
        private readonly ILogger<PlugInTypeValidator> _logger;
        private readonly IRuntimeAssemblyLoadContextFactory _runtimeAssemblyLoadContextFactory;

        public PlugInTypeValidator(
            ILogger<PlugInTypeValidator> logger,
            IRuntimeAssemblyLoadContextFactory runtimeAssemblyLoadContextFactory)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(runtimeAssemblyLoadContextFactory);

            _logger = logger;
            _runtimeAssemblyLoadContextFactory = runtimeAssemblyLoadContextFactory;
        }

        public void Validate(string assemblyFile, out string version)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(assemblyFile);

            if(!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("Assembly file not found", assemblyFile);
            }

            ValidateAssembly(assemblyFile, out version);
        }

        private void ValidateAssembly(string assemblyFile, out string version)
        {
            var runtimeAssemblyLoadContext = _runtimeAssemblyLoadContextFactory.Create(assemblyFile);
            var unloadingWeakReference = new UnloadingWeakReference<IRuntimeAssemblyLoadContext>(runtimeAssemblyLoadContext);

            try
            {
                var plugInAssembly = runtimeAssemblyLoadContext.LoadFromAssemblyPath(assemblyFile);

                var resolvedPlugIns = plugInAssembly.ResolveAssemblyDerivedTypes(typeof(IPlugIn));

                var count = resolvedPlugIns.Count();

                if (count == 0)
                {
                    throw new PlugInTypeNotFoundException();
                }

                if (count > 1)
                {
                    throw new PlugInTypesFoundException(count);
                }

                var plugInType = resolvedPlugIns.First();

                var plugInConstructors = plugInType.GetConstructors();

                if(plugInConstructors.Length != 1)
                {
                    throw new PlugInInvalidConstructorsException();
                }

                var plugInConstructor = plugInConstructors[0];
                var plugInConstructorParameters = plugInConstructor.GetParameters();

                version = plugInAssembly.GetName().Version!.ToString();
            }
            finally
            {
                runtimeAssemblyLoadContext.Unload();

                unloadingWeakReference.Dispose();
            }
        }
    }
}