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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shaos.Services.Hosting
{
    /// <summary>
    /// A base background service
    /// </summary>
    public abstract class BaseBackgroundService : BackgroundService
    {
        /// <summary>
        /// The <see cref="Logger{T}"/>
        /// </summary>
        protected readonly ILogger<BaseBackgroundService> Logger;

        /// <summary>
        /// Create an instance of a <see cref="BaseBackgroundService"/>
        /// </summary>
        /// <param name="logger">The <see cref="Logger{T}"/> instance.</param>
        protected BaseBackgroundService(ILogger<BaseBackgroundService> logger)
        {
            Logger = logger;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExecuteInternalAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    Logger.LogError("Background task cancelled");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error occurred executing task work item.");
                }
            }
        }

        /// <summary>
        /// Execute the <see cref="BaseBackgroundService"/> process.
        /// </summary>
        /// <param name="stoppingToken">The <see cref="CancellationToken"/> used to cancellation the operation.</param>
        protected abstract Task ExecuteInternalAsync(CancellationToken stoppingToken);
    }
}
