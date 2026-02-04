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
using Serilog.Events;
using Shaos.Services.Hosting;
using System.Diagnostics;

namespace Shaos.Services.Processing
{
    /// <summary>
    /// A <see cref="BackgroundService"/> implementation for the asynchronous execution of work items
    /// </summary>
    public partial class WorkItemProcessorBackgroundService : BaseBackgroundService
    {
        private readonly IWorkItemQueue _workItemQueue;
        private readonly Stopwatch _stopwatch;

        /// <summary>
        /// Create an instance of a <see cref="WorkItemProcessorBackgroundService"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> instance</param>
        /// <param name="workItemQueue">The <see cref="WorkItemQueue"/> to dequeue work items from</param>
        public WorkItemProcessorBackgroundService(ILogger<WorkItemProcessorBackgroundService> logger,
                                                  IWorkItemQueue workItemQueue) : base(logger)
        {
            _workItemQueue = workItemQueue;

            _stopwatch = Stopwatch.StartNew();
        }

        /// <inheritdoc/>
        protected override async Task ExecuteInternalAsync(CancellationToken stoppingToken)
        {
            Func<CancellationToken, Task>? workItem =
                        await _workItemQueue.DequeueAsync(stoppingToken);

            _stopwatch.Restart();
            await workItem(stoppingToken);
            _stopwatch.Stop();

            LogElapsedWorkItem(_stopwatch.Elapsed,
                            _workItemQueue.Count);
        }

        [LoggerMessage(Level = LogLevel.Debug, Message = "Executed work item Elapsed: [{elapsed}] remaining WorkItems: [{count}]")]
        private partial void LogElapsedWorkItem(TimeSpan elapsed, int count);
    }
}