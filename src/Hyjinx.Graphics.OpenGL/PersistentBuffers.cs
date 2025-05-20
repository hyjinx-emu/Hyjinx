using OpenTK.Graphics.OpenGL;
using Hyjinx.Logging.Abstractions;
using Hyjinx.Graphics.GAL;
using Hyjinx.Graphics.OpenGL.Image;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Hyjinx.Graphics.OpenGL
{
    class PersistentBuffers : IDisposable
    {
        private readonly PersistentBuffer _main = new();
        private readonly PersistentBuffer _background = new();

        private readonly Dictionary<BufferHandle, IntPtr> _maps = new();

        public PersistentBuffer Default => BackgroundContextWorker.InBackground ? _background : _main;

        public void Dispose()
        {
            _main?.Dispose();
            _background?.Dispose();
        }

        public void Map(BufferHandle handle, int size)
        {
            GL.BindBuffer(BufferTarget.CopyWriteBuffer, handle.ToInt32());
            IntPtr ptr = GL.MapBufferRange(BufferTarget.CopyWriteBuffer, IntPtr.Zero, size, BufferAccessMask.MapReadBit | BufferAccessMask.MapPersistentBit);

            _maps[handle] = ptr;
        }

        public void Unmap(BufferHandle handle)
        {
            if (_maps.ContainsKey(handle))
            {
                GL.BindBuffer(BufferTarget.CopyWriteBuffer, handle.ToInt32());
                GL.UnmapBuffer(BufferTarget.CopyWriteBuffer);

                _maps.Remove(handle);
            }
        }

        public bool TryGet(BufferHandle handle, out IntPtr ptr)
        {
            return _maps.TryGetValue(handle, out ptr);
        }
    }

    partial class PersistentBuffer : IDisposable
    {
        private static readonly ILogger<PersistentBuffers> _logger =
            Logger.DefaultLoggerFactory.CreateLogger<PersistentBuffers>();

        private IntPtr _bufferMap;
        private int _copyBufferHandle;
        private int _copyBufferSize;

        private byte[] _data;
        private IntPtr _dataMap;

        private void EnsureBuffer(int requiredSize)
        {
            if (_copyBufferSize < requiredSize && _copyBufferHandle != 0)
            {
                GL.DeleteBuffer(_copyBufferHandle);

                _copyBufferHandle = 0;
            }

            if (_copyBufferHandle == 0)
            {
                _copyBufferHandle = GL.GenBuffer();
                _copyBufferSize = requiredSize;

                GL.BindBuffer(BufferTarget.CopyWriteBuffer, _copyBufferHandle);
                GL.BufferStorage(BufferTarget.CopyWriteBuffer, requiredSize, IntPtr.Zero, BufferStorageFlags.MapReadBit | BufferStorageFlags.MapPersistentBit);

                _bufferMap = GL.MapBufferRange(BufferTarget.CopyWriteBuffer, IntPtr.Zero, requiredSize, BufferAccessMask.MapReadBit | BufferAccessMask.MapPersistentBit);
            }
        }

        public unsafe IntPtr GetHostArray(int requiredSize)
        {
            if (_data == null || _data.Length < requiredSize)
            {
                _data = GC.AllocateUninitializedArray<byte>(requiredSize, true);

                _dataMap = (IntPtr)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(_data));
            }

            return _dataMap;
        }

        private static void Sync()
        {
            GL.MemoryBarrier(MemoryBarrierFlags.ClientMappedBufferBarrierBit);

            IntPtr sync = GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None);
            WaitSyncStatus syncResult = GL.ClientWaitSync(sync, ClientWaitSyncFlags.SyncFlushCommandsBit, 1000000000);

            if (syncResult == WaitSyncStatus.TimeoutExpired)
            {
                LogBufferStateSyncTookTooLong(_logger);
            }

            GL.DeleteSync(sync);
        }

        [LoggerMessage(LogLevel.Error,
            EventId = (int)LogClass.Gpu, EventName = nameof(LogClass.Gpu),
            Message = "Failed to sync persistent buffer state within 1000ms. Continuing...")]
        private static partial void LogBufferStateSyncTookTooLong(ILogger logger);

        public unsafe ReadOnlySpan<byte> GetTextureData(TextureView view, int size)
        {
            EnsureBuffer(size);

            GL.BindBuffer(BufferTarget.PixelPackBuffer, _copyBufferHandle);

            view.WriteToPbo(0, false);

            GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);

            Sync();

            return new ReadOnlySpan<byte>(_bufferMap.ToPointer(), size);
        }

        public unsafe ReadOnlySpan<byte> GetTextureData(TextureView view, int size, int layer, int level)
        {
            EnsureBuffer(size);

            GL.BindBuffer(BufferTarget.PixelPackBuffer, _copyBufferHandle);

            int offset = view.WriteToPbo2D(0, layer, level);

            GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);

            Sync();

            return new ReadOnlySpan<byte>(_bufferMap.ToPointer(), size)[offset..];
        }

        public unsafe ReadOnlySpan<byte> GetBufferData(BufferHandle buffer, int offset, int size)
        {
            EnsureBuffer(size);

            GL.BindBuffer(BufferTarget.CopyReadBuffer, buffer.ToInt32());
            GL.BindBuffer(BufferTarget.CopyWriteBuffer, _copyBufferHandle);

            GL.CopyBufferSubData(BufferTarget.CopyReadBuffer, BufferTarget.CopyWriteBuffer, (IntPtr)offset, IntPtr.Zero, size);

            GL.BindBuffer(BufferTarget.CopyWriteBuffer, 0);

            Sync();

            return new ReadOnlySpan<byte>(_bufferMap.ToPointer(), size);
        }

        public void Dispose()
        {
            if (_copyBufferHandle != 0)
            {
                GL.DeleteBuffer(_copyBufferHandle);
            }
        }
    }
}