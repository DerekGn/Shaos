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
using Serilog.Core;
using Serilog.Events;
using Shaos.Repository;
using Shaos.Repository.Models;
using Shaos.Services.Logging;
using Shaos.Testing.Shared;
using Shaos.Testing.Shared.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests.Logging
{
    public class LoggingConfigurationServiceTests : BaseTests
    {
        private readonly LoggingConfigurationService _loggingConfigurationService;
        private readonly Mock<ILoggingConfiguration> _mockLoggingConfiguration;
        private readonly Mock<IRepository> _mockRepository;

        public LoggingConfigurationServiceTests(ITestOutputHelper output) : base(output)
        {
            _mockRepository = new Mock<IRepository>();
            _mockLoggingConfiguration = new Mock<ILoggingConfiguration>();

            _loggingConfigurationService = new LoggingConfigurationService(LoggerFactory!.CreateLogger<LoggingConfigurationService>(),
                                                                           _mockRepository.Object);
        }

        [Fact]
        public async Task TestInitialiseLoggingConfigurationAsync()
        {
            List<LogLevelSwitch> logLevels =
            [
                new()
                {
                    Name = "test",
                    Level = LogEventLevel.Information
                }
            ];

            var dictionary = new Dictionary<string, LoggingLevelSwitch>
            {
                { "test", new LoggingLevelSwitch() }
            };

            _mockRepository
                .Setup(_ => _.GetEnumerableAsync<LogLevelSwitch>(It.IsAny<CancellationToken>()))
                .Returns(logLevels.ToAsyncEnumerable());

            _mockLoggingConfiguration
                .Setup(_ => _.LoggingLevelSwitches)
                .Returns(dictionary);

            await _loggingConfigurationService.InitialiseLoggingConfigurationAsync(_mockLoggingConfiguration.Object);
        }

        [Fact]
        public async Task TestUpdateLogLevelSwitchAsync()
        {
            await _loggingConfigurationService.UpdateLogLevelSwitchAsync("test",
                                                                         LogEventLevel.Information);

            _mockRepository.Verify(_ => _.UpsertLogLevelSwitchAsync(It.IsAny<string>(),
                                                                    It.IsAny<LogEventLevel>(),
                                                                    It.IsAny<CancellationToken>()));
        }
    }
}