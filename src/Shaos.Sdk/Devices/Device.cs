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
using Shaos.Sdk.Devices.Parameters;

namespace Shaos.Sdk.Devices
{
    /// <summary>
    /// Represents a <see cref="Device"/>
    /// </summary>
    public class Device
    {
        /// <summary>
        /// Create an instance of a <see cref="Device"/>
        /// </summary>
        /// <param name="name">The device name</param>
        /// <param name="parameters">The set of <see cref="BaseParameter"/> instances for this <see cref="Device"/></param>
        /// <param name="batteryLevel">The optional battery level if the <see cref="Device"/> instance is battery powered</param>
        /// <param name="signalLevel">The optional signal level if the <see cref="Device"/> is wireless</param>
        public Device(string name,
                      IList<BaseParameter> parameters,
                      uint? batteryLevel,
                      int? signalLevel)
        {
            Name = name;
            BatteryLevel = batteryLevel != null ? new BatteryLevel(this, (uint)batteryLevel) : null;
            SignalLevel = signalLevel != null ? new SignalLevel(this, (int)signalLevel) : null;

            foreach (var parameter in parameters)
            {
                Parameters.Add(parameter);
            }
        }

        /// <summary>
        /// Raised when the <see cref="Device"/> changes
        /// </summary>
        public event EventHandler<DeviceChangedEventArgs>? DeviceChanged;

        /// <summary>
        /// The <see cref="BatteryLevel"/> for this device
        /// </summary>
        /// <remarks>
        /// A <see cref="Device"/> optional battery level
        /// </remarks>
        public BatteryLevel? BatteryLevel { get; }

        /// <summary>
        /// The device identifier
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// The device name
        /// </summary>
        /// <remarks>
        /// An optional <see cref="Device"/> name
        /// </remarks>
        public string Name { get; }

        /// <summary>
        /// The set of <see cref="Device"/> <see cref="BaseParameter"/>
        /// </summary>
        public ObservableList<BaseParameter> Parameters { get; } = [];

        /// <summary>
        /// The <see cref="SignalLevel"/> for this device
        /// </summary>
        /// <remarks>
        /// A <see cref="Device"/> optional signal level
        /// </remarks>
        public SignalLevel? SignalLevel { get; }

        /// <summary>
        /// Raise the value changed event to subscribed listeners
        /// </summary>
        /// <param name="e">The <see cref="ParameterValueChangedEventArgs{T}"/></param>
        protected virtual void OnDeviceChanged(DeviceChangedEventArgs e)
        {
            DeviceChanged?.Invoke(this, e);
        }

        internal void BatteryLevelChanged(uint level)
        {
            OnDeviceChanged(new DeviceChangedEventArgs() { BatteryLevel = level });
        }

        internal void SignalLevelChanged(int level)
        {
            OnDeviceChanged(new DeviceChangedEventArgs() { SignalLevel = level });
        }
    }
}