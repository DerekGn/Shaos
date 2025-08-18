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

        public static IList<SdkIBaseParameter> ToSdk(this IList<ModelBaseParameter> parameters)
        {
            List<SdkIBaseParameter>? result = null;

            if (parameters != null)
            {
                result = [];
                foreach (var parameter in parameters)
                {
                    result.Add(Convert(parameter));
                }
            }

            return result!;
        }

        private static SdkIBaseParameter Convert(ModelBaseParameter parameter)
        {
            var type = parameter.GetType();
            SdkIBaseParameter? result = null;

            switch (type)
            {
                case Type _ when type == typeof(BoolParameter):
                    result = new Sdk.Devices.Parameters.BoolParameter(((BoolParameter)parameter).Value,
                                                                      parameter.Name,
                                                                      parameter.Units,
                                                                      parameter.ParameterType);
                    break;

                case Type _ when type == typeof(FloatParameter):
                    result = new Sdk.Devices.Parameters.FloatParameter(((FloatParameter)parameter).Value,
                                                                       parameter.Name,
                                                                       parameter.Units,
                                                                       parameter.ParameterType);
                    break;

                case Type _ when type == typeof(IntParameter):
                    result = new Sdk.Devices.Parameters.IntParameter(((IntParameter)parameter).Value,
                                                                     parameter.Name,
                                                                     parameter.Units,
                                                                     parameter.ParameterType);
                    break;

                case Type _ when type == typeof(StringParameter):
                    result = new Sdk.Devices.Parameters.StringParameter(((StringParameter)parameter).Value,
                                                                        parameter.Name,
                                                                        parameter.Units,
                                                                        parameter.ParameterType);
                    break;

                case Type _ when type == typeof(UIntParameter):
                    result = new Sdk.Devices.Parameters.UIntParameter(((UIntParameter)parameter).Value,
                                                                      parameter.Name,
                                                                      parameter.Units,
                                                                      parameter.ParameterType);
                    break;
            }

            result?.SetId(parameter.Id);

            return result!;
        }

        private static ModelBaseParameter Convert(SdkIBaseParameter parameter)
        {
            var type = parameter.GetType();
            ModelBaseParameter? result = null;

            switch (type)
            {
                case Type _ when type == typeof(Sdk.Devices.Parameters.BoolParameter):
                    result = new BoolParameter()
                    {
                        Id = parameter.Id,
                        Name = parameter.Name,
                        ParameterType = parameter.ParameterType,
                        Units = parameter.Units,
                        Value = ((Sdk.Devices.Parameters.BoolParameter)parameter).Value
                    };
                    break;

                case Type _ when type == typeof(Sdk.Devices.Parameters.FloatParameter):
                    result = new FloatParameter()
                    {
                        Id = parameter.Id,
                        Name = parameter.Name,
                        ParameterType = parameter.ParameterType,
                        Units = parameter.Units,
                        Value = ((Sdk.Devices.Parameters.FloatParameter)parameter).Value
                    };
                    break;

                case Type _ when type == typeof(Sdk.Devices.Parameters.IntParameter):
                    result = new IntParameter()
                    {
                        Id = parameter.Id,
                        Name = parameter.Name,
                        ParameterType = parameter.ParameterType,
                        Units = parameter.Units,
                        Value = ((Sdk.Devices.Parameters.IntParameter)parameter).Value
                    };
                    break;

                case Type _ when type == typeof(Sdk.Devices.Parameters.StringParameter):
                    result = new StringParameter()
                    {
                        Id = parameter.Id,
                        Name = parameter.Name,
                        ParameterType = parameter.ParameterType,
                        Units = parameter.Units,
                        Value = ((Sdk.Devices.Parameters.StringParameter)parameter).Value
                    };
                    break;

                case Type _ when type == typeof(Sdk.Devices.Parameters.UIntParameter):
                    result = new UIntParameter()
                    {
                        Id = parameter.Id,
                        Name = parameter.Name,
                        ParameterType = parameter.ParameterType,
                        Units = parameter.Units,
                        Value = ((Sdk.Devices.Parameters.UIntParameter)parameter).Value
                    };
                    break;
            }

            return result!;
        }
    }
}