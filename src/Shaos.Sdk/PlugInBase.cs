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

using Shaos.Sdk.Devices;
using System.Collections.ObjectModel;

namespace Shaos.Sdk
{
    /// <summary>
    /// A base class for a <see cref="IPlugIn"/> derived class
    /// </summary>
    /// <remarks>
    /// Implements the standard dispose pattern
    /// </remarks>
    public abstract class PlugInBase : IDisposable, IPlugIn
    {
        private readonly ObservableCollection<Device> _devices = [];

        private bool _disposedValue;

        /// <inheritdoc/>
        public ObservableCollection<Device> Devices => _devices;

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public abstract Task ExecuteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Indicates if the dispose was called directly</param>
        /// <remarks>
        /// If disposing equals true, the method has been called directly or indirectly by a user's code.
        /// Managed and unmanaged resources can be disposed.
        /// If disposing equals false, the method has been called by the runtime from inside the finalizer
        /// and you should not reference other objects. Only unmanaged resources can be disposed.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    DisposeManaged();
                }

                DisposeUnmanaged();

                _disposedValue = true;
            }
        }

        /// <summary>
        /// Dispose managed resources
        /// </summary>
        /// <remarks>
        /// Maybe overridden in a derived class
        /// </remarks>
        protected virtual void DisposeManaged()
        {
        }

        /// <summary>
        /// Dispose unmanaged resources
        /// </summary>
        /// <remarks>
        /// Maybe overridden in a derived class
        /// </remarks>
        protected virtual void DisposeUnmanaged()
        {
        }
    }
}