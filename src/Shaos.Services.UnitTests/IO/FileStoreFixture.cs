using Microsoft.Extensions.Options;
using Shaos.Services.IO;
using Shaos.Services.UnitTests.Extensions;
using System.Reflection;

namespace Shaos.Services.UnitTests.IO
{
    public class FileStoreFixture : IDisposable
    {
        public const string ExtractionFolder = "extracted";
        public const string PackagesFolder = "packages";
        public const string PackageFileName = "Shaos.Test.PlugIn.zip";

        public FileStoreFixture()
        {
            AssemblyDirectory = Path
                .GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);

            var sourcePackageDirectory = AssemblyDirectory!.Replace("Shaos.Services.UnitTests", "Shaos.Test.PlugIn").Replace("net8.0", string.Empty);
            var sourcePackageFilePath = Path.Combine(sourcePackageDirectory, PackageFileName);

            var targetPackageDirectory = Path.Combine(AssemblyDirectory, "testing");
            var targetPackageFilePath = Path.Combine(targetPackageDirectory, PackageFileName);

            targetPackageDirectory.CreateFolder();

            File.Copy(sourcePackageFilePath, targetPackageFilePath, true);

            SourcePath = targetPackageDirectory;
            SourceFilePath = targetPackageFilePath;

            var optionsInstance = new FileStoreOptions()
            {
                BinariesPath = SourcePath,
                PackagesPath = SourcePath
            };

            FileStoreOptions = Options.Create(optionsInstance);
        }

        public string? AssemblyDirectory { get; }

        public IOptions<FileStoreOptions> FileStoreOptions { get; }

        public string SourceFilePath { get; }
        public string SourcePath { get; }

        public void Dispose()
        {
            var extractedFolder = Path.Combine(
                FileStoreOptions.Value.PackagesPath,
                ExtractionFolder);

            if (Directory.Exists(extractedFolder))
            {
                Directory.Delete(Path.Combine(
                    FileStoreOptions.Value.PackagesPath,
                    ExtractionFolder),
                    true);
            }
        }
    }
}