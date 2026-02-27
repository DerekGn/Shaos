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
using Shaos.Repository;
using Shaos.Repository.Models;
using Shaos.Testing.Shared;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests
{
    public abstract class BaseServiceTests : BaseTests
    {
        protected readonly Mock<IShaosRepository> MockRepository;

        protected BaseServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            MockRepository = new Mock<IShaosRepository>();
        }

        internal static PlugInInformation CreatePlugInInformation(bool hasConfiguration = false,
                                                                  bool hasLogger = false)
        {
            return new PlugInInformation()
            {
                AssemblyFileName = "AssemblyFileName",
                AssemblyVersion = "1.0.0",
                Directory = "Directory",
                HasConfiguration = hasConfiguration,
                HasLogger = hasLogger,
                PackageFileName = "PackageFileName",
                TypeName = "TypeName"
            };
        }

        internal static PlugInInstance CreatePlugInInstance()
        {
            return new PlugInInstance()
            {
                Id = 1,
                Enabled = true,
                Name = "Test"
            };
        }

        protected void SetupPlugInInstanceGetByIdAsync()
        {
            var plugIn = new PlugIn()
            {
                Description = "Test",
                Name = "Test",
                PlugInInformation = CreatePlugInInformation()
            };

            var plugInInstance = CreatePlugInInstance();

            MockRepository
                .Setup(_ => _.GetByIdAsync<PlugInInstance>(It.IsAny<int>(),
                                                           It.IsAny<bool>(),
                                                           It.IsAny<List<string>?>(),
                                                           It.IsAny<CancellationToken>()))
                .ReturnsAsync(plugInInstance);
        }

        protected void SetupPlugInInstanceGetByIdAsync(PlugInInstance plugInInstance)
        {
            MockRepository
                .Setup(_ => _.GetByIdAsync<PlugInInstance>(It.IsAny<int>(),
                                                           It.IsAny<bool>(),
                                                           It.IsAny<List<string>?>(),
                                                           It.IsAny<CancellationToken>()))
                .ReturnsAsync(plugInInstance);
        }

        protected void VerifyGetByIdAsync()
        {
            MockRepository
                .Verify(_ => _.GetByIdAsync<PlugInInstance>(It.IsAny<int>(),
                                                            It.IsAny<bool>(),
                                                            It.IsAny<List<string>?>(),
                                                            It.IsAny<CancellationToken>()));
        }

        protected void VerifySaveAsync()
        {
            MockRepository.Verify(_ => _.SaveChangesAsync(It.IsAny<CancellationToken>()));
        }
    }
}