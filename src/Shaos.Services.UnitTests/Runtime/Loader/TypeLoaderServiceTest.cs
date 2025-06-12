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
using Shaos.Services.IO;
using Shaos.Services.Runtime;
using Shaos.Services.Runtime.Factories;
using Shaos.Services.Runtime.Loader;
using System.Reflection;
using Xunit;

namespace Shaos.Services.UnitTests.Runtime.Loader
{
    public class TypeLoaderServiceTest
    {
        private readonly Mock<IFileStoreService> _mockFileStoreService;
        private readonly Mock<IPlugInFactory> _mockPlugInFactory;
        private readonly Mock<IRuntimeAssemblyLoadContextFactory> _mockRuntimeAssemblyLoadContextFactory;
        private readonly TypeLoaderService _typeLoaderService;
        private Mock<IRuntimeAssemblyLoadContext> _mockRuntimeAssemblyLoadContext;

        public TypeLoaderServiceTest()
        {
            _mockRuntimeAssemblyLoadContextFactory = new Mock<IRuntimeAssemblyLoadContextFactory>();
            _mockFileStoreService = new Mock<IFileStoreService>();
            _mockPlugInFactory = new Mock<IPlugInFactory>();

            _typeLoaderService = new TypeLoaderService(_mockPlugInFactory.Object,
                                                       _mockFileStoreService.Object,
                                                       _mockRuntimeAssemblyLoadContextFactory.Object);
        }

        [Fact]
        public void TestLoadConfiguration()
        {
            _mockRuntimeAssemblyLoadContext = new Mock<IRuntimeAssemblyLoadContext>();

            _mockFileStoreService
                .Setup(_ => _.GetAssemblyPath(It.IsAny<int>(),
                                              It.IsAny<string>()))
                .Returns("AssemblyPath");

            _mockRuntimeAssemblyLoadContextFactory
                .Setup(_ => _.Create(It.IsAny<string>()))
                .Returns(_mockRuntimeAssemblyLoadContext.Object);

            _mockRuntimeAssemblyLoadContext
                .Setup(_ => _.LoadFromAssemblyPath(It.IsAny<string>()))
                .Returns(new Test().GetType().Assembly);

            _mockPlugInFactory
                .Setup(_ => _.CreateConfiguration(It.IsAny<Assembly>()))
                .Returns(new Test());

            var result = _typeLoaderService.LoadConfiguration(1,
                                                              "AssemblyFile",
                                                              "{\"id\":5}");

            Assert.NotNull(result);
            Assert.IsType<Test>(result);
            Assert.Equal(5, ((Test)result).Id);
        }

        public class Test
        {
            public int Id { get; set; }
        }
    }
}