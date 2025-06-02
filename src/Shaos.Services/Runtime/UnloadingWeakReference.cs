
namespace Shaos.Services.Runtime
{
    /// <summary>
    /// An unloading weak reference
    /// </summary>
    /// <typeparam name="T">The type of the wrapped weak reference</typeparam>
    public class UnloadingWeakReference<T> : IDisposable where T : class
    {
        private readonly WeakReference _weakReference;
        private bool disposedValue;

        /// <summary>
        /// Create an instance of a <see cref="UnloadingWeakReference{T}"/>
        /// </summary>
        /// <param name="target">The instance of the type to wrap</param>
        public UnloadingWeakReference(T target)
        {
            ArgumentNullException.ThrowIfNull(target);

            _weakReference = new WeakReference(target);
        }

        /// <summary>
        /// The target reference
        /// </summary>
        public T Target => (T)_weakReference.Target!;

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
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

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
