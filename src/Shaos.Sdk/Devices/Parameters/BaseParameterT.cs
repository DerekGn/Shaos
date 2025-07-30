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

using Shaos.Sdk.Collections.Generic;

namespace Shaos.Sdk.Devices.Parameters
{
    /// <summary>
    /// A device <see cref="BaseParameter{T}"/>
    /// </summary>
    public abstract class BaseParameter<T> : BaseParameter
    {
        private T _value;

        /// <summary>
        /// Create an instance of a <see cref="BaseParameter{T}"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="name">The name of the parameter</param>
        /// <param name="units">The units of this parameter</param>
        /// <param name="parameterType">The <see cref="ParameterType"/> of this parameter</param>
        protected BaseParameter(T value,
                                string? name,
                                string? units,
                                ParameterType? parameterType) : base(name, units, parameterType)
        {
            _value = value;
        }

        /// <summary>
        /// Raised when the value of the parameter changes
        /// </summary>
        public event AsyncEventHandler<ParameterValueChangedEventArgs<T>>? ValueChanged;

        /// <summary>
        /// The <see cref="BaseParameter{T}"/> current value
        /// </summary>
        public T Value
        {
            get
            {
                return _value;
            }
        }

        /// <summary>
        /// Update the <see cref="ObservableList{T}"/>
        /// </summary>
        /// <param name="value">The value to assign to <see cref="Value"/></param>
        public async Task WriteValueAsync(T value)
        {
            _value = value;

            await OnValueChangedAsync(new ParameterValueChangedEventArgs<T>() { Value = value });
        }

        /// <summary>
        /// Raise the <see cref="ValueChanged"/> event
        /// </summary>
        /// <param name="eventArgs">The <see cref="ListChangedEventArgs{T}"/></param>
        protected virtual async Task OnValueChangedAsync(ParameterValueChangedEventArgs<T> eventArgs)
        {
            if (ValueChanged != null)
            {
                await ValueChanged.Invoke(this, eventArgs);
            }
        }
    }
}