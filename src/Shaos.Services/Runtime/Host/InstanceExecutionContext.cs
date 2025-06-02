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

using Shaos.Sdk;
using System.Text;

namespace Shaos.Services.Runtime.Host
{
    /// <summary>
    /// An instance execution context type
    /// </summary>
    /// <remarks>
    /// This is a container for <see cref="IPlugIn"/> instances. It manages the start and dispose of an <see cref="IPlugIn"/> instance
    /// </remarks>
    public class InstanceExecutionContext : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// Create an instance of an <see cref="InstanceExecutionContext"/>
        /// </summary>
        /// <param name="plugIn">The <see cref="IPlugIn"/> to manage</param>
        public InstanceExecutionContext(IPlugIn plugIn)
        {
            ArgumentNullException.ThrowIfNull(plugIn);

            PlugIn = plugIn;
        }

        /// <summary>
        /// The <see cref="IPlugIn"/> instance contained by this <see cref="InstanceExecutionContext"/>
        /// </summary>
        public IPlugIn? PlugIn { get; private set; }

        /// <summary>
        /// The execution <see cref="Task"/> for the <see cref="IPlugIn"/> instance
        /// </summary>
        public Task? Task { get; private set; }

        /// <summary>
        /// The <see cref="CancellationTokenSource"/> used to cancel the executing <see cref="Task"/>
        /// </summary>
        public CancellationTokenSource? TokenSource { get; private set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{nameof(PlugIn)}: {(PlugIn == null ? "Empty" : PlugIn.GetType().Name)}");
            stringBuilder.AppendLine($"{nameof(Task)}: {(Task == null ? "Empty" : $"Id: {Task.Id} State: [{Task.Status}]")}");
            stringBuilder.AppendLine($"{nameof(TokenSource)}: {(TokenSource == null ? "Empty" : $"{nameof(TokenSource.IsCancellationRequested)}: {TokenSource.IsCancellationRequested}")}");

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Start executing of the <see cref="PlugIn"/> task
        /// </summary>
        /// <param name="executeTask">The <see cref="Task"/> that executes the <see cref="IPlugIn"/> execution method</param>
        /// <param name="completionTask">The <see cref="Task"/> to execute on completion of the <paramref name="executeTask"/></param>
        internal void StartExecution(Func<CancellationToken, Task> executeTask,
                                     Action<Task> completionTask)
        {
            ArgumentNullException.ThrowIfNull(executeTask);
            ArgumentNullException.ThrowIfNull(completionTask);

            TokenSource = new CancellationTokenSource();

            Task = Task
                .Run(async () => await executeTask(TokenSource.Token))
                .ContinueWith(completionTask);
        }

        #region Dispose

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    PlugIn?.Dispose();
                    PlugIn = null;
                }

                disposedValue = true;
            }
        }

        #endregion Dispose
    }
}