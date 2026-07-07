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

using Serilog;
using Serilog.Configuration;

namespace Shaos.Services.Logging
{
    /// <summary>
    /// Extension for injection of <see cref="ServerSentSink"/>
    /// </summary>
    public static class ServerSentSinkExtensions
    {
        /// <summary>
        /// Add a <see cref="ServerSentSink"/> instance to the serilog sinks collection
        /// </summary>
        /// <param name="loggerConfiguration">The <see cref="LoggerSinkConfiguration"/></param>
        /// <param name="loggerItemQueue">The <see cref="ILoggerItemQueue"/> for writing log events</param>
        /// <param name="formatProvider">The optional <see cref="IFormatProvider"/> instance</param>
        /// <returns>The <see cref="LoggerConfiguration"/></returns>
        public static LoggerConfiguration ServerSentSink(this LoggerSinkConfiguration loggerConfiguration,
                                                         ILoggerItemQueue loggerItemQueue,
                                                         IFormatProvider? formatProvider = null)
        {
            return loggerConfiguration.Sink(new ServerSentEventsSink(loggerItemQueue,
                                                                     formatProvider));
        }
    }
}
