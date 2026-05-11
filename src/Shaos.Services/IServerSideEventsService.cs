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

using Shaos.Services.Eventing;
using System.Net.ServerSentEvents;

namespace Shaos.Services
{
    /// <summary>
    /// A server side event service for managing event streaming
    /// </summary>
    public interface IServerSideEventsService
    {
        /// <summary>
        /// Broadcast an <see cref="BaseEvent"/> to subscribed event listeners
        /// </summary>
        /// <param name="baseEvent"></param>
        Task BroadcastEventAsync(BaseEvent baseEvent);

        /// <summary>
        /// Stream application
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        IAsyncEnumerable<SseItem<ApplicationEvent>> StreamApplicationEventsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Read streamed parameter events asynchronously
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="BaseEvent"/> instances</returns>
        IAsyncEnumerable<SseItem<BaseParameterUpdatedEvent>> StreamParameterEventsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Read streamed parameter events asynchronously
        /// </summary>
        /// <param name="id">The parameter identifier</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="BaseEvent"/> instances</returns>
        IAsyncEnumerable<SseItem<BaseParameterUpdatedEvent>> StreamParameterEventsByIdAsync(int id,
                                                                                            CancellationToken cancellationToken);
    }
}