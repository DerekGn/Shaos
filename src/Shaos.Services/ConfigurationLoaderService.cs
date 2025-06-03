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

using Shaos.Services.IO;
using Shaos.Services.Json;
using Shaos.Services.Runtime;
using Shaos.Services.Runtime.Factories;

namespace Shaos.Services
{
    /// <summary>
    /// The configuration loader service
    /// </summary>
    public class ConfigurationLoaderService : IConfigurationLoaderService
    {
        private readonly IPlugInFactory _plugInFactory;
        private readonly IFileStoreService _fileStoreService;
        private readonly IRuntimeAssemblyLoadContextFactory _runtimeAssemblyLoadContextFactory;

        /// <summary>
        /// Create an instance of a configuration loader service
        /// </summary>
        /// <param name="plugInFactory">The <see cref="IPlugInFactory"/> instance</param>
        /// <param name="fileStoreService">The <see cref="IFileStoreService"/> instance</param>
        /// <param name="runtimeAssemblyLoadContextFactory">The <see cref="IRuntimeAssemblyLoadContextFactory"/> instance</param>
        public ConfigurationLoaderService(IPlugInFactory plugInFactory,
                                          IFileStoreService fileStoreService,
                                          IRuntimeAssemblyLoadContextFactory runtimeAssemblyLoadContextFactory)
        {

            _plugInFactory = plugInFactory;
            _fileStoreService = fileStoreService;
            _runtimeAssemblyLoadContextFactory = runtimeAssemblyLoadContextFactory;
        }

        /// <inheritdoc/>
        public object? LoadConfiguration(int id,
                                         string assemblyFile,
                                         string? configuration = default)
        {
            var assemblyPath = _fileStoreService.GetAssemblyPath(id,
                                                                 assemblyFile);

            IRuntimeAssemblyLoadContext? context = null;
            object? configurationInstance = null;

            try
            {
                context = _runtimeAssemblyLoadContextFactory.Create(assemblyPath);
                var assembly = context.LoadFromAssemblyPath(assemblyPath);

                configurationInstance = _plugInFactory.CreateConfiguration(assembly);

                if (!string.IsNullOrEmpty(configuration))
                {
                    configurationInstance = Utf8JsonSerializer.Deserialize(configuration, configurationInstance!.GetType());
                }
            }
            finally
            {
                context?.Unload();
            }

            return configurationInstance!;
        }
    }
}
