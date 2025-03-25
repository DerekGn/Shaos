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

using Microsoft.Extensions.Options;
using Shaos.Services.IO;
using Shaos.Services.Shared.Tests.Extensions;
using System.IO.Compression;
using System.Reflection;

namespace Shaos.Services.Shared.Tests
{
    public abstract class BaseTestFixture : IDisposable
    {
        public const string AssemblyFileName = "Shaos.Test.PlugIn.dll";
        public const string ExtractionFolder = "extracted";
        public const string PackageFileName = "Shaos.Test.PlugIn.zip";
        public const string PackagesFolder = "packages";
        public const string ValidationFolder = "validation";

        public BaseTestFixture(string testProjectName, string plugInProjectName)
        {
            var assemblyDirectory = Path
                .GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);

            var sourcePackageDirectory = assemblyDirectory!.Replace(testProjectName, plugInProjectName).Replace("net8.0", string.Empty);
            var sourcePackageFilePath = Path.Combine(sourcePackageDirectory, PackageFileName);

            var targetPackageDirectory = Path.Combine(Path.Combine(assemblyDirectory, "testing"), Guid.NewGuid().ToString());
            var targetPackageFilePath = Path.Combine(targetPackageDirectory, PackageFileName);

            targetPackageDirectory.CreateDirectory();

            File.Copy(sourcePackageFilePath, targetPackageFilePath, true);

            BinariesPath = targetPackageDirectory;
            PackageFilePath = targetPackageFilePath;

            var optionsInstance = new FileStoreOptions()
            {
                BinariesPath = BinariesPath,
                PackagesPath = BinariesPath
            };

            FileStoreOptions = Options.Create(optionsInstance);

            BinariesValidationPath = Path.Combine(BinariesPath, ValidationFolder);
            ZipFile.ExtractToDirectory(PackageFilePath, BinariesValidationPath, true);
        }

        public IOptions<FileStoreOptions> FileStoreOptions { get; }

        public string PackageFilePath { get; }

        public string BinariesPath { get; }

        public string BinariesValidationPath { get; }

        public virtual void Dispose()
        {
            BinariesPath.DeleteDirectory();
        }
    }
}