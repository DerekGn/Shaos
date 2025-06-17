
using Microsoft.Extensions.Logging;
using Shaos.Services.Runtime;
using Shaos.Services.UnitTests.Fixtures;
using Shaos.Testing.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests
{
    public abstract class PlugInFactoryBaseTests : BaseTests, IClassFixture<TestFixture>
    {
        internal readonly UnloadingWeakReference<RuntimeAssemblyLoadContext> _unloadingWeakReference;
        internal readonly TestFixture _fixture;

        protected PlugInFactoryBaseTests(ITestOutputHelper outputHelper,
                                         TestFixture fixture) : base(outputHelper)
        {
            _fixture = fixture;

            var assemblyLoadContext = new RuntimeAssemblyLoadContext(LoggerFactory!.CreateLogger<RuntimeAssemblyLoadContext>(), _fixture.AssemblyFilePath);
            _unloadingWeakReference = new UnloadingWeakReference<RuntimeAssemblyLoadContext>(assemblyLoadContext);
        }
    }
}
