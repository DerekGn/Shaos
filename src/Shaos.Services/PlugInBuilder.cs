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
using Shaos.Repository.Models;
using Shaos.Sdk;
using Shaos.Services.Exceptions;
using Shaos.Services.Extensions;
using Shaos.Services.Json;
using System.Reflection;

namespace Shaos.Services
{
    /// <summary>
    /// A <see cref="IPlugIn"/> builder
    /// </summary>
    /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> instance</param>
    /// <param name="logger">A <see cref="ILogger{TCategoryName}"/> instance</param>
    public partial class PlugInBuilder(ILoggerFactory loggerFactory,
                                       ILogger<PlugInBuilder> logger) : BasePlugInBuilder(logger), IPlugInBuilder
    {
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
            internal set => _plugin = value;
        }

        /// <inheritdoc/>
        public void Load(Assembly assembly,
                         string? configuration)
        {
            ArgumentNullException.ThrowIfNull(assembly);

            var plugInType = ResolvePlugInType(assembly);

            LogResolvedPlugIn(plugInType.Name,
                             assembly.FullName);

            var constructorParameters = GetConstructorParameters(plugInType);

            object? configurationInstance = LoadConfiguration(assembly, configuration);

            if (configurationInstance != null)
            {
                constructorParameters.Add(configurationInstance);
            }

            _plugin = Activator.CreateInstance(plugInType, constructorParameters.ToArray()) as IPlugIn;
        }

        /// <inheritdoc/>
        public void Restore(PlugInInstance plugInInstance)
        {
            ArgumentNullException.ThrowIfNull(plugInInstance);

            if (_plugin == null)
            {
                throw new PlugInInstanceNotLoadedException();
            }

            foreach (var device in plugInInstance.Devices)
            {
                _plugin.Devices.Add(device.ToSdk());
            }

            AssignPlugInIdentifier(plugInInstance.Id);
        }

        private void AssignPlugInIdentifier(int id)
        {
            var baseType = _plugin!.GetType().BaseType ?? throw new PlugInBuilderException("IPlugIn instance has no base type");
            var propertyInfo = baseType.GetProperty(nameof(IPlugIn.Id)) ?? throw new PlugInBuilderException("IPlugIn instance base type has no Id property setter");
            var setMethod = propertyInfo.GetSetMethod(true) ?? throw new PlugInBuilderException("IPlugIn instance Id property has no setter");

            setMethod.Invoke(_plugin, [id]);
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

                        LogCreatingPlugIn(parameterType.FullName);

                        result.Add(Activator.CreateInstance(loggerType, _loggerFactory)!);
                    }
                }
            }

            return result;
        }

        private object? LoadConfiguration(Assembly assembly,
                                          string? configuration)
        {
            object? configurationInstance = CreateConfigurationInternal(assembly);

            if (!configuration!.IsEmptyOrWhiteSpace())
            {
                configurationInstance = Utf8JsonSerializer.Deserialize(configuration!,
                                                                       configurationInstance!.GetType());
            }

            return configurationInstance;
        }

        [LoggerMessage(Level = LogLevel.Debug, Message = "Creating instance of [{fullName}]")]
        private partial void LogCreatingPlugIn(string? fullName);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Resolved PlugIn: [{name}] from Assembly: [{assembly}]")]
        private partial void LogResolvedPlugIn(string name,
                                               string? assembly);
    }
}