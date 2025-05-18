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
using System.Reflection;
using System.Text;

namespace Shaos.Services.Runtime
{
    public class PlugInLoadContext : IDisposable
    {
        private UnloadingWeakReference<IRuntimeAssemblyLoadContext> _unloadingWeakReference;
        private bool disposedValue;

        public PlugInLoadContext(IRuntimeAssemblyLoadContext assemblyLoadContext, string assemblyPath)
        {
            ArgumentNullException.ThrowIfNull(assemblyLoadContext);

            RuntimeAssemblyLoadContext = assemblyLoadContext;
            _unloadingWeakReference = new UnloadingWeakReference<IRuntimeAssemblyLoadContext>(assemblyLoadContext);

            PlugInAssembly = RuntimeAssemblyLoadContext.LoadFromAssemblyPath(assemblyPath);
        }

        public IPlugIn? PlugIn { get; private set; }

        /// <summary>
        /// The PlugIn Assembly
        /// </summary>
        public Assembly PlugInAssembly { get; private set; }

        public object? PlugInConfiguration { get; private set; }

        /// <summary>
        /// The assembly load context used to load the PlugIn assembly
        /// </summary>
        public IRuntimeAssemblyLoadContext RuntimeAssemblyLoadContext { get; private set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{nameof(PlugIn)}: {(PlugIn == null ? "Empty" : PlugIn.GetType().Name)}");
            stringBuilder.AppendLine($"{nameof(PlugInConfiguration)}: {(PlugInConfiguration == null ? "Empty" : PlugInConfiguration.GetType().Name)}");

            return stringBuilder.ToString();
        }

        internal void LoadPlugIn(IPlugIn plugIn, object? configuration)
        {
            PlugIn = plugIn;
            PlugInConfiguration = configuration;
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
                if (PlugIn is IDisposable plugin)
                {
                    plugin?.Dispose();
                }

                PlugIn = null;

                if (PlugInConfiguration is IDisposable config)
                {
                    config?.Dispose();
                }

                PlugInConfiguration = null;

                _unloadingWeakReference?.Dispose();

                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~PlugInContext()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        #endregion Dispose
    }
}