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

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Shaos.Sdk;

[assembly: ExcludeFromCodeCoverage]

namespace Shaos.Test.PlugIn
{
    [ExcludeFromCodeCoverage]
    [PlugInDescription(Name = "A test plugin", Description = "A test plugin that does nothing")]
    public class TestPlugIn : PlugInBase
    {
        private readonly ILogger<TestPlugIn> _logger;
        private readonly TestPlugInConfiguration _configuration;

        public TestPlugIn(
            ILogger<TestPlugIn> logger,
            TestPlugInConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(configuration);

            _logger = logger;
            _configuration = configuration;
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting [{Name}].[{Operation}]", nameof(TestPlugIn), nameof(ExecuteAsync));

            do
            {
                await Task.Delay(_configuration.Delay, cancellationToken);
                _logger.LogInformation("Executing [{Name}].[{Operation}]", nameof(TestPlugIn), nameof(ExecuteAsync));
            } while (!cancellationToken.IsCancellationRequested);

            _logger.LogInformation("Completed [{Name}].[{Operation}]", nameof(TestPlugIn), nameof(ExecuteAsync));
        }
    }
}