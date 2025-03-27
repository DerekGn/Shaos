using Shaos.Services.Runtime;
using System.IO.Compression;
using System.Reflection;

namespace Shaos.Services.IntTests.Fixtures
{
    public class PlugInFactoryTestFixture : TestFixture
    {
        public PlugInFactoryTestFixture()
        {
            var assemblyDirectory = Path.Combine(BinariesPath, "1");

            ZipFile.ExtractToDirectory(PackageFilePath, assemblyDirectory, true);

            var assemblyFilePath = Path.Combine(assemblyDirectory, AssemblyFileName);

            AssemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(assemblyFilePath));
            var assemblyLoadContext = new RuntimeAssemblyLoadContext(nameof(PlugInFactoryTestFixture), assemblyFilePath);

            AssemblyLoadContextReference = new UnloadingWeakReference<RuntimeAssemblyLoadContext>(assemblyLoadContext);
        }

        public UnloadingWeakReference<RuntimeAssemblyLoadContext> AssemblyLoadContextReference { get; }
        public AssemblyName AssemblyName { get; }

        public override void Dispose()
        {
            AssemblyLoadContextReference.Dispose();

            base.Dispose();
        }
    }
}