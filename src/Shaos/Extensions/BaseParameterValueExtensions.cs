using Shaos.Pages.Parameters.Types;
using Shaos.Repository.Models.Devices.Parameters;

namespace Shaos.Extensions
{
    internal static class BaseParameterValueExtensions
    {
        public static string ToCsv(this BaseParameterValue parameter) => parameter switch
        {
            BoolParameterValue boolParameter => boolParameter.ToCsv(),
            FloatParameterValue floatParameter => floatParameter.ToCsv(),
            IntParameterValue intParameter => intParameter.ToCsv(),
            StringParameterValue stringParameter => stringParameter.ToCsv(),
            UIntParameterValue uintParameter => uintParameter.ToCsv(),
            _ => throw new NotImplementedException()
        };

        public static string ToCsv(this BoolParameterValue parameter)
        {
            return $"{parameter.Value},{parameter.TimeStamp.ToUniversalTime}{Environment.NewLine}";
        }

        public static string ToCsv(this FloatParameterValue parameter)
        {
            return $"{parameter.Value},{parameter.TimeStamp.ToUniversalTime}{Environment.NewLine}";
        }

        public static string ToCsv(this IntParameterValue parameter)
        {
            return $"{parameter.Value},{parameter.TimeStamp.ToUniversalTime}{Environment.NewLine}";
        }

        public static string ToCsv(this StringParameterValue parameter)
        {
            return $"{parameter.Value},{parameter.TimeStamp.ToUniversalTime}{Environment.NewLine}";
        }

        public static string ToCsv(this UIntParameterValue parameter)
        {
            return $"{parameter.Value},{parameter.TimeStamp.ToUniversalTime}{Environment.NewLine}";
        }

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