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
using Shaos.Sdk.Devices;
using Shaos.Services.Exceptions;
using Shaos.Services.Json;
using Shaos.Services.Runtime.Host;
using System.Reflection;

namespace Shaos.Services
{
    /// <summary>
    /// A <see cref="IPlugIn"/> builder
    /// </summary>
    /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> instance</param>
    /// <param name="logger">A <see cref="ILogger{TCategoryName}"/> instance</param>
    public class PlugInBuilder(ILoggerFactory loggerFactory,
                               ILogger<PlugInBuilder> logger) : BasePlugInBuilder(logger), IPlugInBuilder
    {
        private readonly ILogger<PlugInBuilder> _logger = logger;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;
        private IPlugIn? _plugin;

        /// <inheritdoc/>
        public IPlugIn? PlugIn
        {
            get
            {
                IPlugIn? plugin = _plugin;

                _plugin = null;

                return plugin;
            }
        }

        /// <inheritdoc/>
        public void Load(Assembly assembly,
                         InstanceConfiguration instanceConfiguration)
        {
            ArgumentNullException.ThrowIfNull(assembly);

            var plugInType = ResolvePlugInType(assembly);

            _logger.LogDebug("Resolved PlugIn: [{Name}] from Assembly: [{Assembly}]", plugInType.Name, assembly.FullName);

            var constructorParameters = GetConstructorParameters(plugInType);

            object? configuration = LoadConfiguration(assembly, instanceConfiguration);

            if (configuration != null)
            {
                constructorParameters.Add(configuration);
            }

            _plugin = Activator.CreateInstance(plugInType, constructorParameters.ToArray()) as IPlugIn;
        }

        /// <inheritdoc/>
        public void Restore(IEnumerable<Device> devices)
        {
            ArgumentNullException.ThrowIfNull(devices);

            if(_plugin == null)
            {
                throw new PlugInInstanceNotLoadedException();
            }

            foreach (var device in devices)
            {
                _plugin.Devices.Add(device);
            }
        }

        private List<object> GetConstructorParameters(Type plugInType)
        {
            var result = new List<object>();

            var constructorInfos = plugInType.GetConstructors();

            var parameterInfos = constructorInfos[0].GetParameters();

            var parameterTypes = (from parameterInfo in parameterInfos
                                  let parameterType = parameterInfo.ParameterType
                                  select parameterType)
                                  .ToList();

            foreach (var parameterType in parameterTypes)
            {
                if (parameterType.IsGenericType)
                {
                    var genericType = parameterType.GetGenericTypeDefinition();

                    if (genericType == typeof(ILogger<>))
                    {
                        Type[] typeArgs = { parameterType.GenericTypeArguments[0] };
                        Type loggerType = typeof(Logger<>).MakeGenericType(typeArgs);

                        _logger.LogDebug("Creating instance of [{Name}]", parameterType.FullName);

                        result.Add(Activator.CreateInstance(loggerType, _loggerFactory)!);
                    }
                }
            }

            return result;
        }

        private object? LoadConfiguration(Assembly assembly,
                                          InstanceConfiguration instanceConfiguration)
        {
            object? configuration = null;

            if (instanceConfiguration.RequiresConfiguration)
            {
                configuration = CreateConfigurationInternal(assembly);

                if (instanceConfiguration.IsConfigured)
                {
                    configuration = Utf8JsonSerializer.Deserialize(instanceConfiguration.Configuration!,
                                                                   configuration!.GetType());
                }
            }

            return configuration;
        }
    }
}