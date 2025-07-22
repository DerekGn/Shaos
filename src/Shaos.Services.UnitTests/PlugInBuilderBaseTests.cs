using Microsoft.Extensions.Logging;
using Shaos.Services.Runtime;
using Shaos.Testing.Shared;
using Xunit.Abstractions;
using Xunit;
using Shaos.Services.UnitTests.Fixtures;

namespace Shaos.Services.UnitTests
{
    public abstract class PlugInBuilderBaseTests : BaseTests, IClassFixture<TestFixture>
    {
        internal readonly UnloadingWeakReference<RuntimeAssemblyLoadContext> _unloadingWeakReference;
        internal readonly TestFixture _fixture;

        protected PlugInBuilderBaseTests(ITestOutputHelper outputHelper,
                                         TestFixture fixture) : base(outputHelper)
        {
            _fixture = fixture;

            var assemblyLoadContext = new RuntimeAssemblyLoadContext(LoggerFactory!.CreateLogger<RuntimeAssemblyLoadContext>(), _fixture.AssemblyFilePath);
            _unloadingWeakReference = new UnloadingWeakReference<RuntimeAssemblyLoadContext>(assemblyLoadContext);
        }
    }
}
