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
    internal class InstanceLoadContext : IDisposable
    {
        private readonly UnloadingWeakReference<IRuntimeAssemblyLoadContext> _unloadingWeakReference;
        private bool disposedValue;

        public InstanceLoadContext(string assemblyPath,
                                   IRuntimeAssemblyLoadContext assemblyLoadContext)
        {
            ArgumentNullException.ThrowIfNull(assemblyLoadContext);

            _unloadingWeakReference = new UnloadingWeakReference<IRuntimeAssemblyLoadContext>(assemblyLoadContext);

            AssemblyPath = assemblyPath;
            Assembly = _unloadingWeakReference.Target.LoadFromAssemblyPath(assemblyPath);
        }

        /// <summary>
        /// The path to the assembly
        /// </summary>
        public string AssemblyPath { get; }

        /// <summary>
        /// The PlugIn instance assembly
        /// </summary>
        public Assembly? Assembly { get; private set; }

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
                    Assembly = null;

                    _unloadingWeakReference.Target.Unload();

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