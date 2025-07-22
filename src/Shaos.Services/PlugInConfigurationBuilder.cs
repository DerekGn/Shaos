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
using Shaos.Services.IO;
using Shaos.Services.Json;
using Shaos.Services.Runtime;
using System.Reflection;

namespace Shaos.Services
{
    /// <summary>
    /// A PlugIn configuration builder
    /// </summary>
    public class PlugInConfigurationBuilder : BasePlugInBuilder, IPlugInConfigurationBuilder
    {
        private readonly IFileStoreService _fileStoreService;
        private readonly IRuntimeAssemblyLoadContextFactory _runtimeAssemblyLoadContextFactory;

        /// <summary>
        /// Create an instance of a PlugIn configuration builder
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="fileStoreService">The <see cref="IFileStoreService"/> instance</param>
        /// <param name="runtimeAssemblyLoadContextFactory">The <see cref="IRuntimeAssemblyLoadContextFactory"/> instance</param>
        public PlugInConfigurationBuilder(ILogger<PlugInConfigurationBuilder> logger,
                                          IFileStoreService fileStoreService,
                                          IRuntimeAssemblyLoadContextFactory runtimeAssemblyLoadContextFactory) : base(logger)
        {
            _fileStoreService = fileStoreService;
            _runtimeAssemblyLoadContextFactory = runtimeAssemblyLoadContextFactory;
        }

        /// <inheritdoc/>
        public object? CreateConfiguration(Assembly assembly)
        {
            return CreateConfigurationInternal(assembly);
        }

        /// <inheritdoc/>
        public object? LoadConfiguration(Assembly assembly,
                                         string? configuration)
        {
            object? result = null;
            object? configurationInstance = CreateConfigurationInternal(assembly);

            if (!string.IsNullOrEmpty(configuration))
            {
                result = Utf8JsonSerializer.Deserialize(configuration, configurationInstance!.GetType());
            }

            return result;
        }

        /// <inheritdoc/>
        public object? LoadConfiguration(int id, string assemblyFile, string? configuration = null)
        {
            var assemblyPath = _fileStoreService.GetAssemblyPath(id,
                                                                 assemblyFile);

            IRuntimeAssemblyLoadContext? context = null;
            object? configurationInstance = null;

            try
            {
                context = _runtimeAssemblyLoadContextFactory.Create(assemblyPath);

                var assembly = context.LoadFromAssemblyPath(assemblyPath);

                configurationInstance = LoadConfiguration(assembly, configuration);
            }
            finally
            {
                context?.Unload();
            }

            return configurationInstance!;
        }
    }
}