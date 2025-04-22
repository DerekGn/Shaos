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

using Microsoft.Extensions.Logging;
using Moq;
using Shaos.Services.Runtime;
using Shaos.Services.Runtime.Exceptions;
using Shaos.Services.Runtime.Validation;
using Shaos.Services.UnitTests.Fixtures;
using Shaos.Test.PlugIn;
using Shaos.Testing.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests.Runtime.Validation
{
    public class PlugInTypeValidatorTests : BaseTests, IClassFixture<PlugInTestFixture>
    {
        private readonly PlugInTestFixture _fixture;
        private readonly Mock<IRuntimeAssemblyLoadContext> _mockRuntimeAssemblyLoadContext;
        private readonly Mock<IRuntimeAssemblyLoadContextFactory> _mockRuntimeAssemblyLoadContextFactory;
        private readonly PlugInTypeValidator _pluginLoader;

        public PlugInTypeValidatorTests(
            ITestOutputHelper output,
            PlugInTestFixture fixture) : base(output)
        {
            _fixture = fixture;

            _mockRuntimeAssemblyLoadContextFactory = new Mock<IRuntimeAssemblyLoadContextFactory>();

            _mockRuntimeAssemblyLoadContext = new Mock<IRuntimeAssemblyLoadContext>();

            _pluginLoader = new PlugInTypeValidator(
                LoggerFactory!.CreateLogger<PlugInTypeValidator>(),
                _mockRuntimeAssemblyLoadContextFactory.Object);
        }

        [Fact]
        public void TestValidateFileNotFound()
        {
            _mockRuntimeAssemblyLoadContextFactory.Setup(_ => _.Create(_fixture.AssemblyFilePath))
                .Returns(_mockRuntimeAssemblyLoadContext.Object);

            _mockRuntimeAssemblyLoadContext.Setup(_ => _.LoadFromAssemblyPath(
                It.IsAny<string>()))
                .Returns(typeof(TestPlugIn).Assembly);

            Assert.Throws<FileNotFoundException>(() => _pluginLoader.Validate("z:\non-exists", out var version));
        }

        [Fact]
        public void TestValidatePlugInTypeNotFound()
        {
            _mockRuntimeAssemblyLoadContextFactory.Setup(_ => _.Create(It.IsAny<string>()))
                .Returns(_mockRuntimeAssemblyLoadContext.Object);

            _mockRuntimeAssemblyLoadContext.Setup(_ => _.LoadFromAssemblyPath(
                It.IsAny<string>()))
                .Returns(typeof(object).Assembly);

            Assert.Throws<PlugInTypeNotFoundException>(() => _pluginLoader.Validate(_fixture.AssemblyFilePath, out var version));
        }

        [Fact]
        public void TestValidatePlugInTypesFound()
        {
            _mockRuntimeAssemblyLoadContextFactory.Setup(_ => _.Create(It.IsAny<string>()))
                .Returns(_mockRuntimeAssemblyLoadContext.Object);

            _mockRuntimeAssemblyLoadContext.Setup(_ => _.LoadFromAssemblyPath(
                It.IsAny<string>()))
                .Returns(typeof(Test.PlugIn.Invalid.TestPlugIn).Assembly);

            Assert.Throws<PlugInTypesFoundException>(() => _pluginLoader.Validate(_fixture.AssemblyFilePathInvalid, out var version));
        }

        [Fact]
        public void TestValidateSuccess()
        {
            _mockRuntimeAssemblyLoadContextFactory.Setup(_ => _.Create(_fixture.AssemblyFilePath))
                .Returns(_mockRuntimeAssemblyLoadContext.Object);

            _mockRuntimeAssemblyLoadContext.Setup(_ => _.LoadFromAssemblyPath(
                It.IsAny<string>()))
                .Returns(typeof(TestPlugIn).Assembly);

            _pluginLoader.Validate(_fixture.AssemblyFilePath, out var version);

            Assert.Equal("1.0.0.0", version);
        }
    }
}