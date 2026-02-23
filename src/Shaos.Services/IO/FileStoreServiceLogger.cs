using Microsoft.Extensions.Logging;

namespace Shaos.Services.IO
{
    internal static partial class FileStoreServiceLogger
    {
        [LoggerMessage(Level = LogLevel.Information,
            Message = "Creating package directory [{path}]")]
        public static partial void LogCreatingPackageDirectory(this ILogger<FileStoreService> logger,
                                                               string path);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Creating packages directory [{path}]")]
        public static partial void LogCreatingPackagesDirectory(this ILogger<FileStoreService> logger,
                                                                string path);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Deleting directory [{path}]")]
        public static partial void LogDeletingDirectory(this ILogger<FileStoreService> logger,
                                                        string path);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Emptying directory [{path}]")]
        public static partial void LogEmptyDirectory(this ILogger<FileStoreService> logger,
                                                     string path);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Emptying package directory [{path}]")]
        public static partial void LogEmptyingPackageDirectory(this ILogger<FileStoreService> logger,
                                                               string path);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Extracting package: [{sourcePath}] to [{targetPath}]")]
        public static partial void LogExtracingPackage(this ILogger<FileStoreService> logger,
                                                       string sourcePath,
                                                       string targetPath);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Writing Package File: [{file}]")]
        public static partial void LogWritingPackageFile(this ILogger<FileStoreService> logger,
                                                         string file);
    }
}