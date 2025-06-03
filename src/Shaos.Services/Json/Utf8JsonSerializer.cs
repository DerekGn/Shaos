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
using System.Text;
using System.Text.Json;

namespace Shaos.Services.Json
{
    internal class Utf8JsonSerializer
    {
        public static object Deserialize(string json, Type type)
        {
            var instance = Activator.CreateInstance(type);

            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

            var properties = type.GetProperties();

            PropertyInfo? propertyInfo = null;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.None:
                        break;

                    case JsonTokenType.StartObject:
                        break;

                    case JsonTokenType.EndObject:
                        break;

                    case JsonTokenType.StartArray:
                        break;

                    case JsonTokenType.EndArray:
                        break;

                    case JsonTokenType.PropertyName:
                        var name = reader.GetString();
                        propertyInfo = properties.First(_ => _.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                        break;

                    case JsonTokenType.Comment:
                        break;

                    case JsonTokenType.String:
                        propertyInfo.SetValue(instance, ReadString(reader, propertyInfo));
                        break;

                    case JsonTokenType.Number:
                        propertyInfo!.SetValue(instance, ReadNumber(reader, propertyInfo));
                        break;

                    case JsonTokenType.True:
                        propertyInfo!.SetValue(instance, reader.GetBoolean());
                        break;

                    case JsonTokenType.Null:
                        break;
                }
            }

            return instance;
        }

        public static string Serialize(object value)
        {
            ArgumentNullException.ThrowIfNull(value);

            using MemoryStream stream = new();
            using Utf8JsonWriter writer = new(stream);

            writer.WriteStartObject();

            foreach (var propertyInfo in value.GetType().GetProperties())
            {
                WriteValue(writer, propertyInfo, value);
            }

            writer.WriteEndObject();
            writer.Flush();

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private static object? ReadNumber(Utf8JsonReader reader, PropertyInfo propertyInfo)
        {
            object? result = null;

            switch (propertyInfo.PropertyType)
            {
                case Type byteType when byteType == typeof(byte):
                    result = reader.GetByte();
                    break;

                case Type decimalType when decimalType == typeof(decimal):
                    result = reader.GetDecimal();
                    break;

                case Type decimalType when decimalType == typeof(double):
                    result = reader.GetDouble();
                    break;

                case Type shortType when shortType == typeof(short):
                    result = reader.GetInt16();
                    break;

                case Type intType when intType == typeof(int):
                    result = reader.GetInt32();
                    break;

                case Type longType when longType == typeof(long):
                    result = reader.GetInt64();
                    break;

                case Type sbyteType when sbyteType == typeof(sbyte):
                    result = reader.GetSByte();
                    break;

                case Type floatType when floatType == typeof(float):
                    result = reader.GetSingle();
                    break;

                case Type ushortType when ushortType == typeof(ushort):
                    result = reader.GetUInt16();
                    break;

                case Type uintType when uintType == typeof(uint):
                    result = reader.GetUInt32();
                    break;

                case Type ulongType when ulongType == typeof(ulong):
                    result = reader.GetUInt64();
                    break;

                default:
                    throw new ConfigurationPropertyTypeNotMappedException(propertyInfo.Name, propertyInfo.PropertyType);
            }

            return result;
        }

        private static object? ReadString(Utf8JsonReader reader,
                                          PropertyInfo propertyInfo)
        {
            object? result = null;

            switch (propertyInfo.PropertyType)
            {
                case Type charType when charType == typeof(char):
                    result = reader.GetString()?.FirstOrDefault();
                    break;

                case Type dateTimeType when dateTimeType == typeof(DateTime):
                    result = reader.GetDateTime();
                    break;

                case Type guidType when guidType == typeof(Guid):
                    result = reader.GetGuid();
                    break;

                case Type stringType when stringType == typeof(string):
                    result = reader.GetString();
                    break;

                case Type timeSpanType when timeSpanType == typeof(TimeSpan):
                    result = TimeSpan.Parse(reader.GetString()!, CultureInfo.InvariantCulture);
                    break;

                default:
                    throw new ConfigurationPropertyTypeNotMappedException(propertyInfo.Name, propertyInfo.PropertyType);
            }

            return result;
        }

        private static void WriteValue(Utf8JsonWriter writer,
                                       PropertyInfo propertyInfo,
                                       object value)
        {
            switch (propertyInfo.PropertyType)
            {
                case Type boolType when boolType == typeof(bool):
                    writer.WriteBoolean(propertyInfo.Name, (bool)propertyInfo.GetValue(value)!);
                    break;

                case Type byteType when byteType == typeof(byte):
                    writer.WriteNumber(propertyInfo.Name, (byte)propertyInfo.GetValue(value)!);
                    break;

                case Type sbyteType when sbyteType == typeof(sbyte):
                    writer.WriteNumber(propertyInfo.Name, (sbyte)propertyInfo.GetValue(value)!);
                    break;

                case Type charType when charType == typeof(char):
                    writer.WriteString(propertyInfo.Name, ((char)propertyInfo.GetValue(value)!).ToString());
                    break;

                case Type decimalType when decimalType == typeof(decimal):
                    writer.WriteNumber(propertyInfo.Name, (decimal)propertyInfo.GetValue(value)!);
                    break;

                case Type doubleType when doubleType == typeof(double):
                    writer.WriteNumber(propertyInfo.Name, (double)propertyInfo.GetValue(value)!);
                    break;

                case Type enumType when enumType.BaseType == typeof(Enum):
                    writer.WriteNumber(propertyInfo.Name, (int)propertyInfo.GetValue(value)!);
                    break;

                case Type floatType when floatType == typeof(float):
                    writer.WriteNumber(propertyInfo.Name, (float)propertyInfo.GetValue(value)!);
                    break;

                case Type intType when intType == typeof(int):
                    writer.WriteNumber(propertyInfo.Name, (int)propertyInfo.GetValue(value)!);
                    break;

                case Type uintType when uintType == typeof(uint):
                    writer.WriteNumber(propertyInfo.Name, (uint)propertyInfo.GetValue(value)!);
                    break;

                case Type longType when longType == typeof(long):
                    writer.WriteNumber(propertyInfo.Name, (long)propertyInfo.GetValue(value)!);
                    break;

                case Type ulongType when ulongType == typeof(ulong):
                    writer.WriteNumber(propertyInfo.Name, (ulong)propertyInfo.GetValue(value)!);
                    break;

                case Type shortType when shortType == typeof(short):
                    writer.WriteNumber(propertyInfo.Name, (short)propertyInfo.GetValue(value)!);
                    break;

                case Type ushortType when ushortType == typeof(ushort):
                    writer.WriteNumber(propertyInfo.Name, (ushort)propertyInfo.GetValue(value)!);
                    break;

                case Type stringType when stringType == typeof(string):
                    writer.WriteString(propertyInfo.Name, (string)propertyInfo.GetValue(value)!);
                    break;

                case Type timeSpanType when timeSpanType == typeof(TimeSpan):
                    writer.WriteString(propertyInfo.Name, ((TimeSpan)propertyInfo.GetValue(value)!).ToString());
                    break;

                case Type timeSpanType when timeSpanType == typeof(DateTime):
                    writer.WriteString(propertyInfo.Name, (DateTime)propertyInfo.GetValue(value)!);
                    break;

                case Type guidType when guidType == typeof(Guid):
                    writer.WriteString(propertyInfo.Name, (Guid)propertyInfo.GetValue(value)!);
                    break;

                default:
                    throw new ConfigurationPropertyTypeNotMappedException(propertyInfo.Name, propertyInfo.PropertyType);
            }
        }
    }
}