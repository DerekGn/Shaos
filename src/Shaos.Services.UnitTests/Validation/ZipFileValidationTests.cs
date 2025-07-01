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

using Microsoft.AspNetCore.Http;
using Moq;
using Shaos.Repository.Exceptions;
using Shaos.Services.Exceptions;
using Shaos.Services.Validation;
using Xunit;

namespace Shaos.Services.UnitTests.Validation
{
    public class ZipFileValidationTests
    {
        private const string FileName = "filename";
        private const string FileNameZip = "filename.zip";
        private readonly Mock<IFormFile> _mockFile;
        private readonly ZipFileValidationService _zipFileValidationService;

        public ZipFileValidationTests()
        {
            _zipFileValidationService = new ZipFileValidationService();
            _mockFile = new Mock<IFormFile>();
        }

        [Fact]
        public void TestValidateFileFileNameEmpty()
        {
            _mockFile.Setup(_ => _.FileName).Returns(string.Empty);
            _mockFile.Setup(_ => _.ContentType).Returns(ZipFileValidationService.ContentType);
            _mockFile.Setup(_ => _.Length).Returns(10);

            var exception = Assert.Throws<FileNameEmptyException>(() => _zipFileValidationService.ValidateFile(_mockFile.Object));

            Assert.NotNull(exception);
        }

        [Fact]
        public void TestValidateFileInvalidContentType()
        {
            _mockFile.Setup(_ => _.FileName).Returns(FileName);
            _mockFile.Setup(_ => _.ContentType).Returns("content");

            var exception = Assert.Throws<FileContentInvalidException>(() => _zipFileValidationService.ValidateFile(_mockFile.Object));

            Assert.NotNull(exception);
            Assert.Equal("filename", exception.FileName);
            Assert.Equal("content", exception.ContentType);
        }

        [Fact]
        public void TestValidateFileInvalidContentTypeEmpty()
        {
            _mockFile.Setup(_ => _.FileName).Returns(FileName);
            _mockFile.Setup(_ => _.ContentType).Returns(string.Empty);

            var exception = Assert.Throws<FileContentInvalidException>(() => _zipFileValidationService.ValidateFile(_mockFile.Object));

            Assert.NotNull(exception);
            Assert.Equal("filename", exception.FileName);
            Assert.Equal(string.Empty, exception.ContentType);
        }

        [Fact]
        public void TestValidateFileInvalidFileType()
        {
            _mockFile.Setup(_ => _.FileName).Returns("filename.exe");
            _mockFile.Setup(_ => _.ContentType).Returns(ZipFileValidationService.ContentType);
            _mockFile.Setup(_ => _.Length).Returns(20);

            var exception = Assert.Throws<FileNameInvalidExtensionException>(() => _zipFileValidationService.ValidateFile(_mockFile.Object));

            Assert.NotNull(exception);
            Assert.Equal("filename.exe", exception.FileName);
        }

        [Fact]
        public void TestValidateFileInvalidLength()
        {
            _mockFile.Setup(_ => _.FileName).Returns(FileNameZip);
            _mockFile.Setup(_ => _.ContentType).Returns(ZipFileValidationService.ContentType);
            _mockFile.Setup(_ => _.Length).Returns(0);

            var exception = Assert.Throws<FileLengthInvalidException>(() => _zipFileValidationService.ValidateFile(_mockFile.Object));

            Assert.NotNull(exception);
            Assert.Equal(0, exception.Length);
        }

        [Fact]
        public void TestValidateFileValid()
        {
            _mockFile.Setup(_ => _.FileName).Returns(FileNameZip);
            _mockFile.Setup(_ => _.ContentType).Returns(ZipFileValidationService.ContentType);
            _mockFile.Setup(_ => _.Length).Returns(10);

            var exception = Record.Exception(() => _zipFileValidationService.ValidateFile(_mockFile.Object));

            Assert.Null(exception);
        }
    }
}