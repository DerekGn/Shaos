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

using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;
using Shaos.Services.Extensions;
using Shaos.Services.Options;

namespace Shaos.Services.Package
{
    public class NuGetPackageService : INuGetPackageService
    {
        private readonly ILogger<NuGetPackageService> _logger;
        private readonly NuGetFramework _nuGetFramework;
        private readonly NuGetPackageLogger _nuGetLogger;
        private readonly IOptions<NuGetPackageServiceOptions> _options;
        private readonly IEnumerable<SourceRepository> _repositories;
        private readonly ISettings _settings;
        private readonly SourceCacheContext _sourceCacheContext;
        private readonly SourceRepositoryProvider _sourceRepositoryProvider;

        public NuGetPackageService(
            ILogger<NuGetPackageService> logger,
            IOptions<NuGetPackageServiceOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _nuGetFramework = FrameworkConstants.CommonFrameworks.Net80;
            _nuGetLogger = new NuGetPackageLogger(_logger);
            _sourceCacheContext = new SourceCacheContext();

            var packageSourceProvider = options.Value.GetPackageSourceProvider();

            _sourceRepositoryProvider = new SourceRepositoryProvider(
                packageSourceProvider,
                NuGet.Protocol.Core.Types.Repository.Provider.GetCoreV3());

            _repositories = _sourceRepositoryProvider.GetRepositories();
            _settings = Settings.LoadDefaultSettings(_options.Value.PackageFolder);
        }

        public async Task<NuGetPackageResolveResult> ResolvePackagesAsync(
            NuGetPackageResolveRequest packageResolveRequest,
            CancellationToken cancellationToken = default)
        {
            NuGetPackageResolveResult? result;
            var resolvedSourcePackages = new HashSet<SourcePackageDependencyInfo>();

            _logger.LogDebug("Resolving [{PackageName}] [{PackageVersion}] [{PreRelease}]",
                packageResolveRequest.Package,
                packageResolveRequest.Version,
                packageResolveRequest.PreRelease);

            var packageIdentity = await GetPackageIdentityAsync(
                packageResolveRequest, cancellationToken);

            if (packageIdentity is null)
            {
                _logger.LogDebug("Unable to resolve [{PackageName}] [{PackageVersion}] [{PreRelease}]",
                    packageResolveRequest.Package,
                    packageResolveRequest.Version,
                    packageResolveRequest.PreRelease);

                result = new NuGetPackageResolveResult()
                {
                    Request = packageResolveRequest,
                    Status = ResolveStatus.NotFound
                };
            }
            else
            {
                _logger.LogInformation(
                    "Resolved [{PackageName}] [{PackageVersion}] [{PreRelease}] " +
                    "Resolved Package: [{Id}] Version: [{Version}]",
                    packageResolveRequest.Package,
                    packageResolveRequest.Version,
                    packageResolveRequest.PreRelease,
                    packageIdentity.Id,
                    packageIdentity.Version);

                await GetPackageDependenciesAsync(
                    packageIdentity,
                    DependencyContext.Default!,
                    resolvedSourcePackages,
                    cancellationToken);

                result = new NuGetPackageResolveResult()
                {
                    Request = packageResolveRequest,
                    Status = ResolveStatus.Success,
                    Identity = packageIdentity,
                    Dependencies = GetPackageDependencies(packageResolveRequest, resolvedSourcePackages, cancellationToken)
                };
            }

            return result;
        }

        private static bool HostSuppliedDependancy(
            DependencyContext hostDependencies,
            PackageDependency packageDependency)
        {
            bool result = false;

            var runtimeLibrary = hostDependencies.RuntimeLibraries.FirstOrDefault(r => r.Name == packageDependency.Id);

            if (runtimeLibrary != null)
            {
                var runtimeLibraryVersion = NuGetVersion.Parse(runtimeLibrary.Version);

                if (runtimeLibraryVersion.IsPrerelease)
                {
                    result = true;
                }
                else
                {
                    result = packageDependency.VersionRange.Satisfies(runtimeLibraryVersion);
                }
            }

            return result;
        }

        private async Task GetPackageDependenciesAsync(
            PackageIdentity packageIdentity,
            DependencyContext dependencyContext,
            HashSet<SourcePackageDependencyInfo> resolvedSourcePackages,
            CancellationToken cancellationToken)
        {
            if (!resolvedSourcePackages.Contains(packageIdentity))
            {
                foreach (var repository in _repositories)
                {
                    var dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>(cancellationToken);

                    var resolvedPackageDependencyInfo = await dependencyInfoResource.ResolvePackage(
                        packageIdentity,
                        _nuGetFramework,
                        _sourceCacheContext,
                        _nuGetLogger,
                        cancellationToken);

                    if (resolvedPackageDependencyInfo == null)
                    {
                        continue;
                    }

                    var sourcePackageDependencyInfo = new SourcePackageDependencyInfo(
                       resolvedPackageDependencyInfo.Id,
                       resolvedPackageDependencyInfo.Version,
                       resolvedPackageDependencyInfo.Dependencies.Where(_ => !HostSuppliedDependancy(dependencyContext, _)),
                       resolvedPackageDependencyInfo.Listed,
                       resolvedPackageDependencyInfo.Source);

                    resolvedSourcePackages.Add(sourcePackageDependencyInfo);

                    foreach (var dependency in sourcePackageDependencyInfo.Dependencies)
                    {
                        await GetPackageDependenciesAsync(
                            new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion),
                            dependencyContext,
                            resolvedSourcePackages,
                            cancellationToken);
                    }
                }
            }
        }

        private async Task<PackageIdentity?> GetPackageIdentityAsync(
            NuGetPackageResolveRequest resolveNuGetPackage,
            CancellationToken cancellationToken)
        {
            NuGetVersion? nuGetVersion = null;

            foreach (var sourceRepository in _repositories)
            {
                var packageByIdResource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);

                var packageVersions = await packageByIdResource.GetAllVersionsAsync(
                    resolveNuGetPackage.Package,
                    _sourceCacheContext,
                    _nuGetLogger,
                    cancellationToken);

                if (resolveNuGetPackage.GetVersionRange(out var versionRange))
                {
                    nuGetVersion = versionRange?.FindBestMatch(packageVersions.Where(_ => resolveNuGetPackage.PreRelease || !_.IsPrerelease));
                    break;
                }
                else
                {
                    nuGetVersion = packageVersions.LastOrDefault(_ => _.IsPrerelease == resolveNuGetPackage.PreRelease);
                    break;
                }
            }

            return nuGetVersion == null ? null : new PackageIdentity(resolveNuGetPackage.Package, nuGetVersion);
        }

        private IEnumerable<SourcePackageDependencyInfo> GetPackageDependencies(
            NuGetPackageResolveRequest packageResolveRequest,
            HashSet<SourcePackageDependencyInfo> resolvedSourcePackages,
            CancellationToken cancellationToken)
        {
            var resolverContext = new PackageResolverContext(
                DependencyBehavior.Lowest,
                [packageResolveRequest.Package],
                Enumerable.Empty<string>(),
                Enumerable.Empty<PackageReference>(),
                Enumerable.Empty<PackageIdentity>(),
                resolvedSourcePackages,
                _sourceRepositoryProvider.GetRepositories().Select(_ => _.PackageSource),
                _nuGetLogger);

            var resolver = new PackageResolver();

            return resolver
                .Resolve(resolverContext, cancellationToken)
                .Select(_ => resolvedSourcePackages.Single(x => PackageIdentityComparer.Default.Equals(x, _)));
        }

        private async Task InstallPackagesAsync(
            IEnumerable<SourcePackageDependencyInfo> packagesToInstall,
            CancellationToken cancellationToken)
        {
            var packagePathResolver = new PackagePathResolver(_options.Value.PackageFolder, true);
            var packageExtractionContext = new PackageExtractionContext(
                PackageSaveMode.Defaultv3,
                XmlDocFileSaveMode.Skip,
                ClientPolicyContext.GetClientPolicy(_settings, _nuGetLogger),
                _nuGetLogger);

            foreach (var package in packagesToInstall)
            {
                var downloadResource = await package.Source.GetResourceAsync<DownloadResource>(cancellationToken);

                // Download the package (might come from the shared package cache).
                var downloadResult = await downloadResource.GetDownloadResourceResultAsync(
                    package,
                    new PackageDownloadContext(_sourceCacheContext),
                    SettingsUtility.GetGlobalPackagesFolder(_settings),
                    _nuGetLogger,
                    cancellationToken);

                // Extract the package into the target directory.
                await PackageExtractor.ExtractPackageAsync(
                    downloadResult.PackageSource,
                    downloadResult.PackageStream,
                    packagePathResolver,
                    packageExtractionContext,
                    cancellationToken);
            }
        }
    }
}