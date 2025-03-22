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

using Shaos.Services.Extensions;
using System.Runtime.Loader;

namespace Shaos.Services
{
    public class AssemblyValidationService : IAssemblyValidationService
    {
        public bool ValidateContainsType<T>(string assemblyFile, out string version)
        {
            bool result = false;

            assemblyFile.ThrowIfNullOrEmpty(nameof(assemblyFile));
            version = string.Empty;

            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("Assembly file not found", assemblyFile);
            }

            var assemblyLoadContext = new AssemblyLoadContext("test", true);
            var plugInAssembly = assemblyLoadContext.LoadFromAssemblyPath(assemblyFile);

            if (plugInAssembly != null)
            {
                version = plugInAssembly.GetName().Version!.ToString();

                result = plugInAssembly.GetTypes().Any(t => typeof(T).IsAssignableFrom(t));
            }

            assemblyLoadContext.Unload();
            return result;
        }
    }
}