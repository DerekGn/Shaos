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
using Shaos.Services.Validation;
using Xunit;

namespace Shaos.Services.UnitTests.Validation
{
    public class CodeFileValidationTests
    {
        private const string ContentType = "application/octet-stream";
        private readonly CodeFileValidationService _codeFileValidationService;
        private Mock<IFormFile> _mockFile;

        public CodeFileValidationTests()
        {
            _codeFileValidationService = new CodeFileValidationService();
            _mockFile = new Mock<IFormFile>();
        }

        [Fact]
        public void TestValidateFileInvalidFileName()
        {
            _mockFile.Setup(_ => _.FileName).Returns(string.Empty);

            var result = _codeFileValidationService.ValidateFile(_mockFile.Object);

            Assert.Equal(FileValidationResult.FileNameEmpty, result);
        }

        [Fact]
        public void TestValidateFileInvalidContentType()
        {
            _mockFile.Setup(_ => _.FileName).Returns("filename");
            _mockFile.Setup(_ => _.ContentType).Returns(string.Empty);

            var result = _codeFileValidationService.ValidateFile(_mockFile.Object);

            Assert.Equal(FileValidationResult.InvalidContentType, result);
        }

        [Fact]
        public void TestValidateFileInvalidFileType()
        {
            _mockFile.Setup(_ => _.FileName).Returns("filename.exe");
            _mockFile.Setup(_ => _.ContentType).Returns(ContentType);

            var result = _codeFileValidationService.ValidateFile(_mockFile.Object);

            Assert.Equal(FileValidationResult.InvalidFileName, result);
        }

        [Fact]
        public void TestValidateFileInvalidLength()
        {
            _mockFile.Setup(_ => _.FileName).Returns("filename.nupkg");
            _mockFile.Setup(_ => _.ContentType).Returns(ContentType);
            _mockFile.Setup(_ => _.Length).Returns(0);

            var result = _codeFileValidationService.ValidateFile(_mockFile.Object);

            Assert.Equal(FileValidationResult.InvalidFileLength, result);
        }

        [Fact]
        public void TestValidateFileValid()
        {
            _mockFile.Setup(_ => _.FileName).Returns("filename.nupkg");
            _mockFile.Setup(_ => _.ContentType).Returns(ContentType);
            _mockFile.Setup(_ => _.Length).Returns(10);
            
            var result = _codeFileValidationService.ValidateFile(_mockFile.Object);

            Assert.Equal(FileValidationResult.Success, result);
        }
    }
}
