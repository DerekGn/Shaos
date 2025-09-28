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

using Shaos.Repository.Models;
using Shaos.Sdk.Devices;
using Shaos.Sdk.Devices.Parameters;

namespace Shaos.Services.Runtime.Host
{
    /// <summary>
    /// Handles <see cref="Device"/> updates
    /// </summary>
    public interface IRuntimeDeviceUpdateHandler
    {
        /// <summary>
        /// Create a <see cref="Device"/> <see cref="BaseParameter"/> instance.
        /// </summary>
        /// <param name="id">The <see cref="Device"/> instance identifier.</param>
        /// <param name="parameters">The <see cref="Device"/> <see cref="BaseParameter"/> instances.</param>
        Task CreateDeviceParametersAsync(int id, IEnumerable<IBaseParameter> parameters);

        /// <summary>
        /// Create a list of <see cref="Device"/> instances.
        /// </summary>
        /// <param name="id">The <see cref="PlugInInstance"/> identifier.</param>
        /// <param name="devices">The set of <see cref="Device"/> to create.</param>
        /// <returns></returns>
        Task CreateDevicesAsync(int id, IEnumerable<IDevice> devices);

        /// <summary>
        /// Delete a set of <see cref="IBaseParameter"/> instances.
        /// </summary>
        /// <param name="parameterIds">The set of <see cref="IBaseParameter"/> instances identifiers to delete the <see cref="BaseParameter"/> instances.</param>
        Task DeleteDeviceParametersAsync(IEnumerable<int> parameterIds);

        /// <summary>
        /// Delete a set of <see cref="IDevice"/> instances
        /// </summary>
        /// <param name="deviceIds">The set of <see cref="IDevice"/> instances identifiers to delete the <see cref="Device"/> instances.</param>
        Task DeleteDevicesAsync(IEnumerable<int> deviceIds);

        /// <summary>
        /// Update a <see cref="IDevice"/> instance battery level.
        /// </summary>
        /// <param name="id">The <see cref="IDevice"/> instance identifier.</param>
        /// <param name="level">The battery level</param>
        /// <param name="timestamp">The timestamp of the event</param>
        Task DeviceBatteryLevelUpdateAsync(int id, uint level, DateTime timestamp);

        /// <summary>
        /// Update a <see cref="IDevice"/> instance signal level.
        /// </summary>
        /// <param name="id">The <see cref="IDevice"/> instance identifier.</param>
        /// <param name="level">The signal level</param>
        /// <param name="timestamp">The timestamp of the event</param>
        Task DeviceSignalLevelUpdateAsync(int id, int level, DateTime timestamp);

        /// <summary>
        /// Save a <see cref="BaseParameter"/>
        /// </summary>
        /// <param name="id">The parameter id</param>
        /// <param name="value">The updated value</param>
        /// <param name="timeStamp">The timestamp</param>
        Task SaveParameterChangeAsync(int id, int value, DateTime timeStamp);

        /// <summary>
        /// Save a <see cref="BaseParameter"/>
        /// </summary>
        /// <param name="id">The parameter id</param>
        /// <param name="value">The updated value</param>
        /// <param name="timeStamp">The timestamp</param>
        Task SaveParameterChangeAsync(int id, string value, DateTime timeStamp);

        /// <summary>
        /// Save a <see cref="BaseParameter"/>
        /// </summary>
        /// <param name="id">The parameter id</param>
        /// <param name="value">The updated value</param>
        /// <param name="timeStamp">The timestamp</param>
        Task SaveParameterChangeAsync(int id, float value, DateTime timeStamp);

        /// <summary>
        /// Save a <see cref="BaseParameter"/>
        /// </summary>
        /// <param name="id">The parameter id</param>
        /// <param name="value">The updated value</param>
        /// <param name="timeStamp">The timestamp</param>
        Task SaveParameterChangeAsync(int id, bool value, DateTime timeStamp);

        /// <summary>
        /// Save a <see cref="BaseParameter"/>
        /// </summary>
        /// <param name="id">The parameter id</param>
        /// <param name="value">The updated value</param>
        /// <param name="timeStamp">The timestamp</param>
        Task SaveParameterChangeAsync(int id, uint value, DateTime timeStamp);
    }
}