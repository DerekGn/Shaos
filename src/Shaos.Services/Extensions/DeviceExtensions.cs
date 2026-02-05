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

using ModelDevice = Shaos.Repository.Models.Devices.Device;
using SdkDevice = Shaos.Sdk.Devices.Device;

namespace Shaos.Services.Extensions
{
    internal static class DeviceExtensions
    {
        public static ModelDevice ToModel(this IDevice device)
        {
            var modelDevice = new ModelDevice()
            {
                Id = device.Id,
                Name = device.Name,
                Features = device.Features,
                BatteryLevel = device.BatteryLevel?.Level,
                SignalLevel = device.SignalLevel?.Level,
            };

            modelDevice.Parameters.AddRange(device.Parameters.ToModel());

            return modelDevice;
        }

        //public static SdkDevice ToSdk(this ModelDevice device)
        //{
        //    var sdkDevice = new Device(device.Name,
        //                               device.Features,
        //                               device.Parameters.ToSdk());

        //    sdkDevice.SetId(device.Id);

        //    return sdkDevice;
        //}
    }
}