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

namespace Shaos.Sdk.Devices.Parameters
{
    /// <summary>
    /// Represents a <see cref="string"/> parameter
    /// </summary>
    public class StringParameter : BaseParameter
    {
        private string _value;

        /// <summary>
        /// Create an instance of a <see cref="StringParameter"/>
        /// </summary>
        /// <param name="id">The identifier of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="name">The name of the parameter</param>
        /// <param name="units">The units of this parameter</param>
        /// <param name="parameterType">The <see cref="ParameterType"/> of this parameter</param>
        public StringParameter(int id,
                               string value,
                               string? name,
                               string? units,
                               ParameterType? parameterType) : base(id, name, units, parameterType)
        {
            _value = value;
        }

        /// <summary>
        /// Raised when the value of the parameter changes
        /// </summary>
        public event EventHandler<ParameterValueChangedEventArgs<string>>? ValueChanged;

        /// <summary>
        /// The current <see cref="StringParameter"/> value
        /// </summary>
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnValueChanged(new ParameterValueChangedEventArgs<string>() { Value = _value });
            }
        }

        /// <summary>
        /// Raise the value changed event to subscribed listeners
        /// </summary>
        /// <param name="e">The <see cref="ParameterValueChangedEventArgs{T}"/></param>
        protected virtual void OnValueChanged(ParameterValueChangedEventArgs<string> e)
        {
            ValueChanged?.Invoke(this, e);
        }
    }
}