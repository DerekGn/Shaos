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
using Microsoft.Extensions.Options;
using Shaos.Sdk;
using System.Diagnostics.CodeAnalysis;

namespace Shaos.Test.PlugIn
{
    [ExcludeFromCodeCoverage]
    public class TestPlugIn : IPlugIn
    {
        private readonly ILogger<TestPlugIn> _logger;
        private readonly IOptions<TestPlugInOptions> _options;

        public TestPlugIn(
            ILogger<TestPlugIn> logger,
            IOptions<TestPlugInOptions> options)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(options);

            _logger = logger;
            _options = options;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting [{Name}].[{Operation}]", nameof(TestPlugIn), nameof(ExecuteAsync));

            do
            {
                await Task.Delay(_options.Value.Delay, cancellationToken);
                _logger.LogInformation("Executing [{Name}].[{Operation}]", nameof(TestPlugIn), nameof(ExecuteAsync));
            } while (!cancellationToken.IsCancellationRequested);

            _logger.LogInformation("Completed [{Name}].[{Operation}]", nameof(TestPlugIn), nameof(ExecuteAsync));
        }
    }
}