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
using System.Reflection;

namespace Shaos.Services.Runtime
{
    public class PlugInFactory : IPlugInFactory
    {
        private readonly ILogger<PlugInFactory> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public PlugInFactory(
            ILogger<PlugInFactory> logger,
            ILoggerFactory loggerFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public IPlugIn? CreateInstance(Assembly assembly, IRuntimeAssemblyLoadContext assemblyLoadContext)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            ArgumentNullException.ThrowIfNull(assemblyLoadContext);

            var plugInType = ResolvePlugInType(assembly);
            var loggerType = typeof(Logger<>);
            var loggerFactoryType = typeof(LoggerFactory);

            var loggerAssembly = ResolveAssemblyForType(loggerType, assemblyLoadContext);
            var loggerFactoryAssembly = ResolveAssemblyForType(loggerFactoryType, assemblyLoadContext);

            var resolvedLoggerType = ResolveTypeFromAssembly(loggerType, loggerAssembly);
            var resolvedLoggerFactoryType = ResolveTypeFromAssembly(loggerFactoryType, loggerFactoryAssembly);

            Type[] typeArgs = { plugInType };
            Type genericLoggerType = resolvedLoggerType.MakeGenericType(typeArgs);

            object? loggerFactory = Activator.CreateInstance(resolvedLoggerFactoryType);
            object? logger = Activator.CreateInstance(genericLoggerType, loggerFactory) ?? throw new InvalidOperationException($"Unable to create instance of type [{genericLoggerType}]");

            IPlugIn? result = null;

            result = Activator.CreateInstance(plugInType, logger) as IPlugIn;

            return result;
        }

        private void DumpTypeConstructor(Type type)
        {
            foreach (var ctr in type.GetConstructors())
            {
                foreach (var parameterInfo in ctr.GetParameters())
                {
                    _logger.LogDebug(parameterInfo.ToString());
                }
            }
        }

        private static Assembly ResolveAssemblyForType(
            Type type,
            IRuntimeAssemblyLoadContext assemblyLoadContext)
        {
            ArgumentNullException.ThrowIfNull(type);

            return assemblyLoadContext
                .LoadFromAssemblyName(type.Assembly.GetName()) ??
                throw new InvalidOperationException($"Unable to resolve type for [{type}]");
        }

        private static Type ResolvePlugInType(Assembly assembly)
        {
            var result = from Type type in assembly.GetTypes()
                         where typeof(IPlugIn).IsAssignableFrom(type)
                         select type;

            if (result == null || !result.Any())
            {
                throw new InvalidOperationException($"Unable to resolve [{typeof(IPlugIn)}]");
            }

            return result.FirstOrDefault()!;
        }

        private static Type ResolveTypeFromAssembly(Type typeToResolve, Assembly assembly)
        {
            var result = from Type type in assembly.GetTypes()
                         where type.FullName == typeToResolve.FullName
                         select type;

            if (result == null || !result.Any())
            {
                throw new InvalidOperationException($"Unable to resolve [{typeToResolve}]");
            }

            return result.FirstOrDefault()!;
        }
    }
}