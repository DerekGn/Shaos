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

using Shaos.Testing.Shared.Extensions;
using System.IO.Compression;
using System.Reflection;

namespace Shaos.Testing.Shared
{
    public abstract class BaseTestFixture : IDisposable
    {
        public BaseTestFixture(string testProjectName, string packageName)
        {
            var assemblyDirectory = Path
                .GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);

            PlugInId = 1;
            PlugInIdInvalid = 10;

            PackageName = packageName;
            PackageNameInvalid = string.Concat(PackageName, ".Invalid");

            PackageFile = string.Concat(PackageName, ".zip");
            PackageFileInvalid = string.Concat(PackageNameInvalid, ".zip");

            BaseTestDirectory = Path.Combine(Path.Combine(assemblyDirectory, "testing"), Guid.NewGuid().ToString());

            PackageDirectory = Path.Combine(BaseTestDirectory, "packages");
            PackageDirectory.CreateDirectory();

            BinariesDirectory = Path.Combine(BaseTestDirectory, "binaries");
            BinariesDirectory.CreateDirectory();

            var packageDirectory = Path.Combine(PackageDirectory, PlugInId.ToString());
            packageDirectory.CreateDirectory();

            PackageFilePath = Path.Combine(packageDirectory, PackageFile);

            var invalidPackageDirectory = Path.Combine(PackageDirectory, PlugInIdInvalid.ToString());
            invalidPackageDirectory.CreateDirectory();

            PackageFileInvalidPath = Path.Combine(invalidPackageDirectory, PackageFileInvalid);

            CopyPackageFile(
                PackageName,
                PackageFile,
                testProjectName,
                assemblyDirectory,
                packageDirectory);

            CopyPackageFile(
                PackageNameInvalid,
                PackageFileInvalid,
                testProjectName,
                assemblyDirectory,
                invalidPackageDirectory);

            AssemblyFileName = String.Concat(PackageName, ".dll");

            PlugInDirectory = Path.Combine(BinariesDirectory, PlugInId.ToString());
            PlugInDirectoryInvalid = Path.Combine(BinariesDirectory, PlugInIdInvalid.ToString());

            AssemblyFilePath = Path.Combine(PlugInDirectory, AssemblyFileName);
            AssemblyFilePathInvalid = Path.Combine(PlugInDirectoryInvalid, String.Concat(PackageNameInvalid, ".dll"));

            PlugInDirectory.CreateDirectory();
            PlugInDirectoryInvalid.CreateDirectory();

            ZipFile.ExtractToDirectory(PackageFilePath, PlugInDirectory, true);
            ZipFile.ExtractToDirectory(PackageFileInvalidPath, PlugInDirectoryInvalid, true);
        }

        public string AssemblyFileName { get; }
        public string AssemblyFilePath { get; }
        public string AssemblyFilePathInvalid { get; }
        public string BaseTestDirectory { get; }
        public string BinariesDirectory { get; }
        public string PackageDirectory { get; }
        public string PackageFile { get; }
        public string PackageFileInvalid { get; }
        public string PackageFileInvalidPath { get; }
        public string PackageFilePath { get; }
        public string PackageName { get; }
        public string PackageNameInvalid { get; }
        public string PlugInDirectory { get; }
        public string PlugInDirectoryInvalid { get; }
        public int PlugInId { get; }
        public int PlugInIdInvalid { get; }

        public virtual void Dispose()
        {
            BaseTestDirectory.DeleteDirectory();
        }

        private static void CopyPackageFile(
            string packageName,
            string packageFileName,
            string testProjectName,
            string assemblyDirectory,
            string targetPackageDirectory)
        {
            var sourcePackageDirectory = assemblyDirectory!.Replace(testProjectName, packageName).Replace("net8.0", string.Empty);
            var sourcePackageFilePath = Path.Combine(sourcePackageDirectory, packageFileName);
            var targetPackageFilePath = Path.Combine(targetPackageDirectory, packageFileName);

            File.Copy(sourcePackageFilePath, targetPackageFilePath, true);
        }
    }
}