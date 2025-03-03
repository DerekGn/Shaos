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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shaos.Sdk;
using Shaos.Services.Options;
using System.Reflection;

namespace Shaos.Services.Compiler
{
    /// <summary>
    /// A compiler service for c#
    /// </summary>
    public class CSharpCompilerService : ICompilerService
    {
        private readonly ILogger<CSharpCompilerService> _logger;
        private readonly IOptions<CSharpCompilerServiceOptions> _options;

        public CSharpCompilerService(
            ILogger<CSharpCompilerService> logger,
            IOptions<CSharpCompilerServiceOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc/>
        public async Task<EmitResult> CompileAsync(
            string assemblyName,
            Stream outputStream,
            IEnumerable<string> files,
            CancellationToken cancellationToken = default)
        {
#warning compiler settings from configuration
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11);
            var syntaxTrees = new List<SyntaxTree>();
            var compilerOptions = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release,
                assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default);

            var dotNetCorePath = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);

            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(IPlugIn).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            };

            foreach (var name in _options.Value.Assemblies)
            {
                references.Add(MetadataReference.CreateFromFile(Path.Combine(dotNetCorePath!, name)));
            }

            foreach (var file in files.Where(file => File.Exists(file)))
            {
                _logger.LogDebug("Loading file [{File}]", file);
                syntaxTrees.Add(
                    SyntaxFactory.ParseSyntaxTree(
                        SourceText.From(
                            await File.ReadAllTextAsync(file, cancellationToken)),
                            options: options,
                            cancellationToken: cancellationToken));
            }

            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees,
                references,
                compilerOptions
            );

            return compilation.Emit(outputStream, cancellationToken: cancellationToken);
        }
    }
}
