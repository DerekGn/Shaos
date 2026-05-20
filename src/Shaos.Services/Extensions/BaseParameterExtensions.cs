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
using Shaos.Sdk.Collections.Generic;
using ModelBaseParameter = Shaos.Repository.Models.Devices.Parameters.BaseParameter;
using SdkIBaseParameter = Shaos.Sdk.Devices.Parameters.IBaseParameter;

namespace Shaos.Services.Extensions
{
    internal static class ParameterExtensions
    {
        public static IList<ModelBaseParameter> ToModel(this IObservableList<SdkIBaseParameter> parameters)
        {
            List<ModelBaseParameter>? result = null;

            if (parameters != null)
            {
                result = [];

                foreach (var parameter in parameters)
                {
                    result.Add(parameter.ToModel()!);
                }
            }

            return result!;
        }

        public static ModelBaseParameter? ToModel(this SdkIBaseParameter parameter)
        {
            ModelBaseParameter? result = null;

            if (parameter != null)
            {
                result = Convert(parameter);
            }

            return result!;
        }

        private static ModelBaseParameter Convert(SdkIBaseParameter parameter)
        {
            ModelBaseParameter? result = null;

            switch (parameter)
            {
                case BoolParameter boolParameter:
                    result = new BoolParameter()
                    {
                        InstanceId = parameter.Id,
                        Name = parameter.Name,
                        Value = boolParameter.Value
                    };
                    break;

                case FloatParameter floatParameter:
                    result = new FloatParameter()
                    {
                        InstanceId = parameter.Id,
                        Max = floatParameter.Max,
                        Min = floatParameter.Min,
                        Name = parameter.Name,
                        Value = floatParameter.Value
                    };
                    break;

                case IntParameter intParameter:
                    result = new IntParameter()
                    {
                        Max = intParameter.Max,
                        Min = intParameter.Min,
                        Name = parameter.Name,
                        Value = intParameter.Value
                    };
                    break;

                case StringParameter stringParameter:
                    result = new StringParameter()
                    {
                        Name = parameter.Name,
                        Value = stringParameter.Value
                    };
                    break;

                case UIntParameter uIntParameter:
                    result = new UIntParameter()
                    {
                        Max = uIntParameter.Max,
                        Min = uIntParameter.Min,
                        Name = parameter.Name,
                        Value = uIntParameter.Value
                    };
                    break;
            }

            if (result is not null)
            {
                result.InstanceId = parameter.Id;
                result.CanWrite = parameter.CanWrite;
                result.ParameterType = parameter.ParameterType;
                result.Units = parameter.Units;
            }

            return result!;
        }
    }
}