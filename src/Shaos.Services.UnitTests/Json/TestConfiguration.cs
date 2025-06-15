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

namespace Shaos.Services.UnitTests.Json
{
    internal class TestConfiguration
    {
        public bool Bool { get; set; }
        public byte Byte { get; set; }
        public char Char { get; set; }
        public DateTime DateTime { get; set; }
        public decimal Decimal { get; set; }
        public double Double { get; set; }
        public TestValues Enum { get; set; }
        public float Float { get; set; }
        public Guid Guid { get; set; }
        public int Int { get; set; }
        public long Long { get; set; }
        public sbyte SByte { get; set; }
        public short Short { get; set; }
        public string? String { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public uint UInt { get; set; }
        public ulong ULong { get; set; }
        public ushort UShort { get; set; }
    }
}