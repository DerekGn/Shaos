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
using Shaos.Services.IO;
using Shaos.Services.Shared.Tests;
using Shaos.Services.Shared.Tests.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests.IO
{
    public class FileStoreServiceTests : BaseTests, IClassFixture<TestFixture>
    {
        private readonly FileStoreService _fileStoreService;
        private readonly TestFixture _fixture;

        public FileStoreServiceTests(ITestOutputHelper output, TestFixture fixture) : base(output)
        {
            _fixture = fixture;

            _fileStoreService = new FileStoreService(
                LoggerFactory!.CreateLogger<FileStoreService>(),
                _fixture.FileStoreOptions);
        }

        [Fact]
        public void TestDeletePackage()
        {
            var targetPath = Path.Combine(_fixture.BinariesPath, 1.ToString());
            var targetFilePath = Path.Combine(targetPath, TestFixture.PackageFileName);
            
            targetPath.CreateDirectory();

            File.Copy(_fixture.PackageFilePath, targetFilePath, true);

            _fileStoreService.DeletePackage(1, TestFixture.PackageFileName);

            Assert.False(File.Exists(targetFilePath));
        }

        [Fact]
        public void TestExtractPackage()
        {
            var result = _fileStoreService.ExtractPackage(
                TestFixture.PackageFileName,
                TestFixture.ExtractionFolder);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void TestGetAssemblyPath()
        {
            var expectedPath = Path.Combine(_fixture.BinariesPath, "1");

            var result = _fileStoreService.GetAssemblyPath(1);

            Assert.NotNull(result);
            Assert.Equal(expectedPath, result);
        }

        [Fact]
        public void TestPackageExists()
        {
            var exists = _fileStoreService.PackageExists(TestFixture.PackageFileName);

            Assert.True(exists);
        }

        [Fact]
        public async Task TestWritePackageFileStreamAsync()
        {
            using var memoryStream = new MemoryStream();
            memoryStream.Write([0xAA, 0x55]);
            memoryStream.Position = 0;

            var result = await _fileStoreService.WritePackageFileStreamAsync(1, "FileName.txt", memoryStream);

            Assert.NotNull(result);
        }
    }
}