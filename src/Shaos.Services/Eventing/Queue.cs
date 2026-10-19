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

using System.Threading.Channels;

namespace Shaos.Services.Eventing
{
    /// <summary>
    /// Defines a queue of <typeparamref name="T"/>
    /// </summary>
    public class Queue<T> : IQueue<T>
    {
        private readonly Channel<T> _queue;

        /// <summary>
        /// Create an instance of a <typeparamref name="T"/>
        /// </summary>
        /// <param name="capacity">The queue capacity</param>
        public Queue(int capacity)
        {
            BoundedChannelOptions options = new(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };

            _queue = Channel.CreateBounded<T>(options);
        }

        /// <inheritdoc/>
        public int Count => _queue.Reader.Count;

        /// <inheritdoc/>
        public async Task<T?> DequeueAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _queue.Reader.ReadAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return default(T);
            }
        }

        /// <inheritdoc/>
        public async Task EnqueueAsync(T @event,
                                       CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(@event);

            await _queue.Writer.WriteAsync(@event,
                                           cancellationToken);
        }
    }
}
