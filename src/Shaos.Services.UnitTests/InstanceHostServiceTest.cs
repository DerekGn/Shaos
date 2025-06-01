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
using Shaos.Services.IO;
using Shaos.Services.Repositories;
using Shaos.Services.Runtime.Host;
using Shaos.Testing.Shared;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests
{
    public class InstanceHostServiceTest : BaseTests
    {
        private readonly InstanceHostService _instanceHostService;
        private readonly Mock<IFileStoreService> _mockFileStoreService;
        private readonly Mock<IInstanceHost> _mockInstanceHost;
        private readonly Mock<IPlugInInstanceRepository> _mockPlugInInstanceRepository;
        private readonly Mock<IPlugInRepository> _mockPlugInRepository;

        public InstanceHostServiceTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            _mockFileStoreService = new Mock<IFileStoreService>();
            _mockInstanceHost = new Mock<IInstanceHost>();
            _mockPlugInInstanceRepository = new Mock<IPlugInInstanceRepository>();
            _mockPlugInRepository = new Mock<IPlugInRepository>();

            _instanceHostService = new InstanceHostService(
                LoggerFactory!.CreateLogger<InstanceHostService>(),
                _mockInstanceHost.Object,
                _mockFileStoreService.Object,
                _mockPlugInRepository.Object,
                _mockPlugInInstanceRepository.Object);
        }
    }
}
