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

using Shaos.Services.Json;
using Shaos.Testing.Shared;
using Xunit;

namespace Shaos.Services.UnitTests.Json
{
    public class Utf8JsonSerializerTests : BaseTests
    {
        private const string TestGuid = "89b3c108-081d-4a47-95eb-38a4ab648cf6";

        public Utf8JsonSerializerTests()
        {
        }

        [Fact]
        public void TestDeserialise()
        {
            var json =
                "{\"Bool\":true,\"Byte\":1,\"Char\":\"Z\"," +
                "\"DateTime\":\"2020-12-20T12:10:59.0000000Z\"," +
                "\"Decimal\":2.1,\"Double\":3.2,\"Float\":4.3," +
                "\"Guid\":\"89b3c108-081d-4a47-95eb-38a4ab648cf6\"," +
                "\"Int\":5,\"Long\":6,\"SByte\":7,\"Short\":8,\"String\":\"String\"," +
                "\"TimeSpan\":\"10675199.02:48:05.4775807\",\"UInt\":10,\"ULong\":11,\"UShort\":12}";

            var result = Utf8JsonSerializer.Deserialize(json, typeof(TestConfiguration));

            Assert.NotNull(result);

            TestConfiguration configuration = (TestConfiguration)result;
            Assert.True(configuration.Bool);
            Assert.Equal(1, configuration.Byte);
            Assert.Equal('Z', configuration.Char);
            Assert.Equal(new DateTime(2020,12,20,12,10,59, DateTimeKind.Utc), configuration.DateTime);
            Assert.Equal(2.1m, configuration.Decimal);
            Assert.Equal(3.2, configuration.Double, 2);
            Assert.Equal(4.3, configuration.Float, 2);
            Assert.Equal(new Guid(TestGuid), configuration.Guid);
            Assert.Equal(5, configuration.Int);
            Assert.Equal(6, configuration.Long);
            Assert.Equal(7, configuration.SByte);
            Assert.Equal(8, configuration.Short);
            Assert.Equal("String", configuration.String);
            Assert.Equal(TimeSpan.MaxValue, configuration.TimeSpan);
            Assert.Equal((uint)10, configuration.UInt);
            Assert.Equal((ulong)11, configuration.ULong);
            Assert.Equal(12, configuration.UShort);
        }

        [Fact]
        public void TestSerialise()
        {
            var testConfiguration = new TestConfiguration()
            {
                Bool = true,
                Byte = 1,
                Char = 'Z',
                DateTime = new DateTime(2020,12,12,12,12,12,DateTimeKind.Utc),
                Decimal = 2.1m,
                Double = 3.2,
                Enum = TestValues.Value,
                Float = 4.3f,
                Guid = new Guid(TestGuid),
                Int = 5,
                Long = 6,
                SByte = 7,
                Short = 8,
                String = "String",
                TimeSpan = TimeSpan.MaxValue,
                UInt = 9,
                ULong = 10,
                UShort = 11
            };

            var result = Utf8JsonSerializer.Serialize(testConfiguration);

            Assert.NotEmpty(result);
            Assert.Equal(
                "{\"Bool\":true,\"Byte\":1,\"Char\":\"Z\"," +
                "\"DateTime\":\"2020-12-12T12:12:12Z\"," +
                "\"Decimal\":2.1,\"Double\":3.2,\"Enum\":1,\"Float\":4.3," +
                "\"Guid\":\"89b3c108-081d-4a47-95eb-38a4ab648cf6\"," +
                "\"Int\":5,\"Long\":6,\"SByte\":7,\"Short\":8,\"String\":\"String\"," +
                "\"TimeSpan\":\"10675199.02:48:05.4775807\",\"UInt\":9,\"ULong\":10,\"UShort\":11}", result);
        }
    }
}
