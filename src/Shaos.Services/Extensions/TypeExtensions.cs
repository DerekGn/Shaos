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
    public static class TypeExtensions
    {
        public static object? Parse(this Type type,
                                    string name,
                                    string value)
        {
            PropertyInfo? propertyInfo = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            object? result = null;

            if (propertyInfo != null)
            {
                switch (propertyInfo.PropertyType)
                {
                    case Type boolType when boolType == typeof(bool):
                        result = bool.Parse(value);
                        break;
                    case Type byteType when byteType == typeof(byte):
                        result = byte.Parse(value);
                        break;
                    case Type sbyteType when sbyteType == typeof(sbyte):
                        result = sbyte.Parse(value);
                        break;
                    case Type charType when charType == typeof(char):
                        result = char.Parse(value);
                        break;
                    case Type decimalType when decimalType == typeof(decimal):
                        result = decimal.Parse(value);
                        break;
                    case Type doubleType when doubleType == typeof(double):
                        result = double.Parse(value);
                        break;
                    case Type floatType when floatType == typeof(float):
                        result = float.Parse(value);
                        break;
                    case Type intType when intType == typeof(int):
                        result = int.Parse(value);
                        break;
                    case Type uintType when uintType == typeof(uint):
                        result = uint.Parse(value);
                        break;
                    case Type longType when longType == typeof(long):
                        result = long.Parse(value);
                        break;
                    case Type ulongType when ulongType == typeof(ulong):
                        result = ulong.Parse(value);
                        break;
                    case Type shortType when shortType == typeof(short):
                        result = short.Parse(value);
                        break;
                    case Type ushortType when ushortType == typeof(ushort):
                        result = ushort.Parse(value);
                        break;
                    case Type stringType when stringType == typeof(string):
                        result = value;
                        break;
                    case Type timeSpanType when timeSpanType == typeof(TimeSpan):
                        result = TimeSpan.Parse(value, CultureInfo.InvariantCulture);
                        break;
                    default:
                        throw new ConfigurationPropertyTypeNotMappedException(name, propertyInfo.PropertyType);
                }
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
                                       object value)
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
