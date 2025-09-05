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

using Shaos.Repository.Models.Devices.Parameters;

namespace Shaos.Repository.Models.Devices
{
    /// <summary>
    /// A device model
    /// </summary>
    public class Device : BaseEntity
    {
        /// <summary>
        /// The <see cref="Device"/> last battery level
        /// </summary>
        public uint? BatteryLevel { get; set; }

        /// <summary>
        /// The set of <see cref="DeviceBatteryUpdate"/>
        /// </summary>
        public List<DeviceBatteryUpdate> BatteryUpdates { get; set; } = [];

        /// <summary>
        /// The <see cref="Device"/> instance name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The set of <see cref="BaseParameter"/>
        /// </summary>
        public List<BaseParameter> Parameters { get; } = [];

        /// <summary>
        /// The parent <see cref="PlugInInstance"/>
        /// </summary>
        public PlugInInstance? PlugInInstance { get; set; } = null;

        /// <summary>
        /// The <see cref="PlugInInstance"/> identifier
        /// </summary>
        public int? PlugInInstanceId { get; set; } = null;

        /// <summary>
        /// The <see cref="Device"/> last signal level
        /// </summary>
        public int? SignalLevel { get; set; }

        /// <summary>
        /// The set of <see cref="DeviceSignalUpdate"/>
        /// </summary>
        public List<DeviceSignalUpdate> SignalUpdates { get; set; } = [];

        /// <summary>
        /// Update the device battery level
        /// </summary>
        /// <param name="batteryLevel">The updated device battery level</param>
        /// <param name="timeStamp">The timestamp of the update</param>
        public void UpdateBatteryLevel(uint batteryLevel,
                                       DateTime timeStamp)
        {
            BatteryLevel = batteryLevel;

            BatteryUpdates.Add(new DeviceBatteryUpdate()
            {
                BatteryLevel = batteryLevel,
                Device = this,
                DeviceId = this.Id,
                TimeStamp = timeStamp
            });
        }

        /// <summary>
        /// Update the device signal level
        /// </summary>
        /// <param name="signalLevel">The updated device signal level</param>
        /// <param name="timeStamp">The timestamp of the update</param>
        public void UpdateSignalLevel(int signalLevel,
                                      DateTime timeStamp)
        {
            SignalLevel = signalLevel;

            SignalUpdates.Add(new DeviceSignalUpdate()
            {
                Device = this,
                DeviceId = this.Id,
                SignalLevel = signalLevel,
                TimeStamp = timeStamp
            });
        }
    }
}