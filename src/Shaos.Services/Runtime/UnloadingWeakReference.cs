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

namespace Shaos.Services.Runtime
{
    /// <summary>
    /// An unloading weak reference
    /// </summary>
    /// <typeparam name="T">The type of the wrapped weak reference</typeparam>
    public class UnloadingWeakReference<T> : IDisposable where T : class
    {
        private readonly WeakReference _weakReference;
        private T? _value = null;
        private bool disposedValue;

        /// <summary>
        /// Create an instance of a <see cref="UnloadingWeakReference{T}"/>
        /// </summary>
        /// <param name="target">The instance of the type to wrap</param>
        public UnloadingWeakReference(T target)
        {
            ArgumentNullException.ThrowIfNull(target);

            _value = target;

            _weakReference = new WeakReference(target);
        }

        /// <summary>
        /// The target reference
        /// </summary>
        public T Target => (T)_weakReference.Target!;

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
                    _value = null;

#warning Might need to bound this loop with configurable value
                    for (int i = 0; _weakReference.IsAlive && i < 10; i++)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                }

                disposedValue = true;
            }
        }

        #endregion Dispose
    }
}