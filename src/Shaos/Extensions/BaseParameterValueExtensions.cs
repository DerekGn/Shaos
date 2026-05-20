using Shaos.Pages.Parameters.Types;
using Shaos.Repository.Models.Devices.Parameters;

namespace Shaos.Extensions
{
    internal static class BaseParameterValueExtensions
    {
        public static BoolValue ToModel(this BoolParameterValue parameter)
        {
            return new BoolValue()
            {
                TimeStamp = parameter.TimeStamp,
                Value = parameter.Value
            };
        }

        public static IntValue ToModel(this IntParameterValue parameter)
        {
            return new IntValue()
            {
                TimeStamp = parameter.TimeStamp,
                Value = parameter.Value
            };
        }

        public static FloatValue ToModel(this FloatParameterValue parameter)
        {
            return new FloatValue()
            {
                TimeStamp = parameter.TimeStamp,
                Value = parameter.Value
            };
        }

        public static StringValue ToModel(this StringParameterValue parameter)
        {
            return new StringValue()
            {
                TimeStamp = parameter.TimeStamp,
                Value = parameter.Value
            };
        }

        public static UIntValue ToModel(this UIntParameterValue parameter)
        {
            return new UIntValue()
            {
                TimeStamp = parameter.TimeStamp,
                Value = parameter.Value
            };
        }

        public static BaseValue ToModel(this BaseParameterValue parameter) => parameter switch
        {
            BoolParameterValue boolParameter => boolParameter.ToModel(),
            FloatParameterValue floatParameter => floatParameter.ToModel(),
            IntParameterValue intParameter => intParameter.ToModel(),
            StringParameterValue stringParameter => stringParameter.ToModel(),
            UIntParameterValue uintParameter => uintParameter.ToModel(),
            _ => throw new NotImplementedException()
        };
    }
}