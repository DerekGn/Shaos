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

namespace Shaos.Services
{
    /// <summary>
    /// The base PlugIn builder class
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> instance</param>
    public abstract class BasePlugInBuilder(ILogger<BasePlugInBuilder> logger)
    {
        protected ILogger<BasePlugInBuilder> Logger = logger;

        /// <summary>
        /// Create a PlugIn configuration instance
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to load the configuration from</param>
        /// <returns>The configuration instance</returns>
        protected object? CreateConfigurationInternal(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);

            var plugInType = ResolvePlugInType(assembly);

            var constructors = plugInType.GetConstructors();
            var parameterInfos = constructors[0].GetParameters();
            var parameterTypes = (from parameterInfo in parameterInfos
                                  let parameterType = parameterInfo.ParameterType
                                  select parameterType)
                                  .ToList();

            var configurationType = (from parameterType in parameterTypes
                                     where parameterType.GetCustomAttributes<PlugInConfigurationClassAttribute>().Any()
                                     select parameterType)
                                          .FirstOrDefault();

            object? configuration = null;

            if (configurationType != null)
            {
                configuration = Activator.CreateInstance(configurationType);
            }

            return configuration;
        }

        /// <summary>
        /// Resolve a <see cref="IPlugIn"/> type from an <see cref="Assembly"/>
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to load the <see cref="IPlugIn"/> derived type from</param>
        /// <returns>The resolved <see cref="IPlugIn"/> derived type</returns>
        /// <exception cref="InvalidOperationException">Thrown if an <see cref="IPlugIn"/> derived type cannot be resolved</exception>
        protected Type ResolvePlugInType(Assembly assembly)
        {
            var result = from Type type in assembly.GetTypes()
                         where typeof(IPlugIn).IsAssignableFrom(type)
                         select type;

            if (result == null || !result.Any())
            {
                Logger.LogWarning("Unable to resolve a [{Type}] derived type from Assembly: [{Name}]", typeof(IPlugIn), assembly.FullName);

                throw new InvalidOperationException($"Unable to resolve a [{typeof(IPlugIn)}] derived type from Assembly: [{assembly.FullName}]");
            }

            return result.FirstOrDefault()!;
        }
    }
}