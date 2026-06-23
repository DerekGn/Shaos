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

using ModelBaseParameter = Shaos.Repository.Models.Devices.Parameters.BaseParameter;
using ModelBoolParameter = Shaos.Repository.Models.Devices.Parameters.BoolParameter;
using ModelFloatParameter = Shaos.Repository.Models.Devices.Parameters.FloatParameter;
using ModelIntParameter = Shaos.Repository.Models.Devices.Parameters.IntParameter;
using ModelStringParameter = Shaos.Repository.Models.Devices.Parameters.StringParameter;
using ModelUIntParameter = Shaos.Repository.Models.Devices.Parameters.UIntParameter;

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

        public static ModelBaseParameter? ToModel(this IBaseParameter parameter)
        {
            ModelBaseParameter? result = null;

            if (parameter != null)
            {
                result = Convert(parameter);
            }

            return result!;
        }

        public static IList<IBaseParameter> ToSdk(this IList<ModelBaseParameter> parameters)
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
            IBaseParameter? result = null;

            switch (parameter)
            {
                case ModelBoolParameter boolParameter:
                    result = new BoolParameter(boolParameter.Value,
                                               parameter.Name,
                                               parameter.Units,
                                               parameter.ReferenceId,
                                               parameter.ParameterType);
                    break;
                case ModelFloatParameter floatParameter:
                    result = new FloatParameter(floatParameter.Value,
                                                floatParameter.Min,
                                                floatParameter.Max,
                                                floatParameter.Step,
                                                parameter.Name,
                                                parameter.Units,
                                                parameter.ReferenceId,
                                                parameter.ParameterType);
                    break;
                case ModelIntParameter intParameter:
                    result = new IntParameter(intParameter.Value,
                                              intParameter.Min,
                                              intParameter.Max,
                                              intParameter.Step,
                                              parameter.Name,
                                              parameter.Units,
                                              parameter.ReferenceId,
                                              parameter.ParameterType);
                    break;
                case ModelStringParameter stringParameter:
                    result = new StringParameter(stringParameter.Value,
                                                 parameter.Name,
                                                 parameter.Units,
                                                 parameter.ReferenceId,
                                                 parameter.ParameterType);
                    break;
                case ModelUIntParameter uintParameter:
                    result = new UIntParameter(uintParameter.Value,
                                               uintParameter.Min,
                                               uintParameter.Max,
                                               uintParameter.Step,
                                               parameter.Name,
                                               parameter.Units,
                                               parameter.ReferenceId,
                                               parameter.ParameterType);
                    break;
            }

            result?.AssignId(parameter.Id);

            return result!;
        }

        private static ModelBaseParameter Convert(SdkIBaseParameter parameter)
        {
            ModelBaseParameter? result = null;

            switch (parameter)
            {
                case BoolParameter boolParameter:
                    result = new ModelBoolParameter()
                    {
                        Name = parameter.Name,
                        Value = boolParameter.Value
                    };
                    break;

                case FloatParameter floatParameter:
                    result = new ModelFloatParameter()
                    {
                        Max = floatParameter.Max,
                        Min = floatParameter.Min,
                        Name = parameter.Name,
                        Value = floatParameter.Value
                    };
                    break;

                case IntParameter intParameter:
                    result = new ModelIntParameter()
                    {
                        Max = intParameter.Max,
                        Min = intParameter.Min,
                        Name = parameter.Name,
                        Value = intParameter.Value
                    };
                    break;

                case StringParameter stringParameter:
                    result = new ModelStringParameter()
                    {
                        Name = parameter.Name,
                        Value = stringParameter.Value
                    };
                    break;

                case UIntParameter uIntParameter:
                    result = new ModelUIntParameter()
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
                result.CanWrite = parameter.CanWrite;
                result.ParameterType = parameter.ParameterType;
                result.Units = parameter.Units;
            }

            return result!;
        }
    }
}