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

namespace Shaos.Services.Runtime
{
    public class InstanceExecutionContext : IDisposable
    {
        private bool disposedValue;

        public InstanceExecutionContext(IPlugIn plugIn)
        {
            ArgumentNullException.ThrowIfNull(plugIn);
            
            PlugIn = plugIn;
        }

        public IPlugIn? PlugIn { get; private set; }
        public Task? Task { get; private set; }
        public CancellationTokenSource? TokenSource { get; private set; }

        internal void StartExecution(
            Func<CancellationToken, Task> executeTask,
            Action<Task> completionTask)
        {
            ArgumentNullException.ThrowIfNull(executeTask);
            ArgumentNullException.ThrowIfNull(completionTask);

            TokenSource = new CancellationTokenSource();

            Task = Task
                .Run(async () => await executeTask(TokenSource.Token))
                .ContinueWith(completionTask);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{nameof(PlugIn)}: {(PlugIn == null ? "Empty" : PlugIn.GetType().Name)}");
            stringBuilder.AppendLine($"{nameof(Task)}: {(Task == null ? "Empty" : $"Id: {Task.Id} State: [{Task.Status}]" )}");
            stringBuilder.AppendLine($"{nameof(TokenSource)}: {(TokenSource == null ? "Empty" : $"{nameof(TokenSource.IsCancellationRequested)}: {TokenSource.IsCancellationRequested}" )}");

            return stringBuilder.ToString();
        }

        #region Dispose

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

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