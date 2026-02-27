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

using System.ComponentModel.DataAnnotations;

namespace Shaos.Repository.Models.Devices.Parameters
{
    /// <summary>
    /// Represents an integer parameter
    /// </summary>
    public class IntParameter : BaseParameter
    {
        /// <summary>
        /// The maximum value for this <see cref="IntParameter"/>
        /// </summary>
        [Required]
        public int Max { get; set; }

        /// <summary>
        /// The minimum value for this <see cref="IntParameter"/>
        /// </summary>
        [Required]
        public int Min { get; set; }

        /// <summary>
        /// The current value
        /// </summary>
        [Required]
        public int Value { get; set; }

        /// <summary>
        /// The set of <see cref="IntParameterValue"/> previous values
        /// </summary>
        public List<IntParameterValue> Values { get; set; } = [];

        /// <summary>
        /// Update the value and add a new value entry
        /// </summary>
        /// <param name="value">The updated value</param>
        /// <param name="timestamp">The timestamp</param>
        public void UpdateValue(int value, DateTime timestamp)
        {
            Value = value;

            Values.Add(new IntParameterValue()
            {
                Parameter = this,
                ParameterId = Id,
                TimeStamp = timestamp,
                Value = value
            });
        }
    }
}