
namespace Shaos.Services.Runtime
{
    public class UnloadingWeakReference<T> : IDisposable where T : class
    {
        private readonly WeakReference _weakReference;
        private bool disposedValue;

        public UnloadingWeakReference(T target)
        {
            ArgumentNullException.ThrowIfNull(target);

            _weakReference = new WeakReference(target);
        }

        public T Target => (T)_weakReference.Target!;

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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
