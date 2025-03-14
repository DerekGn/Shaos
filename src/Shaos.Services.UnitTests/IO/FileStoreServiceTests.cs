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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shaos.Services.IO;
using Shaos.Services.Options;
using Shaos.Services.Shared.Tests;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests.IO
{
    public class FileStoreServiceTests : BaseTests
    {
        private readonly FileStoreService _fileStoreService;
        private readonly IOptions<FileStoreOptions>? _options;

        public FileStoreServiceTests(ITestOutputHelper output) : base(output)
        {
            _options = ServiceProvider.GetService<IOptions<FileStoreOptions>>();

            _fileStoreService = new FileStoreService(
                Factory!.CreateLogger<FileStoreService>(),
                _options!);
        }

        //[Theory]
        //[InlineData("TestCodeFile.txt")]
        //public void GetCodeFileStream(string file)
        //{
        //    var filePath = Path.Combine(_options!.Value.CodeFilesPath, file);

        //    var result = _fileStoreService.GetCodeFileStream(filePath);

        //    Assert.NotNull(result);
        //}

        //[Fact]
        //public void TestCreateAssemblyFileStream()
        //{
        //    using var result = _fileStoreService
        //        .CreateAssemblyFileStream(
        //        "folder", "assemblyFileName",
        //        out var assemblyFilePath);

        //    Assert.NotNull(result);
        //    Assert.NotNull(assemblyFilePath);
        //    Assert.True(File.Exists(assemblyFilePath));
        //}

        //[Theory]
        //[InlineData("./Files/TestFile.txt")]
        //public void TestDeleteCodeFile(string filePath)
        //{
        //    _fileStoreService.DeleteCodeFile(filePath);

        //    Assert.False(File.Exists(filePath));
        //}

        //[Theory]
        //[InlineData("TestFolder")]
        //public void TestDeleteCodeFolder(string folder)
        //{
        //    var folderPath = Path.Combine(_options!.Value.CodeFilesPath, folder);

        //    Directory.CreateDirectory(folderPath);

        //    _fileStoreService.DeleteCodeFolder(folder);

        //    Assert.False(Directory.Exists(folderPath));
        //}

        //[Fact]
        //public async Task TestWriteCodeFileStreamAsync()
        //{
        //    using var memoryStream = new MemoryStream();
        //    memoryStream.Write([0xAA, 0x55]);
        //    memoryStream.Position = 0;

        //    var result = await _fileStoreService.WriteCodeFileStreamAsync("CodeWriteFolder", "FileName.txt", memoryStream);
            
        //    Assert.NotNull(result);
        //}
    }
}
