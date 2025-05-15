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
using System.Reflection;

namespace Shaos.Services.Runtime.Validation
{
    public class PlugInTypeValidator : IPlugInTypeValidator
    {
        private const int AllowedConstructorCount = 1;
        private const int AllowedConstructorParameterCount = 2;

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

        public PlugInTypeInformation Validate(string assemblyFile)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(assemblyFile);

            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("Assembly file not found", assemblyFile);
            }

            UnloadingWeakReference<IRuntimeAssemblyLoadContext>? unloadingWeakReference = null;

            try
            {
                return ValidatePlugInAssembly(assemblyFile, out unloadingWeakReference);
            }
            finally
            {
                unloadingWeakReference.Dispose();
            }
        }

        internal void ValidatePlugInType(Type plugInType, out bool hasLogger, out bool hasConfiguration)
        {
            hasConfiguration = false;
            hasLogger = false;

            var constructors = plugInType.GetConstructors();

            if (constructors.Length != AllowedConstructorCount)
            {
                _logger.LogError("PlugIn [{Name}] contains invalid number of constructors [{Length}]",
                    plugInType.Name,
                    constructors.Length);

                throw new PlugInConstructorsException(
                    $"PlugIn [{plugInType.Name}] contains invalid number of constructors [{constructors.Length}]");
            }

            var parameterInfos = constructors[0].GetParameters();

            if (parameterInfos.Length > AllowedConstructorParameterCount)
            {
                _logger.LogError("PlugIn [{Name}] contains invalid number of constructor parameters [{Length}]",
                    plugInType.Name,
                    parameterInfos.Length);

                throw new PlugInConstructorException(
                    $"PlugIn [{plugInType.Name}] constructor contains invalid number of constructor parameters [{parameterInfos.Length}]");
            }

            var parameterTypes = (from parameterInfo in parameterInfos
                                  let parameterType = parameterInfo.ParameterType
                                  select parameterType)
                                 .ToList();

            var genericTypes = (from type in parameterTypes
                                where type.IsGenericType
                                let genericType = type.GetGenericTypeDefinition()
                                select genericType)
                                .ToList();

            if (genericTypes.Count == 0)
            {
                var constructorParameterList = String.Join(',', parameterTypes.Select(_ => _.Name));

                _logger.LogError("PlugIn [{Name}] contains an invalid constructor parameters [{List}]",
                    plugInType.Name,
                    constructorParameterList);

                throw new PlugInConstructorException($"PlugIn [{plugInType.Name}] contains an invalid constructor parameters [{constructorParameterList}]");
            }

            if (genericTypes.Count == 1 && !genericTypes.Contains(typeof(ILogger<>)))
            {
                var constructorParameterList = String.Join(',', parameterTypes.Select(_ => _.Name));

                _logger.LogError("PlugIn [{Name}] contains an invalid constructor parameters [{List}]",
                    plugInType.Name,
                    constructorParameterList);

                throw new PlugInConstructorException($"PlugIn [{plugInType.Name}] contains an invalid constructor parameters [{constructorParameterList}]");
            }

            var loggerType = parameterTypes.FirstOrDefault(_ => _.GetGenericTypeDefinition() == typeof(ILogger<>));

            if (loggerType != null)
            {
                var loggerGenericType = loggerType.GenericTypeArguments[0];

                if (loggerGenericType != plugInType)
                {
                    _logger.LogError("PlugIn [{Name}] [{Type}] parameter invalid generic type parameter [{Arg}]",
                        plugInType.Name,
                        nameof(ILogger),
                        loggerGenericType.Name);

                    throw new PlugInConstructorException(
                        $"PlugIn [{plugInType.Name}] [{nameof(ILogger)}] parameter invalid generic type parameter [{loggerGenericType.Name}]");
                }

                hasLogger = true;
            }

            var lastConstructorParameterType = parameterTypes.Except([loggerType]).FirstOrDefault();

            if (lastConstructorParameterType != null && !lastConstructorParameterType!.IsClass && lastConstructorParameterType.GetCustomAttributes<PlugInConfigurationClassAttribute>() != null)
            {
                throw new PlugInConstructorException($"PlugIn [{plugInType.Name}] contains an invalid constructor parameters [{lastConstructorParameterType}]");
            }

            if (lastConstructorParameterType != null)
            {
                hasConfiguration = true;
            }
        }

        private PlugInTypeInformation ValidatePlugInAssembly(string assemblyFile, out UnloadingWeakReference<IRuntimeAssemblyLoadContext> unloadingWeakReference)
        {
            var runtimeAssemblyLoadContext = _runtimeAssemblyLoadContextFactory.Create(assemblyFile);
            unloadingWeakReference = new UnloadingWeakReference<IRuntimeAssemblyLoadContext>(runtimeAssemblyLoadContext);

            try
            {
                var assembly = runtimeAssemblyLoadContext.LoadFromAssemblyPath(assemblyFile);

                var resolvedPlugIns = assembly.ResolveAssemblyDerivedTypes(typeof(IPlugIn));

                var count = resolvedPlugIns.Count();

                if (count == 0)
                {
                    _logger.LogError("No PlugIn type found in assembly [{Assembly}]", assembly.FullName);
                    throw new PlugInTypeNotFoundException();
                }

                if (count > 1)
                {
                    _logger.LogError("More than one PlugIn type found in assembly [{Assembly}]", assembly.FullName);
                    throw new PlugInTypesFoundException(count);
                }

                var plugInType = resolvedPlugIns.First();

                ValidatePlugInType(plugInType, out var hasLogger, out var hasConfiguration);

                return new PlugInTypeInformation(
                    plugInType.Name,
                    hasLogger,
                    hasConfiguration,
                    assembly.GetName().Version.ToString());
            }
            finally
            {
                runtimeAssemblyLoadContext.Unload();
            }
        }
    }
}