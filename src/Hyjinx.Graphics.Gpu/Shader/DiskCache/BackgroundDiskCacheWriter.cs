using Hyjinx.Common;
using Hyjinx.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Hyjinx.Graphics.Gpu.Shader.DiskCache
{
    /// <summary>
    /// Represents a background disk cache writer.
    /// </summary>
    partial class BackgroundDiskCacheWriter : IDisposable
    {
        private readonly ILogger<BackgroundDiskCacheWriter> _logger =
            Logger.DefaultLoggerFactory.CreateLogger<BackgroundDiskCacheWriter>();
        
        /// <summary>
        /// Possible operation to do on the <see cref="_fileWriterWorkerQueue"/>.
        /// </summary>
        private enum CacheFileOperation
        {
            /// <summary>
            /// Operation to add a shader to the cache.
            /// </summary>
            AddShader,
        }

        /// <summary>
        /// Represents an operation to perform on the <see cref="_fileWriterWorkerQueue"/>.
        /// </summary>
        private readonly struct CacheFileOperationTask
        {
            /// <summary>
            /// The type of operation to perform.
            /// </summary>
            public readonly CacheFileOperation Type;

            /// <summary>
            /// The data associated to this operation or null.
            /// </summary>
            public readonly object Data;

            public CacheFileOperationTask(CacheFileOperation type, object data)
            {
                Type = type;
                Data = data;
            }
        }

        /// <summary>
        /// Background shader cache write information.
        /// </summary>
        private readonly struct AddShaderData
        {
            /// <summary>
            /// Cached shader program.
            /// </summary>
            public readonly CachedShaderProgram Program;

            /// <summary>
            /// Binary host code.
            /// </summary>
            public readonly byte[] HostCode;

            /// <summary>
            /// Creates a new background shader cache write information.
            /// </summary>
            /// <param name="program">Cached shader program</param>
            /// <param name="hostCode">Binary host code</param>
            public AddShaderData(CachedShaderProgram program, byte[] hostCode)
            {
                Program = program;
                HostCode = hostCode;
            }
        }

        private readonly GpuContext _context;
        private readonly DiskCacheHostStorage _hostStorage;
        private readonly AsyncWorkQueue<CacheFileOperationTask> _fileWriterWorkerQueue;

        /// <summary>
        /// Creates a new background disk cache writer.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="hostStorage">Disk cache host storage</param>
        public BackgroundDiskCacheWriter(GpuContext context, DiskCacheHostStorage hostStorage)
        {
            _context = context;
            _hostStorage = hostStorage;
            _fileWriterWorkerQueue = new AsyncWorkQueue<CacheFileOperationTask>(ProcessTask, "GPU.BackgroundDiskCacheWriter");
        }

        /// <summary>
        /// Processes a shader cache background operation.
        /// </summary>
        /// <param name="task">Task to process</param>
        private void ProcessTask(CacheFileOperationTask task)
        {
            switch (task.Type)
            {
                case CacheFileOperation.AddShader:
                    AddShaderData data = (AddShaderData)task.Data;
                    try
                    {
                        _hostStorage.AddShader(_context, data.Program, data.HostCode);
                    }
                    catch (Exception ex) when (ex is DiskCacheLoadException or IOException)
                    {
                        LogErrorWritingShaderToCache(ex);
                    }
                    break;
            }
        }

        [LoggerMessage(LogLevel.Error,
            EventId = (int)LogClass.Gpu, EventName = nameof(LogClass.Gpu),
            Message = "Error writing shader to disk cache.")]
        private partial void LogErrorWritingShaderToCache(Exception exception);

        /// <summary>
        /// Adds a shader program to be cached in the background.
        /// </summary>
        /// <param name="program">Shader program to cache</param>
        /// <param name="hostCode">Host binary code of the program</param>
        public void AddShader(CachedShaderProgram program, byte[] hostCode)
        {
            _fileWriterWorkerQueue.Add(new CacheFileOperationTask(CacheFileOperation.AddShader, new AddShaderData(program, hostCode)));
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fileWriterWorkerQueue.Dispose();
            }
        }
    }
}
