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
using System.Xml.Linq;

namespace Shaos.Services.Runtime
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

            Type[] typeArgs = { plugInType };
            Type genericLoggerType = typeof(Logger<>).MakeGenericType(typeArgs);

            object? logger = Activator.CreateInstance(genericLoggerType, _loggerFactory);

            return Activator.CreateInstance(plugInType, logger) as IPlugIn;
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
    }
}