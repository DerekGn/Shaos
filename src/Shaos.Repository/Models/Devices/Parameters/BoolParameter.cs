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

namespace Shaos.Repository.Models.Devices.Parameters
{
    /// <summary>
    /// Represents a boolean parameter
    /// </summary>
    public class BoolParameter : BaseParameter
    {
        /// <summary>
        /// The last boolean value
        /// </summary>
        public bool Value { get; set; }

        /// <summary>
        /// The set of <see cref="BoolParameterValue"/> previous values
        /// </summary>
        public List<BoolParameterValue> Values { get; set; } = [];

        /// <summary>
        /// Update the value and add a new value entry
        /// </summary>
        /// <param name="value">The updated value</param>
        /// <param name="timestamp">The timestamp</param>
        public void UpdateValue(bool value,
                                DateTime timestamp)
        {
            Value = value;

            Values.Add(new BoolParameterValue()
            {
                Parameter = this,
                ParameterId = Id,
                TimeStamp = timestamp,
                Value = value
            });
        }
    }
}