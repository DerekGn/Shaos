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

using System.Reflection;

namespace Shaos.Services.Runtime.Host
{
    /// <summary>
    /// An instance load context
    /// </summary>
    /// <remarks>
    /// A container for a <see cref="IRuntimeAssemblyLoadContext"/>
    /// The container will release the <see cref="IRuntimeAssemblyLoadContext"/> on dispose
    /// </remarks>
    public class InstanceLoadContext : IDisposable
    {
        private readonly UnloadingWeakReference<IRuntimeAssemblyLoadContext>? _unloadingWeakReference;
        private bool disposedValue;

        /// <summary>
        /// Create an instance of a <see cref="InstanceLoadContext"/>
        /// </summary>
        /// <param name="assembly"></param>
        internal InstanceLoadContext(Assembly assembly)
        {
            Assembly = assembly;
        }

        /// <summary>
        /// Create an instance of a <see cref="InstanceLoadContext"/>
        /// </summary>
        /// <param name="assemblyLoadContext">The <see cref="IRuntimeAssemblyLoadContext"/></param>
        public InstanceLoadContext(IRuntimeAssemblyLoadContext assemblyLoadContext)
        {
            ArgumentNullException.ThrowIfNull(assemblyLoadContext);

            _unloadingWeakReference = new UnloadingWeakReference<IRuntimeAssemblyLoadContext>(assemblyLoadContext);

            Assembly = _unloadingWeakReference.Target.LoadFromAssemblyPath(assemblyLoadContext.AssemblyPath);
        }

        /// <summary>
        /// The PlugIn instance assembly
        /// </summary>
        public Assembly? Assembly { get; private set; }

        #region Dispose

        /// <summary>
        /// Dispose the instance releasing the <see cref="IRuntimeAssemblyLoadContext"/>
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the instance releasing the <see cref="IRuntimeAssemblyLoadContext"/>
        /// </summary>
        /// <param name="disposing">Indicates that instance is explicitly being disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Assembly = null;

                    _unloadingWeakReference?.Target.Unload();

                    _unloadingWeakReference?.Dispose();
                }

                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~InstanceContext()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        #endregion Dispose
    }
}