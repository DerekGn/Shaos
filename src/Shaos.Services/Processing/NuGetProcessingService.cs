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

using Microsoft.Extensions.Logging;
using NuGet.Packaging;
using Shaos.Services.Package;

namespace Shaos.Services.Processing
{
    public class NuGetProcessingService : INuGetProcessingService
    {
        private readonly ILogger<NuGetProcessingService> _logger;
        private readonly INuGetPackageSourceService _nuGetPackageService;
        private readonly Lazy<IList<DownloadProcessingStep>> _processingSteps;

        public NuGetProcessingService(
            ILogger<NuGetProcessingService> logger,
            INuGetPackageSourceService nuGetPackageService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _nuGetPackageService = nuGetPackageService ?? throw new ArgumentNullException(nameof(nuGetPackageService));

            _processingSteps = new Lazy<IList<DownloadProcessingStep>>(InitaliseProcessingSteps);
        }

        public async Task DownloadNuGetAsync(
            NuGetSpecification specification,
            CancellationToken cancellationToken = default)
        {
            DownloadNuGetContext downloadNuGetContext = new DownloadNuGetContext(specification);

            foreach (var step in _processingSteps.Value)
            {
                if (!await step.ProcessAsync(downloadNuGetContext, cancellationToken))
                {
                    break;
                }
            }

        }

        private async Task<bool> DownloadNuGetPackageAndDependanciesAsync(
            DownloadNuGetContext context,
            CancellationToken cancellationToken)
        {
            bool result = true;

            foreach (var dependency in context.Dependencies)
            {
                var downloadResult = await _nuGetPackageService
                    .DownloadPackageDependenciesAsync(
                    dependency,
                    cancellationToken);

                context.Downloads.Add(downloadResult);

                if (downloadResult.Status == DownloadStatus.Success)
                {
                    context.Dependencies.Remove(dependency);
                }
                else
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        private List<DownloadProcessingStep> InitaliseProcessingSteps()
        {
            return
                [
                    new ()
                    {
                        ProcessAsync = ResolveNuGetSpecificationAsync
                    },
                    new ()
                    {
                        ProcessAsync = DownloadNuGetPackageAndDependanciesAsync
                    }
                ];
        }

        private async Task<bool> ResolveNuGetSpecificationAsync(
            DownloadNuGetContext context,
            CancellationToken cancellationToken)
        {
            bool result = false;

            var resolvedSpecification = await _nuGetPackageService.ResolveNuGetSpecificationAsync(
                context.Specification,
                cancellationToken);

            if (resolvedSpecification != null && resolvedSpecification.Status == ResolveStatus.Success)
            {
                context.Identity = resolvedSpecification.Identity;
                context.Dependencies.AddRange(resolvedSpecification.Dependencies);

                result = true;
            }
            else
            {
                _logger.LogWarning("Unable to resolve specification: [{Specification}]", context.Specification);
            }

            return result;
        }
    }
}