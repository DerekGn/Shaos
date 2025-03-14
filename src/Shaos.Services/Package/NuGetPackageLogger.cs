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
using NuGet.Common;

namespace Shaos.Services.Package
{
    internal class NuGetPackageLogger : LoggerBase
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public NuGetPackageLogger(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override void Log(ILogMessage message)
        {
            LogMessage(message);
        }

        public override Task LogAsync(ILogMessage message)
        {
            LogMessage(message);

            return Task.CompletedTask;
        }

        private void LogMessage(ILogMessage message)
        {
            switch (message.Level)
            {
                case NuGet.Common.LogLevel.Debug:
                    _logger.LogDebug(message.FormatWithCode());
                    break;
                case NuGet.Common.LogLevel.Verbose:
                    _logger.LogTrace(message.FormatWithCode());
                    break;
                case NuGet.Common.LogLevel.Information:
                    _logger.LogInformation(message.FormatWithCode());
                    break;
                case NuGet.Common.LogLevel.Minimal:
                    _logger.LogDebug(message.FormatWithCode());
                    break;
                case NuGet.Common.LogLevel.Warning:
                    _logger.LogWarning(message.FormatWithCode());
                    break;
                case NuGet.Common.LogLevel.Error:
                    _logger.LogError(message.FormatWithCode());
                    break;
                default:
                    break;
            }
        }
    }
}