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

using Shaos.Sdk.Devices;
using Shaos.Sdk.Devices.Parameters;
using Shaos.Services.Exceptions;
using Shaos.Services.Runtime.Host;

namespace Shaos.Services.Parameters
{
    /// <summary>
    /// The <see cref="IParameterService"/> instance
    /// </summary>
    public class ParameterService : IParameterService
    {
        private readonly IRuntimeInstanceHost _runtimeInstanceHost;

        /// <summary>
        /// Create an instance of a <see cref="IParameterService"/>
        /// </summary>
        /// <param name="runtimeInstanceHost">The <see cref="IRuntimeInstanceHost"/> instance</param>
        public ParameterService(IRuntimeInstanceHost runtimeInstanceHost)
        {
            _runtimeInstanceHost = runtimeInstanceHost;
        }

        /// <inheritdoc/>
        public void WriteParameter(int parameterId,
                                   bool value)
        {
            ExecuteParameterUpdate<IBaseParameter<bool>>(parameterId, (parameter) =>
            {
                parameter.WriteAsync(value);
            });
        }

        /// <inheritdoc/>
        public void WriteParameter(int parameterId,
                                   float value)
        {
            ExecuteParameterUpdate<IBaseParameter<float>>(parameterId, (parameter) =>
            {
                parameter.WriteAsync(value);
            });
        }

        /// <inheritdoc/>
        public void WriteParameter(int parameterId,
                                   uint value)
        {
            ExecuteParameterUpdate<IBaseParameter<uint>>(parameterId, (parameter) =>
            {
                parameter.WriteAsync(value);
            });
        }

        /// <inheritdoc/>
        public void WriteParameter(int parameterId,
                                   int value)
        {
            ExecuteParameterUpdate<IBaseParameter<int>>(parameterId, (parameter) =>
            {
                parameter.WriteAsync(value);
            });
        }

        private void ExecuteParameterUpdate<T>(int parameterId,
                                               Action<T> parameterUpdate) where T : class, IBaseParameter
        {
            var parameter = _runtimeInstanceHost
                .Instances
                .Where(_ => _.ExecutionContext != null)
                .Where(_ => _.ExecutionContext!.PlugIn != null)
                .Select(_ => _.ExecutionContext!.PlugIn)
                .Select(_ => _!.Devices)
                .Cast<IDevice>()
                .Select(_ => _.Parameters)
                .Cast<IBaseParameter>()
                .FirstOrDefault(_ => _.Id == parameterId) ?? throw new ParameterNotFoundException(parameterId);

            if(!parameter.CanWrite)
            {
                throw new ParameterNotWritableException(parameterId);
            }

            if (parameter is not T)
            {
                throw new ParameterInvalidTypeException(parameterId,
                                                        typeof(T),
                                                        parameter.GetType());
            }

            parameterUpdate(parameter as T);
        }
    }
}