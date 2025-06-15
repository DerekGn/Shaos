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

using Moq;
using Shaos.Sdk;
using Shaos.Services.Runtime.Host;
using Xunit;

namespace Shaos.Services.UnitTests.Runtime.Host
{
    public class InstanceExecutionContextTests
    {
        private readonly InstanceExecutionContext _instanceExecutionContext;
        private readonly Mock<IPlugIn> _mockPlugIn;
        private bool _executed;
        private bool _completed;

        public InstanceExecutionContextTests()
        {
            _mockPlugIn = new Mock<IPlugIn>();

            _instanceExecutionContext = new InstanceExecutionContext(_mockPlugIn.Object);
        }

        [Fact]
        public void TestStartExecution()
        {
            _instanceExecutionContext.StartExecution(
                async (cancellationToken) => await ExecuteMethod(),
                (antecedent) => CompletionMethod());

            Assert.False(_executed);
            Assert.False(_completed);
        }

        private void CompletionMethod()
        {
            _completed = true;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task ExecuteMethod()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            _executed = true;
        }
    }
}