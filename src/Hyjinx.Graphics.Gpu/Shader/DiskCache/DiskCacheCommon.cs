using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Hyjinx.Graphics.Gpu.Shader.DiskCache
{
    /// <summary>
    /// Common disk cache utility methods.
    /// </summary>
    static partial class DiskCacheCommon
    {
        private static readonly ILogger _logger = Logger.DefaultLoggerFactory.CreateLogger(typeof(DiskCacheCommon));
        
        /// <summary>
        /// Opens a file for read or write.
        /// </summary>
        /// <param name="basePath">Base path of the file (should not include the file name)</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="writable">Indicates if the file will be read or written</param>
        /// <returns>File stream</returns>
        public static FileStream OpenFile(string basePath, string fileName, bool writable)
        {
            string fullPath = Path.Combine(basePath, fileName);

            FileMode mode;
            FileAccess access;

            if (writable)
            {
                mode = FileMode.OpenOrCreate;
                access = FileAccess.ReadWrite;
            }
            else
            {
                mode = FileMode.Open;
                access = FileAccess.Read;
            }

            try
            {
                return new FileStream(fullPath, mode, access, FileShare.Read);
            }
            catch (IOException ioException)
            {
                LogCouldNotAccessFile(_logger, fullPath, ioException);

                throw new DiskCacheLoadException(DiskCacheLoadResult.NoAccess);
            }
        }

        [LoggerMessage(LogLevel.Error,
            EventId = (int)LogClass.Gpu, EventName = nameof(LogClass.Gpu),
            Message = "Could not access file '{path}'")]
        private static partial void LogCouldNotAccessFile(ILogger logger, string path, Exception exception);

        /// <summary>
        /// Gets the compression algorithm that should be used when writing the disk cache.
        /// </summary>
        /// <returns>Compression algorithm</returns>
        public static CompressionAlgorithm GetCompressionAlgorithm()
        {
            return CompressionAlgorithm.Brotli;
        }
    }
}
