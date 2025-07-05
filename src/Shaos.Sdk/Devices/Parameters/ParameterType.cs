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
    /// Represents the <see cref="BaseParameter"/> type
    /// </summary>
    public enum ParameterType
    {
        /// <summary>
        /// The absolute active energy
        /// </summary>
        AbsoluteActiveEnergy,
        /// <summary>
        /// The absolute reactive energy
        /// </summary>
        AbsoluteReactiveEnergy,
        /// <summary>
        /// The active power of the phase
        /// </summary>
        ActivePower,
        /// <summary>
        /// The apparent power of a phase
        /// </summary>
        ApparentPower,
        /// <summary>
        /// The current of a phase
        /// </summary>
        Current,
        /// <summary>
        /// The estimated carbon dioxide level
        /// </summary>
        eCo2,
        /// <summary>
        /// The equivalent ethanol level
        /// </summary>
        EtOH,
        /// <summary>
        /// Flow rate
        /// </summary>
        FlowRate,
        /// <summary>
        /// The forward active energy
        /// </summary>
        ForwardActiveEnergy,
        /// <summary>
        /// The forward reactive energy
        /// </summary>
        ForwardReactiveEnergy,
        /// <summary>
        /// Frequency
        /// </summary>
        Frequency,
        /// <summary>
        /// The Indoor air quality
        /// </summary>
        Iaq,
        /// <summary>
        /// The level
        /// </summary>
        Level,
        /// <summary>
        /// The phase angle of a phase
        /// </summary>
        PhaseAngle,
        /// <summary>
        /// The power factor of a phase
        /// </summary>
        PowerFactor,
        /// <summary>
        /// The pressure
        /// </summary>
        Pressure,
        /// <summary>
        /// Reactive power
        /// </summary>
        ReactivePower,
        /// <summary>
        /// Real power
        /// </summary>
        RealPower,
        /// <summary>
        /// Relative humidity
        /// </summary>
        RelativeHumidity,
        /// <summary>
        /// The reverse active power of a phase
        /// </summary>
        ReverseActiveEnergy,
        /// <summary>
        /// The reverse reactive power of a phase
        /// </summary>
        ReverseReactiveEnergy,
        /// <summary>
        /// The temperature
        /// </summary>
        Temperature,
        /// <summary>
        /// The total volatile organic compounds
        /// </summary>
        TVOC,
        /// <summary>
        /// The voltage
        /// </summary>
        Voltage,
        /// <summary>
        /// A voltage
        /// </summary>
        Volume,
        /// <summary>
        /// Rotational speed
        /// </summary>
        RotationSpeed
    }
}