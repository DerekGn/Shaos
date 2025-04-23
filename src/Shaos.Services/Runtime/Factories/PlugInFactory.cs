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

namespace Shaos.Services.Runtime.Factories
{
    public class PlugInFactory : IPlugInFactory
    {
        private readonly ILogger<PlugInFactory> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public PlugInFactory(
            ILoggerFactory loggerFactory,
            ILogger<PlugInFactory> logger)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IPlugIn? CreateInstance(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);

            var plugInType = ResolvePlugInType(assembly);

            _logger.LogDebug("Resolved PlugIn: [{Name}] from Assembly: [{Assembly}]", plugInType.Name, assembly.FullName);

            var constructorParameters = GetConstructorParameters(plugInType);

            return Activator.CreateInstance(plugInType, constructorParameters) as IPlugIn;
        }

        private List<object> GetConstructorParameters(Type plugInType)
        {
            var result = new List<object>();

            var constructors = plugInType.GetConstructors();

            var parameters = constructors[0].GetParameters();


            foreach (var parameterType in from parameter in parameters
                                          let parameterType = parameter.ParameterType
                                          select parameterType)
            {
                var interfaces = parameterType.GetInterfaces();

                if(interfaces[0] == typeof(ILogger))
                {
                    Type[] typeArgs = { plugInType };
                    Type genericLoggerType = typeof(Logger<>).MakeGenericType(typeArgs);

                    result.Add(Activator.CreateInstance(genericLoggerType, _loggerFactory)!);
                }
            }

            return result;
        }

        private Type ResolvePlugInType(Assembly assembly)
        {
            var result = from Type type in assembly.GetTypes()
                         where typeof(IPlugIn).IsAssignableFrom(type)
                         select type;

            if (result == null || !result.Any())
            {
                _logger.LogWarning("Unable to resolve a [{Type}] derived type from Assembly: [{Name}]", typeof(IPlugIn), assembly.FullName);

                throw new InvalidOperationException($"Unable to resolve a [{typeof(IPlugIn)}] derived type from Assembly: [{assembly.FullName}]");
            }

            return result.FirstOrDefault()!;
        }

        //private IPlugIn? ActivateType(Type plugInType)
        //{
        //    if(plugInType.GetConstructors().Length > 1)
        //    {
        //        throw new InvalidOperationException($"Resolved PlugIn: [{plugInType.Name}] contains multiple constructors");
        //    }

        //    if (plugInType.GetConstructors().Length == 0)
        //    {
        //        throw new InvalidOperationException($"Resolved PlugIn: [{plugInType.Name}] contains no constructor");
        //    }

        //    var constructor = plugInType.GetConstructors()[0];

        //    var parameters = constructor.GetParameters();
        //}
    }
}