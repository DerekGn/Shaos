using Shaos.Pages.Parameters.Types;
using Shaos.Repository.Models.Devices.Parameters;

namespace Shaos.Extensions
{
    internal static class BaseParameterValueExtensions
    {
        public static BoolHistoryValue ToModel(this BoolParameterValue parameter)
        {
            return new BoolHistoryValue()
            {
                TimeStamp = parameter.TimeStamp,
                Value = parameter.Value
            };
        }

        public static IntHistoryValue ToModel(this IntParameterValue parameter)
        {
            return new IntHistoryValue()
            {
                TimeStamp = parameter.TimeStamp,
                Value = parameter.Value
            };
        }

        public static FloatHistoryValue ToModel(this FloatParameterValue parameter)
        {
            return new FloatHistoryValue()
            {
                TimeStamp = parameter.TimeStamp,
                Value = parameter.Value
            };
        }

        public static StringHistoryValue ToModel(this StringParameterValue parameter)
        {
            return new StringHistoryValue()
            {
                TimeStamp = parameter.TimeStamp,
                Value = parameter.Value
            };
        }

        public static UIntHistoryValue ToModel(this UIntParameterValue parameter)
        {
            return new UIntHistoryValue()
            {
                TimeStamp = parameter.TimeStamp,
                Value = parameter.Value
            };
        }

        public static BaseHistoryValue ToModel(this BaseParameterValue parameter) => parameter switch
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