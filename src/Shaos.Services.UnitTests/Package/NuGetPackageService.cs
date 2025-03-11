using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shaos.Services.Options;
using Shaos.Services.Package;
using Xunit;
using Xunit.Abstractions;

namespace Shaos.Services.UnitTests.Package
{
    public class NuGetPackageServiceTests : BaseUnitTests
    {
        private readonly NuGetPackageService _nuGetPackageService;
        private readonly IOptions<NuGetPackageServiceOptions>? _options;
        private readonly ITestOutputHelper _output;

        public NuGetPackageServiceTests(ITestOutputHelper output)
        {
            var factory = ServiceProvider.GetService<ILoggerFactory>();
            _options = ServiceProvider.GetService<IOptions<NuGetPackageServiceOptions>>();

            _nuGetPackageService = new NuGetPackageService(
                factory!.CreateLogger<NuGetPackageService>(),
                _options);
        }

        [Fact]
        public async Task TestResolvePackagesAsync()
        {
            var packagesToResolve = new List<NuGetPackageResolveRequest>()
            {
                new NuGetPackageResolveRequest()
                {
                    PackageName = "HexIO",
                    PackageVersion = "5.0.1"
                }
            };

            var resolved = await _nuGetPackageService.ResolvePackagesAsync(packagesToResolve);

            Assert.NotNull(resolved);
            Assert.Single(resolved);
        }
    }
}