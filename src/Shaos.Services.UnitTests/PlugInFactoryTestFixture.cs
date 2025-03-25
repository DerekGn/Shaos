
using Shaos.Services.Runtime;
using System.Reflection;

namespace Shaos.Services.UnitTests
{
    public class PlugInFactoryTestFixture : TestFixture
    {
        public PlugInFactoryTestFixture()
        {
            var assemblyPath = Path.Combine(BinariesValidationPath, TestFixture.AssemblyFileName);
            AssemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(assemblyPath));
            var assemblyLoadContext = new RuntimeAssemblyLoadContext(nameof(PlugInFactoryTestFixture), assemblyPath);

            AssemblyLoadContextReference = new WeakReference(assemblyLoadContext);
        }

        public AssemblyName AssemblyName { get; }
        public WeakReference AssemblyLoadContextReference { get; }

        public override void Dispose()
        {
            if (AssemblyLoadContextReference != null)
            {
                for (int i = 0; AssemblyLoadContextReference.IsAlive && (i < 10); i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }

            base.Dispose();
        }
    }
}
