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

namespace Shaos.Services.System
{
    /// <summary>
    /// Represents a system service for query of system runtime information
    /// </summary>
    public interface ISystemService
    {
        /// <summary>
        /// Gets the <see cref="SystemEnvironment"/> details
        /// </summary>
        /// <returns>A <see cref="SystemEnvironment"/></returns>
        SystemEnvironment GetEnvironment();

        /// <summary>
        /// Get information about the operating system
        /// </summary>
        /// <returns>A <see cref="RuntimeInformation"/></returns>
        RuntimeInformation GetOsInformation();

        /// <summary>
        /// Get the <see cref="ProcessInformation"/>
        /// </summary>
        /// <returns>A <see cref="ProcessInformation"/></returns>
        ProcessInformation GetProcessInformation();

        /// <summary>
        /// Get the application version
        /// </summary>
        /// <returns>The application version in Major.Minor.Build format</returns>
        string GetVersion();

        /// <summary>
        /// Shutdown the application
        /// </summary>
        void ShutdownApplication();
    }
}