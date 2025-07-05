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

namespace Shaos.Sdk.Devices
{
    /// <summary>
    /// Represents a battery level for a <see cref="Device"/>
    /// </summary>
    public class BatteryLevel
    {
        private uint _level;

        /// <summary>
        /// The maximum value allowable for the <see cref="Level"/>
        /// </summary>
        public const uint Maximum = 100;

        /// <summary>
        /// The minimal value allowable for the <see cref="Level"/>
        /// </summary>
        public const uint Minimum = 0;

        /// <summary>
        /// The battery level value
        /// </summary>
        public uint Level
        {
            get => _level;
            set
            {
                _level = value;
            }
        }
    }
}
