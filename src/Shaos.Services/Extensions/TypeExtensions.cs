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

using Shaos.Services.Exceptions;
using System.Globalization;
using System.Reflection;

namespace Shaos.Services.Extensions
{
    internal static class TypeExtensions
    {
        public static object? Parse(this Type type,
                                    string name,
                                    string value)
        {
            PropertyInfo? propertyInfo = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            object? result = null;

            if (propertyInfo != null)
            {
                result = propertyInfo.PropertyType switch
                {
                    Type boolType when boolType == typeof(bool) => bool.Parse(value),
                    Type byteType when byteType == typeof(byte) => byte.Parse(value),
                    Type sbyteType when sbyteType == typeof(sbyte) => sbyte.Parse(value),
                    Type charType when charType == typeof(char) => char.Parse(value),
                    Type decimalType when decimalType == typeof(decimal) => decimal.Parse(value),
                    Type doubleType when doubleType == typeof(double) => double.Parse(value),
                    Type floatType when floatType == typeof(float) => float.Parse(value),
                    Type intType when intType == typeof(int) => int.Parse(value),
                    Type uintType when uintType == typeof(uint) => uint.Parse(value),
                    Type longType when longType == typeof(long) => long.Parse(value),
                    Type ulongType when ulongType == typeof(ulong) => ulong.Parse(value),
                    Type shortType when shortType == typeof(short) => short.Parse(value),
                    Type ushortType when ushortType == typeof(ushort) => ushort.Parse(value),
                    Type stringType when stringType == typeof(string) => value,
                    Type timeSpanType when timeSpanType == typeof(TimeSpan) => TimeSpan.Parse(value, CultureInfo.InvariantCulture),
                    _ => throw new ConfigurationPropertyTypeNotMappedException(name, propertyInfo.PropertyType),
                };
            }
            else
            {
                throw new PropertyNotFoundException(name);
            }

            return result;
        }

        public static void SetProperty(this Type type,
                                       object target,
                                       string name,
                                       object? value)
        {
            PropertyInfo? propertyInfo = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);

            if(propertyInfo != null)
            {
                if(propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(target, value, null);
                }
                else
                {
                    throw new PropertyNotWriteableException(name);
                }
            }
            else
            {
                throw new PropertyNotFoundException(name);
            }
        }
    }
}
