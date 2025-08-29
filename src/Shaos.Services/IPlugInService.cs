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

using Shaos.Repository.Exceptions;
using Shaos.Repository.Models;
using Shaos.Sdk;
using Shaos.Services.Exceptions;
using Shaos.Services.Runtime.Exceptions;
using Shaos.Services.Runtime.Validation;

namespace Shaos.Services
{
    /// <summary>
    /// Defines the PlugIn service interface
    /// </summary>
    public interface IPlugInService
    {
        /// <summary>
        /// Create an instance of a <see cref="PlugIn"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugIn"/> to create the <see cref="PlugInInstance"/></param>
        /// <param name="plugInInstance">The <see cref="PlugInInstance"/> to create</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>The identifier of the created <see cref="PlugInInstance"/></returns>
        /// <exception cref="NotFoundException">Thrown if the <see cref="PlugIn"/> is not found</exception>
        /// <exception cref="NameExistsException">Thrown if a <see cref="PlugInInstance"/> with the same name already exists</exception>
        Task<int> CreatePlugInInstanceAsync(int id,
                                            PlugInInstance plugInInstance,
                                            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a <see cref="PlugIn"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugIn"/> to delete</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <exception cref="PlugInInstanceRunningException">Thrown if a <see cref="PlugInInstance"/> is running</exception>
        Task DeletePlugInAsync(int id,
                               CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a <see cref="PlugInInstance"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugInInstance"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <exception cref="PlugInInstanceRunningException">Thrown if a <see cref="PlugInInstance"/> is running</exception>
        Task DeletePlugInInstanceAsync(int id,
                                       CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packagePath"></param>
        /// <param name="extractedPath"></param>
        void DeletePlugInPackage(string? packagePath, string? extractedPath);

        /// <summary>
        /// Extract <see cref="PackageInformation"/> from an uploaded package
        /// </summary>
        /// <param name="packageFileName">The package file name</param>
        /// <returns>The <see cref="PlugInInformation"/></returns>
        /// <exception cref="NoValidPlugInAssemblyFoundException">Throw if no valid PlugIn assembly file was found</exception>
        /// <exception cref="PlugInTypeNotFoundException">Thrown if no <see cref="IPlugIn"/> derived types where found in the package</exception>
        /// <exception cref="PlugInTypesFoundException">Thrown if multiple <see cref="IPlugIn"/> derived types where found in the unzipped package file</exception>
        PackageInformation ExtractPackageInformation(string packageFileName);

        /// <summary>
        /// Load a <see cref="PlugInInstance"/> configuration
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugInInstance"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <returns>An instance of a <see cref="PlugInInstance"/> configuration</returns>
        Task<object> LoadPlugInInstanceConfigurationAsync(int id,
                                                          CancellationToken cancellationToken = default);

        /// <summary>
        /// Set the <paramref name="enable"/> state of a <see cref="PlugIn"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugInInstance"/></param>
        /// <param name="enable">The enable state to set</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <exception cref="NotFoundException">Thrown if a <see cref="PlugInInstance"/> is not found</exception>
        /// <returns>The updated <see cref="PlugInInstance"/></returns>
        Task<PlugInInstance?> SetPlugInInstanceEnableAsync(int id,
                                                           bool enable,
                                                           CancellationToken cancellationToken = default);

        /// <summary>
        /// Upload the package binaries for a <see cref="PlugIn"/>
        /// </summary>
        /// <param name="id">The identifier of the <see cref="PlugIn"/> to update the NuGet package</param>
        /// <param name="packageFileName">The file name for the <see cref="PlugIn"/></param>
        /// <param name="stream">The <see cref="Stream"/> to write to the <paramref name="packageFileName"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation</param>
        /// <exception cref="ArgumentNullException">Thrown if package file name is null or empty</exception>
        /// <exception cref="PlugInInstanceRunningException">Thrown if a <see cref="PlugInInstance"/> is running</exception>
        /// <exception cref="NoValidPlugInAssemblyFoundException">Throw if no valid PlugIn assembly file was found</exception>
        /// <exception cref="NotFoundException">Thrown if no <see cref="IPlugIn"/> derived types where found in the unzipped package file</exception>
        /// <exception cref="PlugInTypesFoundException">Thrown if multiple <see cref="IPlugIn"/> derived types where found in the unzipped package file</exception>
        Task UploadPlugInPackageAsync(int id,
                                      string packageFileName,
                                      Stream stream,
                                      CancellationToken cancellationToken = default);
    }
}